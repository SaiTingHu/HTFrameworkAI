using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    public sealed class AssistantWindow : HTFEditorWindow
    {
        public static void OpenWindow(string problem)
        {
            AssistantWindow window = GetWindow<AssistantWindow>();
            window.titleContent.image = EditorGUIUtility.IconContent("SoftlockProjectBrowser Icon").image;
            window.titleContent.text = "Assistant";
            window.LoadSessions();
            if (!string.IsNullOrEmpty(problem))
            {
                window.SelectSession("Unity引擎助手");
                EditorApplication.delayCall += () =>
                {
                    window._userContent = problem;
                    if (!window._isReplying)
                    {
                        window.SendMessage();
                        window.ToSessionScrollBottom();
                    }
                };
            }
            window.Show();
        }

        private List<ChatSession> _sessions = new List<ChatSession>();
        private int _currentSession = -1;
        private bool _isShowThink = false;
        private bool _isLoaded = false;
        private Type _markdownWindowType;

        private StringBuilder _replyBuffer;
        private bool _isReplying = false;
        private string _userContent;
        private string _assistantContent;

        private Texture _assistantIcon;
        private Texture _userIcon;
        private GUIContent _assistantGC;
        private GUIContent _userGC;
        private GUIContent _favoriteGC;
        private GUIContent _settingsGC;
        private GUIContent _promptWordGC;
        private GUIContent _deleteGC;
        private GUIContent _foldGC;
        private GUIContent _noFoldGC;
        private GUIContent _copyGC;
        private GUIContent _markdownGC;
        private GUIStyle _userStyle;
        private GUIStyle _assistantStyle;
        private GUIStyle _dateStyle;
        private Vector2 _sessionListScroll;
        private Vector2 _sessionScroll;
        private Rect _sessionRect;

        protected override bool IsEnableTitleGUI => true;
        protected override string HelpUrl => "https://wanderer.blog.csdn.net/article/details/145637201";

        protected override void OnEnable()
        {
            base.OnEnable();

            _isShowThink = EditorPrefs.GetBool(EditorPrefsTableAI.Assistant_ShowThink, false);
            _markdownWindowType = Type.GetType("HT.ModuleManager.Markdown.MarkdownWindow,HT.ModuleManager");

            _assistantIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/HTFrameworkAI/Editor/Assistant/Texture/AssistantIcon.png");
            _userIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/HTFrameworkAI/Editor/Assistant/Texture/UserIcon.png");
            _assistantGC = new GUIContent();
            _assistantGC.image = _assistantIcon;
            _userGC = new GUIContent();
            _userGC.image = _userIcon;
            _favoriteGC = new GUIContent();
            _favoriteGC.image = EditorGUIUtility.IconContent("d_Favorite Icon").image;
            _favoriteGC.tooltip = "Favorite";
            _settingsGC = new GUIContent();
            _settingsGC.image = EditorGUIUtility.IconContent("d_SettingsIcon").image;
            _settingsGC.tooltip = "Settings";
            _promptWordGC = new GUIContent();
            _promptWordGC.image = EditorGUIUtility.IconContent("Profiler.Memory").image;
            _promptWordGC.tooltip = "设置智能体提示词";
            _deleteGC = new GUIContent();
            _deleteGC.image = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
            _deleteGC.tooltip = "删除此会话";
            _foldGC = new GUIContent();
            _foldGC.image = EditorGUIUtility.IconContent("animationvisibilitytoggleoff").image;
            _foldGC.tooltip = "折叠内容";
            _noFoldGC = new GUIContent();
            _noFoldGC.image = EditorGUIUtility.IconContent("animationvisibilitytoggleon").image;
            _noFoldGC.tooltip = "展开内容";
            _copyGC = new GUIContent();
            _copyGC.image = EditorGUIUtility.IconContent("d_winbtn_win_restore@2x").image;
            _copyGC.tooltip = "复制内容";
            _markdownGC = new GUIContent();
            _markdownGC.image = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow").image;
            _markdownGC.tooltip = "在通用 Markdown 查看窗口中预览内容";

            if (_userStyle == null)
            {
                _userStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
                _userStyle.alignment = TextAnchor.MiddleRight;
            }
            if (_assistantStyle == null)
            {
                _assistantStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
                _assistantStyle.alignment = TextAnchor.MiddleLeft;
            }
            if (_dateStyle == null)
            {
                _dateStyle = new GUIStyle(EditorStyles.boldLabel);
                _dateStyle.alignment = TextAnchor.LowerCenter;
            }
        }
        private void OnDestroy()
        {
            SaveSessions();
        }
        protected override void OnTitleGUI()
        {
            base.OnTitleGUI();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_favoriteGC, EditorStyles.iconButton))
            {
                GenericMenu gm = new GenericMenu();
                gm.AddItem(new GUIContent("DeepSeek"), false, () =>
                {
                    Application.OpenURL("https://chat.deepseek.com/");
                });
                gm.AddItem(new GUIContent("Kimi"), false, () =>
                {
                    Application.OpenURL("https://kimi.moonshot.cn");
                });
                gm.AddItem(new GUIContent("Qwen"), false, () =>
                {
                    Application.OpenURL("https://chat.qwenlm.ai/");
                });
                gm.AddItem(new GUIContent("智谱清言"), false, () =>
                {
                    Application.OpenURL("https://chatglm.cn");
                });
                gm.AddItem(new GUIContent("通义千问"), false, () =>
                {
                    Application.OpenURL("https://tongyi.aliyun.com");
                });
                gm.AddItem(new GUIContent("文心一言"), false, () =>
                {
                    Application.OpenURL("https://yiyan.baidu.com");
                });
                gm.AddItem(new GUIContent("豆包"), false, () =>
                {
                    Application.OpenURL("https://www.doubao.com/chat/");
                });
                gm.ShowAsContext();
            }
            if (GUILayout.Button(_settingsGC, EditorStyles.iconButton))
            {
                AssistantSettingsWindow.OpenWindow(this);
            }
        }
        protected override void OnBodyGUI()
        {
            base.OnBodyGUI();

            EventHandle();

            GUILayout.BeginHorizontal();

            OnSessionListGUI();

            GUILayout.Space(5);

            GUILayout.Box("", "DopesheetBackground", GUILayout.Width(5), GUILayout.ExpandHeight(true));

            GUILayout.Space(5);

            OnSessionGUI();

            GUILayout.EndHorizontal();
        }
        private void OnSessionListGUI()
        {
            GUILayout.BeginVertical(GUILayout.Width(200));

            GUILayout.BeginHorizontal();
            GUI.enabled = _currentSession != -1 && !_isReplying;
            if (GUILayout.Button("开启新会话", EditorGlobalTools.Styles.LargeButton))
            {
                _currentSession = -1;
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            _sessionListScroll = GUILayout.BeginScrollView(_sessionListScroll);

            GUI.enabled = !_isReplying;

            for (int i = 0; i < _sessions.Count; i++)
            {
                if (_currentSession == i) GUILayout.BeginHorizontal("InsertionMarker");
                else GUILayout.BeginHorizontal();
                if (GUILayout.Button(_sessions[i].Name, EditorStyles.label, GUILayout.MinWidth(150), GUILayout.Height(24)))
                {
                    if (_currentSession == i)
                    {
                        _currentSession = -1;
                    }
                    else
                    {
                        _currentSession = i;
                    }
                    GUI.FocusControl(null);
                }
                GUILayout.FlexibleSpace();
                GUI.color = string.IsNullOrEmpty(_sessions[i].PromptWords.Content) ? Color.gray : Color.white;
                if (GUILayout.Button(_promptWordGC, EditorGlobalTools.Styles.InvisibleButton, GUILayout.Width(20), GUILayout.Height(30)))
                {
                    AssistantPromptWordWindow.OpenWindow(this, _sessions[i]);
                }
                GUI.color = Color.white;
                if (GUILayout.Button(_deleteGC, EditorGlobalTools.Styles.InvisibleButton, GUILayout.Width(20), GUILayout.Height(30)))
                {
                    ChatSession chatSession = _sessions[i];
                    if (EditorUtility.DisplayDialog("删除会话", $"是否确认删除会话【{chatSession.Name}】？", "是的", "我再想想"))
                    {
                        DeleteSession(chatSession);
                        GUI.FocusControl(null);
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUI.enabled = true;

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
        private void OnSessionGUI()
        {
            GUILayout.BeginVertical();

            if (_currentSession != -1)
            {
                ChatSession chatSession = _sessions[_currentSession];

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(chatSession.Name, "WarningOverlay", GUILayout.ExpandWidth(true)))
                {
                    if (!_isReplying)
                    {
                        GenericMenu gm = new GenericMenu();
                        gm.AddItem(new GUIContent("重命名"), false, () =>
                        {
                            AssistantRenameWindow.OpenWindow(this, chatSession);
                        });
                        gm.AddItem(new GUIContent("清空记录"), false, () =>
                        {
                            chatSession.Messages.Clear();
                        });
                        gm.ShowAsContext();
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.color = Color.gray;
                GUILayout.Label($"共{chatSession.Messages.Count}条记录", GUILayout.ExpandWidth(true));
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                _sessionScroll = GUILayout.BeginScrollView(_sessionScroll);
                for (int i = 0; i < chatSession.Messages.Count; i++)
                {
                    ChatSession.ChatMessage message = chatSession.Messages[i];
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
                _sessionRect = chatSession.Messages.Count > 0 ? GUILayoutUtility.GetLastRect() : Rect.zero;
                OnReplyingMessageGUI();
                GUILayout.EndScrollView();

                GUILayout.FlexibleSpace();
            }
            else
            {
                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("新的会话", "WarningOverlay", GUILayout.Width(100));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("我是你的个人AI助手，很高兴见到你！", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("我可以帮你写代码、读文件、写作各种创意内容，请把你的任务交给我吧~");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUI.color = Color.yellow;
            GUILayout.Label("给AI助手发送消息：");
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _userContent = EditorGUILayout.TextArea(_userContent, GUILayout.MinHeight(40));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = !string.IsNullOrEmpty(_userContent) && !_isReplying;
            if (GUILayout.Button("发送消息", EditorGlobalTools.Styles.LargeButton))
            {
                SendMessage();
                ToSessionScrollBottom();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        private void OnUserMessageGUI(ChatSession.ChatMessage message)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.color = Color.gray;
            GUILayout.Label(message.Date, _dateStyle, GUILayout.Height(40));
            GUI.color = Color.white;
            GUILayout.Label(_userGC, GUILayout.Width(40), GUILayout.Height(40));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("textarea");
            EditorGUILayout.TextArea(message.Content, _userStyle);
            GUILayout.EndHorizontal();
        }
        private void OnAssistantMessageGUI(ChatSession.ChatMessage message)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(_assistantGC, GUILayout.Width(40), GUILayout.Height(40));
            GUI.color = Color.gray;
            GUILayout.Label(message.Date, _dateStyle, GUILayout.Height(40));
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("textarea");

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(message.IsFold ? _noFoldGC : _foldGC, EditorGlobalTools.Styles.IconButton, GUILayout.Width(20), GUILayout.Height(20)))
            {
                message.IsFold = !message.IsFold;
            }
            if (GUILayout.Button(_copyGC, EditorGlobalTools.Styles.IconButton, GUILayout.Width(20), GUILayout.Height(20)))
            {
                GUIUtility.systemCopyBuffer = message.Content;
            }
            if (GUILayout.Button(_markdownGC, EditorGlobalTools.Styles.IconButton, GUILayout.Width(20), GUILayout.Height(20)))
            {
                if (_markdownWindowType != null)
                {
                    MethodInfo openWindow = _markdownWindowType.GetMethod("OpenWindowOfContent", BindingFlags.Static | BindingFlags.Public);
                    openWindow.Invoke(null, new object[] { message.Content });
                }
                else
                {
                    Log.Error("未找到 HTModuleManager 模块，不支持在通用 Markdown 查看窗口中预览内容。");
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (message.IsFold)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("内容已折叠......", _assistantStyle);
                GUILayout.EndHorizontal();
            }
            else
            {
                if (_isShowThink && !string.IsNullOrEmpty(message.Think))
                {
                    GUILayout.BeginHorizontal();
                    GUI.color = Color.gray;
                    EditorGUILayout.LabelField(message.Think, _assistantStyle);
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                EditorGUILayout.TextArea(message.Content, _assistantStyle);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
        private void OnReplyingMessageGUI()
        {
            if (_isReplying)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_assistantGC, GUILayout.Width(40), GUILayout.Height(40));
                if (!string.IsNullOrEmpty(_assistantContent))
                {
                    GUI.color = Color.gray;
                    GUILayout.Label("正在输入......", _dateStyle, GUILayout.Height(40));
                    GUI.color = Color.white;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("textarea");
                EditorGUILayout.LabelField(string.IsNullOrEmpty(_assistantContent) ? "思考中......" : _assistantContent, _assistantStyle);
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
        /// 从文件加载会话
        /// </summary>
        private void LoadSessions()
        {
            if (_isLoaded)
                return;

            _isLoaded = true;

            string rootPath = GetSavePath();
            DirectoryInfo directoryInfo = new DirectoryInfo(rootPath);
            FileSystemInfo[] infos = directoryInfo.GetFileSystemInfos();
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i] is FileInfo && infos[i].Extension == ".session")
                {
                    string content = File.ReadAllText(infos[i].FullName);
                    ChatSession chatSession = JsonToolkit.StringToJson<ChatSession>(content);
                    if (chatSession != null) _sessions.Add(chatSession);
                }
            }

            if (!_sessions.Exists((s) => { return s.Name == "Unity引擎助手"; }))
            {
                CreateSession("Unity引擎助手");
            }
        }
        /// <summary>
        /// 保存会话到文件中
        /// </summary>
        private void SaveSessions()
        {
            string rootPath = GetSavePath();
            for (int i = 0; i < _sessions.Count; i++)
            {
                _sessions[i].Data.Prompt = "";
                string content = JsonToolkit.JsonToString(_sessions[i]);
                File.WriteAllText($"{rootPath}{_sessions[i].ID}.session", content);
            }
        }
        /// <summary>
        /// 获取会话数据存储路径
        /// </summary>
        private string GetSavePath()
        {
            string savePath = null;
            AssistantSessionSavePath savePathType = (AssistantSessionSavePath)EditorPrefs.GetInt(EditorPrefsTableAI.Assistant_SavePath, 0);
            if (savePathType == AssistantSessionSavePath.LocalAppData)
            {
                savePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\UnityAssistant\\";
            }
            else if (savePathType == AssistantSessionSavePath.Library)
            {
                savePath = PathToolkit.ProjectPath + "Library/Assistant/";
            }
            else
            {
                savePath = PathToolkit.ProjectPath + "Library/Assistant/";
            }

            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

            return savePath;
        }

        /// <summary>
        /// 创建新的会话
        /// </summary>
        private void CreateSession(string sessionName)
        {
            string model = EditorPrefs.GetString(EditorPrefsTableAI.Assistant_Model, "deepseek-coder-v2:16b");
            bool stream = EditorPrefs.GetBool(EditorPrefsTableAI.Assistant_Stream, true);
            string baseAddress = EditorPrefs.GetString(EditorPrefsTableAI.Assistant_BaseAddress, "http://localhost:11434");
            string api = EditorPrefs.GetString(EditorPrefsTableAI.Assistant_API, "/api/generate");
            int timeout = EditorPrefs.GetInt(EditorPrefsTableAI.Assistant_Timeout, 60);
            int round = EditorPrefs.GetInt(EditorPrefsTableAI.Assistant_Round, 7);
            bool isLogInEditor = EditorPrefs.GetBool(EditorPrefsTableAI.Assistant_IsLogInEditor, false);

            ChatSession chatSession = new ChatSession(Guid.NewGuid().ToString("N").ToUpper(), sessionName);
            chatSession.Data.Model = model;
            chatSession.Data.Stream = stream;
            chatSession.BaseAddress = baseAddress;
            chatSession.API = api;
            chatSession.Timeout = timeout;
            chatSession.Round = round;
            chatSession.IsLogInEditor = isLogInEditor;

            _sessions.Add(chatSession);
            _currentSession = _sessions.Count - 1;
        }
        /// <summary>
        /// 删除会话
        /// </summary>
        private void DeleteSession(ChatSession chatSession)
        {
            _sessions.Remove(chatSession);
            _currentSession = -1;

            string rootPath = GetSavePath();
            string path = $"{rootPath}{chatSession.ID}.session";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        /// <summary>
        /// 通过名称选中指定的会话
        /// </summary>
        private void SelectSession(string sessionName)
        {
            ChatSession chatSession = _sessions.Find((s) => { return s.Name == sessionName; });
            if (chatSession != null) _currentSession = _sessions.IndexOf(chatSession);
        }

        /// <summary>
        /// 应用设置项
        /// </summary>
        internal void ApplySettings()
        {
            string model = EditorPrefs.GetString(EditorPrefsTableAI.Assistant_Model, "deepseek-coder-v2:16b");
            bool stream = EditorPrefs.GetBool(EditorPrefsTableAI.Assistant_Stream, true);
            string baseAddress = EditorPrefs.GetString(EditorPrefsTableAI.Assistant_BaseAddress, "http://localhost:11434");
            string api = EditorPrefs.GetString(EditorPrefsTableAI.Assistant_API, "/api/generate");
            int timeout = EditorPrefs.GetInt(EditorPrefsTableAI.Assistant_Timeout, 60);
            int round = EditorPrefs.GetInt(EditorPrefsTableAI.Assistant_Round, 7);
            bool isLogInEditor = EditorPrefs.GetBool(EditorPrefsTableAI.Assistant_IsLogInEditor, false);
            _isShowThink = EditorPrefs.GetBool(EditorPrefsTableAI.Assistant_ShowThink, false);

            for (int i = 0; i < _sessions.Count; i++)
            {
                _sessions[i].Data.Model = model;
                _sessions[i].Data.Stream = stream;
                _sessions[i].BaseAddress = baseAddress;
                _sessions[i].API = api;
                _sessions[i].Timeout = timeout;
                _sessions[i].Round = round;
                _sessions[i].IsLogInEditor = isLogInEditor;
            }
        }
        /// <summary>
        /// 发送消息（当前会话）
        /// </summary>
        private void SendMessage()
        {
            if (_currentSession == -1)
                CreateSession(_userContent);

            if (_replyBuffer == null) _replyBuffer = new StringBuilder();
            else _replyBuffer.Clear();

            _isReplying = true;
            _assistantContent = null;

            _sessions[_currentSession].UserSpeak(_userContent,
            (reply) =>
            {
                if (reply == "<think>") _replyBuffer.Append("<开始推理>：");
                else if (reply == "</think>") _replyBuffer.Append("<推理完成>");
                else _replyBuffer.Append(reply);
                _assistantContent = _replyBuffer.ToString();
                Repaint();
            },
            (success) =>
            {
                _isReplying = false;

                _replyBuffer.Clear();
                _assistantContent = null;
                Focus();
                Repaint();
            });
            _userContent = null;
        }
    }
}