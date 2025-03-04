using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static HT.Framework.AI.ChatSession;

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
        private List<ChatMessage> _messages = new List<ChatMessage>();
        private bool _isReplying = false;
        private string _userCode;
        private string _userContent;

        private Texture _aiButlerIcon;
        private GUIContent _aiButlerGC;
        private GUIContent _codeGC;
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
            _codeGC.tooltip = "上传代码";
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
                ChatMessage message = _messages[i];
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
        private void OnUserMessageGUI(ChatMessage message)
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
        private void OnAssistantMessageGUI(ChatMessage message)
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

            _messages.Add(new ChatMessage { Role = "user", Think = null, Content = _userContent, Date = DateTime.Now.ToDefaultDateString() });
            ButlerAgent.SendMessage(_userContent, _userCode, (reply) =>
            {
                _isReplying = false;

                if (!string.IsNullOrEmpty(reply))
                {
                    _messages.Add(new ChatMessage { Role = "assistant", Think = null, Content = reply, Date = DateTime.Now.ToDefaultDateString() });
                }

                Focus();
                Repaint();
            });
            _userContent = null;
            _userCode = null;
        }
    }
}