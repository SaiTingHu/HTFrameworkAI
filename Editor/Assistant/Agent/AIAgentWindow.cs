using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    internal sealed class AIAgentWindow : HTFEditorWindow
    {
        public static void OpenWindow(AssistantWindow assistantWindow)
        {
            AIAgentWindow window = GetWindow<AIAgentWindow>();
            window.titleContent.image = EditorGUIUtility.IconContent("ParticleSystemForceField Icon").image;
            window.titleContent.text = "AI Agent";
            window._assistantWindow = assistantWindow;
            window.Show();
        }

        private AssistantWindow _assistantWindow;
        private AIAgent _agent;
        private List<Message> _messages = new List<Message>();
        private bool _isThinking = false;
        private bool _isReplying = false;
        private string _userCode;
        private string _userFolderPath;
        private string _userContent;
        private string _userAt = "@";

        private Texture _aiAgentIcon;
        private GUIContent _aiAgentGC;
        private GUIContent _codeGC;
        private GUIContent _folderGC;
        private Vector2 _sessionScroll;
        private Rect _sessionRect;

        /// <summary>
        /// AI智能体
        /// </summary>
        private AIAgent Agent
        {
            get
            {
                if (_agent == null)
                {
                    string agent = EditorPrefs.GetString(EditorPrefsTableAI.AIAgent_Type, "<None>");
                    Type type = (agent == "<None>") ? null : ReflectionToolkit.GetTypeInAllAssemblies(agent, false);
                    if (type != null)
                    {
                        _agent = Activator.CreateInstance(type) as AIAgent;
                        _agent.InitAgent();
                    }
                }
                return _agent;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _aiAgentIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/HTFrameworkAI/Editor/Assistant/Texture/AIAgentIcon.png");
            _aiAgentGC = new GUIContent();
            _aiAgentGC.image = _aiAgentIcon;
            _codeGC = new GUIContent();
            _codeGC.image = EditorGUIUtility.IconContent("d_cs Script Icon").image;
            _folderGC = new GUIContent();
            _folderGC.image = EditorGUIUtility.IconContent("Folder Icon").image;
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
                AIAgentSettingsWindow.OpenWindow(_assistantWindow);
            }
        }
        protected override void OnBodyGUI()
        {
            base.OnBodyGUI();

            if (Agent != null)
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
            GUILayout.Label(Agent.Name, "WarningOverlay", GUILayout.ExpandWidth(true));
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
            OnThinkingMessageGUI();
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUI.color = Color.yellow;
            GUILayout.Label($"给【{Agent.Name}】发送指令：");
            GUI.color = _userAt != "@" ? Color.yellow : Color.gray;
            if (GUILayout.Button(_userAt, EditorStyles.popup))
            {
                GenericMenu gm = new GenericMenu();
                for (int i = 0; i < Agent.Ats.Length; i++)
                {
                    string at = Agent.Ats[i];
                    gm.AddItem(new GUIContent(at), at == _userAt, () =>
                    {
                        _userAt = at;
                    });
                }
                gm.AddSeparator("");
                gm.AddItem(new GUIContent("<None>"), "@" == _userAt, () =>
                {
                    _userAt = "@";
                });
                gm.ShowAsContext();
            }
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

            GUI.color = string.IsNullOrEmpty(_userFolderPath) ? Color.gray : Color.white;
            _folderGC.tooltip = string.IsNullOrEmpty(_userFolderPath) ? "选择文件夹" : $"选择文件夹（已选择：{_userFolderPath}）";
            if (GUILayout.Button(_folderGC, EditorStyles.iconButton))
            {
                string path = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    _userFolderPath = "Assets" + path.Replace(Application.dataPath, "");
                    GUI.FocusControl(null);
                }
            }
            GUI.color = Color.white;

            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            if (_isReplying)
            {
                GUILayout.BeginHorizontal();
                GUI.enabled = false;
                EditorGUILayout.TextArea($"{Agent.Name} 正在处理指令，请稍后......", GUILayout.MinHeight(40));
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal("textarea");
                _userContent = EditorGUILayout.TextArea(_userContent, _assistantWindow._userInputStyle, GUILayout.MinHeight(40));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = !string.IsNullOrEmpty(_userContent) && !_isReplying;
            if (GUILayout.Button("发送指令", EditorGlobalTools.Styles.LargeButton))
            {
                SendInstruction();
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
            GUILayout.Label("当前无法启用智能体，请设置有效的【智能体类型】。", EditorStyles.largeLabel);
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
            GUILayout.Label(_aiAgentGC, GUILayout.Width(40), GUILayout.Height(40));
            GUI.color = Color.gray;
            GUILayout.Label(message.Date, _assistantWindow._dateStyle, GUILayout.Height(40));
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("textarea");
            EditorGUILayout.TextArea(message.Content, _assistantWindow._assistantStyle);
            GUILayout.EndHorizontal();
        }
        private void OnThinkingMessageGUI()
        {
            if (_isThinking)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_aiAgentGC, GUILayout.Width(40), GUILayout.Height(40));
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
                                SendInstruction();
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
        /// 发送指令
        /// </summary>
        private void SendInstruction()
        {
            _isThinking = true;
            _isReplying = true;

            string at = _userAt != "@" ? $" <color=yellow>{_userAt}</color>" : "";
            _messages.Add(new Message { Role = "user", Content = _userContent + at, Date = DateTime.Now.ToDefaultDateString(), Code = _userCode, FolderPath = _userFolderPath });
            Agent.SendInstruction(_userContent, _userCode, _userFolderPath, _userAt, (done, reply) =>
            {
                _isThinking = false;

                if (done)
                {
                    _isReplying = false;
                }

                if (!string.IsNullOrEmpty(reply))
                {
                    _messages.Add(new Message { Role = "assistant", Content = reply, Date = DateTime.Now.ToDefaultDateString(), Code = null, FolderPath = null });
                    ToSessionScrollBottom();
                }

                Focus();
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