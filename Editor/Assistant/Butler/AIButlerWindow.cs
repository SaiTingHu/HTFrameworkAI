using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    internal sealed class AIButlerWindow : HTFEditorWindow
    {
        public static void OpenWindow(AssistantWindow assistantWindow)
        {
            AIButlerWindow window = GetWindow<AIButlerWindow>();
            window.titleContent.image = EditorGUIUtility.IconContent("ParticleSystemForceField Icon").image;
            window.titleContent.text = "AI Butler";
            window._assistantWindow = assistantWindow;
            window.Show();
        }

        private AssistantWindow _assistantWindow;
        private AIButlerAgent _butlerAgent;
        private List<Message> _messages = new List<Message>();
        private bool _isReplying = false;
        private string _userCode;
        private string _userFolderPath;
        private string _userContent;

        private Texture _aiButlerIcon;
        private GUIContent _aiButlerGC;
        private GUIContent _codeGC;
        private GUIContent _folderGC;
        private GUIContent _noneFolderGC;
        private Vector2 _sessionScroll;
        private Rect _sessionRect;

        /// <summary>
        /// 智能管家行为代理
        /// </summary>
        private AIButlerAgent ButlerAgent
        {
            get
            {
                if (_butlerAgent == null)
                {
                    string agent = EditorPrefs.GetString(EditorPrefsTableAI.AIButler_Agent, "<None>");
                    Type type = (agent == "<None>") ? null : ReflectionToolkit.GetTypeInAllAssemblies(agent, false);
                    if (type != null)
                    {
                        _butlerAgent = Activator.CreateInstance(type) as AIButlerAgent;
                        _butlerAgent.InitAgent();
                    }
                }
                return _butlerAgent;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _aiButlerIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/HTFrameworkAI/Editor/Assistant/Texture/AIButlerIcon.png");
            _aiButlerGC = new GUIContent();
            _aiButlerGC.image = _aiButlerIcon;
            _codeGC = new GUIContent();
            _codeGC.image = EditorGUIUtility.IconContent("d_cs Script Icon").image;
            _folderGC = new GUIContent();
            _folderGC.image = EditorGUIUtility.IconContent("Folder Icon").image;
            _noneFolderGC = new GUIContent();
            _noneFolderGC.image = EditorGUIUtility.IconContent("FolderEmpty Icon").image;
        }
        private void Update()
        {
            if (_assistantWindow == null)
            {
                Close();
            }
        }
        protected override void OnTitleGUI()
        {
            base.OnTitleGUI();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_assistantWindow._settingsGC, EditorStyles.iconButton))
            {
                AIButlerSettingsWindow.OpenWindow(_assistantWindow);
            }
        }
        protected override void OnBodyGUI()
        {
            base.OnBodyGUI();

            if (ButlerAgent != null)
            {
                EventHandle();
                OnSessionGUI();
            }
            else
            {
                OnErrorGUI();
            }
        }
        private void OnSessionGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(ButlerAgent.Name, "WarningOverlay", GUILayout.ExpandWidth(true));
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
            OnReplyingMessageGUI();
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUI.color = Color.yellow;
            GUILayout.Label($"给 {ButlerAgent.Name} 发送指令：");
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();

            GUI.color = string.IsNullOrEmpty(_userCode) ? Color.gray : Color.white;
            _codeGC.tooltip = string.IsNullOrEmpty(_userCode) ? "上传代码" : "上传代码（已上传）";
            if (GUILayout.Button(_codeGC, EditorStyles.iconButton))
            {
                StringValueEditor.OpenWindow(this, _userCode, "上传代码", (str) =>
                {
                    _userCode = string.IsNullOrEmpty(str) ? null : str.Trim();
                });
            }
            GUI.color = Color.white;

            GUIContent gc = string.IsNullOrEmpty(_userFolderPath) ? _noneFolderGC : _folderGC;
            gc.tooltip = string.IsNullOrEmpty(_userFolderPath) ? "选择文件夹" : $"选择文件夹（已选择：{_userFolderPath}）";
            if (GUILayout.Button(gc, EditorStyles.iconButton))
            {
                string path = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    _userFolderPath = "Assets" + path.Replace(Application.dataPath, "");
                    GUI.FocusControl(null);
                }
            }

            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _userContent = EditorGUILayout.TextArea(_userContent, GUILayout.MinHeight(40));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = !string.IsNullOrEmpty(_userContent) && !_isReplying;
            if (GUILayout.Button("发送指令", EditorGlobalTools.Styles.LargeButton))
            {
                SendMessage();
                ToSessionScrollBottom();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        private void OnErrorGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            GUI.color = Color.yellow;
            GUILayout.Label("当前无法启用智能管家，请设置【智能管家代理】。", EditorStyles.largeLabel);
            GUI.color = Color.white;

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

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
            if (!string.IsNullOrEmpty(message.Code))
            {
                _codeGC.tooltip = "上传了代码";
                if (GUILayout.Button(_codeGC, EditorStyles.iconButton))
                {
                    StringValueEditor.OpenWindow(this, message.Code, "已上传的代码", null);
                }
            }
            if (!string.IsNullOrEmpty(message.FolderPath))
            {
                _folderGC.tooltip = "选择了文件夹";
                if (GUILayout.Button(_folderGC, EditorStyles.iconButton))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(message.FolderPath);
                }
            }
            GUILayout.EndHorizontal();
        }
        private void OnAssistantMessageGUI(Message message)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(_aiButlerGC, GUILayout.Width(40), GUILayout.Height(40));
            GUI.color = Color.gray;
            GUILayout.Label(message.Date, _assistantWindow._dateStyle, GUILayout.Height(40));
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("textarea");
            EditorGUILayout.TextArea(message.Content, _assistantWindow._assistantStyle);
            GUILayout.EndHorizontal();
        }
        private void OnReplyingMessageGUI()
        {
            if (_isReplying)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_aiButlerGC, GUILayout.Width(40), GUILayout.Height(40));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("textarea");
                EditorGUILayout.LabelField("思考中......", _assistantWindow._assistantStyle);
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
                            if (!string.IsNullOrEmpty(_userContent) && !_isReplying)
                            {
                                SendMessage();
                                EditorApplication.delayCall += ToSessionScrollBottom;
                                GUI.FocusControl(null);
                            }
                            break;
                    }
                    break;
            }
        }
        private void ToSessionScrollBottom()
        {
            _sessionScroll = _sessionRect.position;
            Repaint();
        }

        /// <summary>
        /// 发送消息（智能管家）
        /// </summary>
        private void SendMessage()
        {
            _isReplying = true;

            _messages.Add(new Message { Role = "user", Content = _userContent, Date = DateTime.Now.ToDefaultDateString(), Code = _userCode, FolderPath = _userFolderPath });
            ButlerAgent.SendMessage(_userContent, _userCode, _userFolderPath, (reply) =>
            {
                _isReplying = false;

                if (!string.IsNullOrEmpty(reply))
                {
                    _messages.Add(new Message { Role = "assistant", Content = reply, Date = DateTime.Now.ToDefaultDateString(), Code = null, FolderPath = null });
                }

                Focus();
                Repaint();
            });
            _userContent = null;
            _userCode = null;
            _userFolderPath = null;
        }

        [Serializable]
        private class Message
        {
            public string Role;
            public string Content;
            public string Date;
            public string Code;
            public string FolderPath;
        }
    }
}