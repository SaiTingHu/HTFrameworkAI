using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    internal sealed class ImageGenerationWindow : HTFEditorWindow
    {
        public static void OpenWindow(AssistantWindow assistantWindow)
        {
            ImageGenerationWindow window = GetWindow<ImageGenerationWindow>();
            window.titleContent.image = EditorGUIUtility.IconContent("d_RawImage Icon").image;
            window.titleContent.text = "AI Draw";
            window._assistantWindow = assistantWindow;
            window.Show();
        }

        private AssistantWindow _assistantWindow;
        private List<Message> _messages = new List<Message>();
        private List<Seed> _seeds = new List<Seed>();
        private bool _isDrawing = false;
        private string _prompt;
        private int _width = 512;
        private int _height = 512;

        private GUIContent _redrawGC;
        private GUIContent _saveGC;
        private Vector2 _sessionScroll;
        private Rect _sessionRect;

        protected override void OnEnable()
        {
            base.OnEnable();

            _redrawGC = new GUIContent();
            _redrawGC.image = EditorGUIUtility.IconContent("d_editicon.sml").image;
            _redrawGC.tooltip = "重新画一张";
            _saveGC = new GUIContent();
            _saveGC.image = EditorGUIUtility.IconContent("d_SaveAs").image;
            _saveGC.tooltip = "保存图像";
        }
        private void Update()
        {
            if (_assistantWindow == null)
            {
                Close();
            }
        }
        private void OnDestroy()
        {
            for (int i = 0; i < _messages.Count; i++)
            {
                if (_messages[i].Image != null)
                {
                    Main.KillImmediate(_messages[i].Image);
                }
            }
            _messages.Clear();
            _seeds.Clear();
        }
        protected override void OnBodyGUI()
        {
            base.OnBodyGUI();

            EventHandle();
            OnSessionGUI();
        }
        private void OnSessionGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("AI画图", "WarningOverlay", GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.color = Color.gray;
            GUILayout.Label($"共{_messages.Count}条记录", GUILayout.ExpandWidth(true));
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            _sessionScroll = GUILayout.BeginScrollView(_sessionScroll);
            for (int i = 0; i < _messages.Count; i++)
            {
                Message message = _messages[i];
                if (message.Role == "user")
                {
                    OnUserMessageGUI(message);
                }
                else
                {
                    OnAssistantMessageGUI(message);
                }
                GUILayout.Space(5);
            }
            _sessionRect = _messages.Count > 0 ? GUILayoutUtility.GetLastRect() : Rect.zero;
            OnDrawingMessageGUI();
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUI.color = Color.yellow;
            GUILayout.Label("给【AI画图】发送图像描述：");
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            _width = EditorGUILayout.IntField(_width, GUILayout.Width(50));
            GUILayout.Label("x", GUILayout.Width(10));
            _height = EditorGUILayout.IntField(_height, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            
            if (_isDrawing)
            {
                GUILayout.BeginHorizontal();
                GUI.enabled = false;
                EditorGUILayout.TextArea("正在画图中，请稍后......", GUILayout.MinHeight(40));
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.MinHeight(40));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = !string.IsNullOrEmpty(_prompt) && !_isDrawing;
            if (GUILayout.Button("开始画图", EditorGlobalTools.Styles.LargeButton))
            {
                SendImageGeneration();
                ToSessionScrollBottom();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }
        private void OnUserMessageGUI(Message message)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.color = Color.gray;
            GUILayout.Label(message.Date, _assistantWindow._dateStyle, GUILayout.Height(40));
            GUI.color = Color.white;
            GUILayout.Label(_assistantWindow._userGC, GUILayout.Width(40), GUILayout.Height(40));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("textarea");
            EditorGUILayout.TextArea(message.Content, _assistantWindow._userStyle);
            GUILayout.EndHorizontal();
        }
        private void OnAssistantMessageGUI(Message message)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(_assistantWindow._assistantGC, GUILayout.Width(40), GUILayout.Height(40));
            GUI.color = Color.gray;
            GUILayout.Label(message.Date, _assistantWindow._dateStyle, GUILayout.Height(40));
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();

            GUILayout.Space(2);

            if (message.Image)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                GUI.enabled = !_isDrawing;
                if (GUILayout.Button(_redrawGC, EditorGlobalTools.Styles.IconButton, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    _prompt = message.Content;
                    SendImageGeneration();
                    ToSessionScrollBottom();
                }
                if (GUILayout.Button(_saveGC, EditorGlobalTools.Styles.IconButton, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    string path = EditorUtility.SaveFilePanel("保存图像", Application.dataPath, "", "png");
                    if (!string.IsNullOrEmpty(path))
                    {
                        File.WriteAllBytes(path, message.Image.EncodeToPNG());
                        if (path.StartsWith(Application.dataPath)) AssetDatabase.Refresh();
                    }
                }
                GUI.enabled = true;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                int width = (int)Mathf.Min(message.Image.width, position.width - 10);
                int height = (int)((float)width / message.Image.width * message.Image.height);
                GUILayout.Label(message.Image, GUILayout.Width(width), GUILayout.Height(height));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
        private void OnDrawingMessageGUI()
        {
            if (_isDrawing)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_assistantWindow._assistantGC, GUILayout.Width(40), GUILayout.Height(40));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("textarea");
                EditorGUILayout.LabelField("画图中......", _assistantWindow._assistantStyle);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
        }
        private void EventHandle()
        {
            if (Event.current == null)
                return;

            switch (Event.current.rawType)
            {
                case EventType.KeyDown:
                    switch (Event.current.keyCode)
                    {
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            if (!string.IsNullOrEmpty(_prompt) && !_isDrawing)
                            {
                                SendImageGeneration();
                                ToSessionScrollBottom();
                                GUI.FocusControl(null);
                            }
                            break;
                    }
                    break;
            }
        }
        private void ToSessionScrollBottom()
        {
            EditorApplication.delayCall += () =>
            {
                _sessionScroll = _sessionRect.position;
                Repaint();
            };
        }

        /// <summary>
        /// 发送生成图像请求
        /// </summary>
        private async void SendImageGeneration()
        {
            _isDrawing = true;

            _messages.Add(new Message { Role = "user", Content = _prompt, Date = DateTime.Now.ToDefaultDateString(), Image = null });
            byte[] bytes = await PollinationsAI.ImageGeneration(_prompt, _width, _height, GetSeed(_prompt));

            if (bytes != null)
            {
                Texture2D texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
                texture.LoadImage(bytes);
                _messages.Add(new Message { Role = "assistant", Content = _prompt, Date = DateTime.Now.ToDefaultDateString(), Image = texture });
                ToSessionScrollBottom();
            }

            Focus();

            _isDrawing = false;
            _prompt = null;
        }
        /// <summary>
        /// 获取生成种子
        /// </summary>
        /// <param name="prompt">生成图像的提示</param>
        /// <returns>生成种子</returns>
        private int GetSeed(string prompt)
        {
            Seed seed = _seeds.Find((s) => { return s.Prompt == prompt; });
            if (seed == null)
            {
                seed = new Seed()
                {
                    Prompt = prompt,
                    SeedIndex = -1
                };
                _seeds.Add(seed);
            }

            seed.SeedIndex += 1;
            return seed.SeedIndex;
        }

        [Serializable]
        private class Message
        {
            public string Role;
            public string Content;
            public string Date;
            public Texture2D Image;
        }
        [Serializable]
        private class Seed
        {
            public string Prompt;
            public int SeedIndex;
        }
    }
}