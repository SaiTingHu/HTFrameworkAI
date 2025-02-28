using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    internal sealed class AssistantSettingsWindow : HTFEditorWindow
    {
        private const int KEYEVENTF_EXTENDEDKEY = 0x1;
        private const int KEYEVENTF_KEYUP = 0x2;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_V = 0x56;
        private const byte VK_RETURN = 0x0D;
        private static string[] DeepSeekModels = new string[] { 
            "deepseek-r1:8b"
            , "deepseek-r1:14b"
            , "deepseek-r1:32b"
            , "deepseek-r1:70b"
            , "deepseek-r1:671b"
            , "deepseek-coder-v2:16b"
            , "deepseek-coder-v2:236b"
            , "deepseek-v2:16b"
            , "deepseek-v2:236b"
            , "deepseek-v3:671b"
            , "deepseek-llm:7b"
            , "deepseek-llm:67b"
        };
        private static string[] GLMModels = new string[] {
            "glm4:9b"
        };
        private static string[] QwenModels = new string[] {
            "qwen:14b"
            , "qwen2:7b"
            , "qwen2.5:14b"
        };
        private static string[] PhiModels = new string[] {
            "phi3:14b"
            , "phi4:14b"
        };
        private static string[] LlamaModels = new string[] {
            "llama2:13b"
            , "llama3:8b"
            , "llama3.1:8b"
            , "llama3.3:70b"
        };
        private static string[] MistralModels = new string[] {
            "mistral:7b"
            , "mistral-nemo:12b"
        };
        private static string[] GemmaModels = new string[] {
            "gemma:7b"
            , "gemma2:9b"
            , "gemma2:27b"
        };

        public static void OpenWindow(AssistantWindow assistantWindow)
        {
            AssistantSettingsWindow window = GetWindow<AssistantSettingsWindow>();
            window.titleContent.text = "Assistant Settings";
            window._assistantWindow = assistantWindow;
            window.minSize = new Vector2(300, 300);
            window.maxSize = new Vector2(300, 300);
            window.Show();
        }
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private AssistantWindow _assistantWindow;
        private Texture _ollamaIcon;
        private GUIContent _ollamaGC;
        private string _model;
        private bool _stream;
        private string _baseAddress;
        private string _api;
        private int _timeout;
        private int _round;
        private bool _isLogInEditor;
        private bool _isShowThink;
        private AssistantSessionSavePath _savePath;
        private bool _isEnableAIButler;

        protected override void OnEnable()
        {
            base.OnEnable();

            _ollamaIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/HTFrameworkAI/Editor/Assistant/Texture/OllamaIcon.png");
            _ollamaGC = new GUIContent();
            _ollamaGC.image = _ollamaIcon;
            _model = EditorPrefs.GetString(EditorPrefsTableAI.Assistant_Model, "deepseek-coder-v2:16b");
            _stream = EditorPrefs.GetBool(EditorPrefsTableAI.Assistant_Stream, true);
            _baseAddress = EditorPrefs.GetString(EditorPrefsTableAI.Assistant_BaseAddress, "http://localhost:11434");
            _api = EditorPrefs.GetString(EditorPrefsTableAI.Assistant_API, "/api/chat");
            _timeout = EditorPrefs.GetInt(EditorPrefsTableAI.Assistant_Timeout, 60);
            _round = EditorPrefs.GetInt(EditorPrefsTableAI.Assistant_Round, 7);
            _isLogInEditor = EditorPrefs.GetBool(EditorPrefsTableAI.Assistant_IsLogInEditor, false);
            _isShowThink = EditorPrefs.GetBool(EditorPrefsTableAI.Assistant_ShowThink, false);
            _savePath = (AssistantSessionSavePath)EditorPrefs.GetInt(EditorPrefsTableAI.Assistant_SavePath, 0);
            _isEnableAIButler = EditorPrefs.GetBool(EditorPrefsTableAI.Assistant_EnableAIButler, false);
        }
        protected override void OnBodyGUI()
        {
            base.OnBodyGUI();

            GUILayout.BeginHorizontal();
            GUILayout.Label("大模型", GUILayout.Width(100));
            _model = EditorGUILayout.TextField(_model);
            if (GUILayout.Button("选择", EditorStyles.popup))
            {
                GenericMenu gm = new GenericMenu();
                SetGenericMenuItem(gm, "DeepSeek (深度求索)", DeepSeekModels);
                SetGenericMenuItem(gm, "GLM (智谱清言)", GLMModels);
                SetGenericMenuItem(gm, "Qwen (通义千问)", QwenModels);
                SetGenericMenuItem(gm, "Phi (Microsoft)", PhiModels);
                SetGenericMenuItem(gm, "Llama (Meta)", LlamaModels);
                SetGenericMenuItem(gm, "Mistral (Mistral AI)", MistralModels);
                SetGenericMenuItem(gm, "Gemma (Google)", GemmaModels);
                gm.AddSeparator("");
                gm.AddItem(new GUIContent("Other......"), false, () =>
                {
                    Application.OpenURL("https://ollama.com/search");
                });
                gm.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("流式请求", GUILayout.Width(100));
            _stream = EditorGUILayout.Toggle(_stream);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("大模型根地址", GUILayout.Width(100));
            _baseAddress = EditorGUILayout.TextField(_baseAddress);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("API接口", GUILayout.Width(100));
            _api = EditorGUILayout.TextField(_api);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("超时时长（秒）", GUILayout.Width(100));
            _timeout = EditorGUILayout.IntField(_timeout);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("多轮对话最大轮数", GUILayout.Width(100));
            _round = EditorGUILayout.IntField(_round);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("打印相关日志", GUILayout.Width(100));
            _isLogInEditor = EditorGUILayout.Toggle(_isLogInEditor);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("显示推理过程", GUILayout.Width(100));
            _isShowThink = EditorGUILayout.Toggle(_isShowThink);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("数据存储路径", GUILayout.Width(100));
            _savePath = (AssistantSessionSavePath)EditorGUILayout.EnumPopup(_savePath);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.color = _isEnableAIButler ? Color.green : Color.gray;
            GUILayout.Label("启用智能管家", GUILayout.Width(100));
            _isEnableAIButler = EditorGUILayout.Toggle(_isEnableAIButler);
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.yellow;
            _ollamaGC.text = "Run In Ollama";
            if (GUILayout.Button(_ollamaGC, GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Run In Ollama", $"是否确认在 Ollama 中启动大模型 [{_model}] ？如果该模型未下载，将自动下载。", "是的", "我再想想"))
                {
                    RunOllamaInCMD($"ollama run {_model}");
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.yellow;
            _ollamaGC.text = "View Ollama Model List";
            if (GUILayout.Button(_ollamaGC, GUILayout.Height(30)))
            {
                RunOllamaInCMD("ollama list");
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存", "ButtonLeft"))
            {
                EditorPrefs.SetString(EditorPrefsTableAI.Assistant_Model, _model);
                EditorPrefs.SetBool(EditorPrefsTableAI.Assistant_Stream, _stream);
                EditorPrefs.SetString(EditorPrefsTableAI.Assistant_BaseAddress, _baseAddress);
                EditorPrefs.SetString(EditorPrefsTableAI.Assistant_API, _api);
                EditorPrefs.SetInt(EditorPrefsTableAI.Assistant_Timeout, _timeout);
                EditorPrefs.SetInt(EditorPrefsTableAI.Assistant_Round, _round);
                EditorPrefs.SetBool(EditorPrefsTableAI.Assistant_IsLogInEditor, _isLogInEditor);
                EditorPrefs.SetBool(EditorPrefsTableAI.Assistant_ShowThink, _isShowThink);
                EditorPrefs.SetInt(EditorPrefsTableAI.Assistant_SavePath, (int)_savePath);
                EditorPrefs.SetBool(EditorPrefsTableAI.Assistant_EnableAIButler, _isEnableAIButler);
                _assistantWindow.ApplySettings();
                Close();
            }
            if (GUILayout.Button("取消", "ButtonRight"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }
        private void Update()
        {
            if (_assistantWindow == null)
            {
                Close();
            }
        }
        private void RunOllamaInCMD(string order)
        {
            GUIUtility.systemCopyBuffer = order;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            using Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            Thread.Sleep(1000);

            keybd_event(VK_CONTROL, 0, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(VK_V, 0, KEYEVENTF_EXTENDEDKEY, 0);

            Thread.Sleep(100);

            keybd_event(VK_V, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            Thread.Sleep(1000);

            keybd_event(VK_RETURN, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(VK_RETURN, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }
        private void SetGenericMenuItem(GenericMenu gm, string group, string[] models)
        {
            for (int i = 0; i < models.Length; i++)
            {
                string model = models[i];
                gm.AddItem(new GUIContent(group + "/" + model), model == _model, () =>
                {
                    _model = model;
                });
            }
        }
    }

    /// <summary>
    /// 会话数据存储路径
    /// </summary>
    public enum AssistantSessionSavePath
    {
        LocalAppData = 0,
        Library = 1
    }
}