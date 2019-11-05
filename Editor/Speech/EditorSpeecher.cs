using Baidu.Aip.Speech;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace HT.Framework.AI
{
    public sealed class EditorSpeecher : HTFEditorWindow
    {
        [MenuItem("HTFramework.AI/Speech/Editor Speecher")]
        private static void OpenEditorSpeecher()
        {
            EditorSpeecher window = GetWindow<EditorSpeecher>();
            window.titleContent.text = "Speecher";
            window.position = new Rect(200, 200, 400, 400);
            window.Show();
        }

        private string APIKEY = "";
        private string SECRETKEY = "";
        private string TOKEN = "";
        private string _synthesisText = "";
        private Vector2 _synthesisTextScroll = Vector2.zero;
        private string _savePath = "";
        private string _saveName = "NewAudio";
        private SynthesisType _format = SynthesisType.MP3;
        private int _timeout = 60000;
        private Speaker _speaker = Speaker.Woman;
        private int _volume = 15;
        private int _speed = 5;
        private int _pitch = 5;
        private Dictionary<string, object> _ttsOptions = new Dictionary<string, object>()
        {
             {"spd", 5},
             {"pit", 5},
             {"vol", 15},
             {"per", 4},
             {"aue", 3}
        };

        private void OnEnable()
        {
            APIKEY = EditorPrefs.GetString(EditorPrefsTableAI.Speech_APIKEY, "");
            SECRETKEY = EditorPrefs.GetString(EditorPrefsTableAI.Speech_SECRETKEY, "");
        }
        protected override void OnTitleGUI()
        {
            base.OnTitleGUI();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("About", "Toolbarbutton"))
            {
                Application.OpenURL(@"http://ai.baidu.com/");
            }
            if (GUILayout.Button("SDK", "Toolbarbutton"))
            {
                Application.OpenURL(@"http://ai.baidu.com/docs#/TTS-Online-Csharp-SDK/top");
            }
            if (GUILayout.Button("Console Login", "Toolbarbutton"))
            {
                Application.OpenURL(@"https://login.bce.baidu.com/");
            }
        }
        protected override void OnBodyGUI()
        {
            base.OnBodyGUI();

            SynthesisIdentityGUI();
            SynthesisTextGUI();
            SynthesisArgsGUI();
            SynthesisButtonGUI();
        }
        private void SynthesisIdentityGUI()
        {
            GUILayout.BeginHorizontal("DD HeaderStyle");
            GUILayout.FlexibleSpace();
            GUILayout.Label("Speech Synthesis in Editor");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.BeginVertical("Box");

            GUILayout.BeginHorizontal();
            GUILayout.Label("API Key:", GUILayout.Width(80));
            string apikey = EditorGUILayout.PasswordField(APIKEY);
            if (apikey != APIKEY)
            {
                APIKEY = apikey;
                EditorPrefs.SetString(EditorPrefsTableAI.Speech_APIKEY, APIKEY);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Secret Key:", GUILayout.Width(80));
            string secretkey = EditorGUILayout.PasswordField(SECRETKEY);
            if (secretkey != SECRETKEY)
            {
                SECRETKEY = secretkey;
                EditorPrefs.SetString(EditorPrefsTableAI.Speech_SECRETKEY, SECRETKEY);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Token:", GUILayout.Width(80));
            EditorGUILayout.TextField(TOKEN);
            GUI.enabled = (APIKEY != "" && SECRETKEY != "");
            if (GUILayout.Button("Generate", "Minibutton", GUILayout.Width(60)))
            {
                string uri = string.Format("https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials&client_id={0}&client_secret={1}", APIKEY, SECRETKEY);
                UnityWebRequest request = UnityWebRequest.Get(uri);
                UnityWebRequestAsyncOperation async = request.SendWebRequest();
                async.completed += UnityWebRequestAsyncOperationDone;
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        private void SynthesisTextGUI()
        {
            GUILayout.BeginVertical("Box");
            GUILayout.Label("Synthesis Text:");
            _synthesisTextScroll = GUILayout.BeginScrollView(_synthesisTextScroll);
            _synthesisText = EditorGUILayout.TextArea(_synthesisText);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        private void SynthesisArgsGUI()
        {
            GUILayout.BeginVertical("Box");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Save Path:", GUILayout.Width(80));
            _savePath = EditorGUILayout.TextField(_savePath);
            if (GUILayout.Button("Browse", "Minibutton"))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Path", Application.dataPath, "");
                if (path.Length != 0)
                {
                    _savePath = path.Replace(Application.dataPath, "");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Save Name:", GUILayout.Width(80));
            _saveName = EditorGUILayout.TextField(_saveName);
            GUILayout.Label("Format:");
            _format = (SynthesisType)EditorGUILayout.EnumPopup(_format);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Timeout:", GUILayout.Width(80));
            _timeout = EditorGUILayout.IntField(_timeout);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Speaker:", GUILayout.Width(80));
            bool man = GUILayout.Toggle(_speaker == Speaker.Man, "Man");
            if (man) _speaker = Speaker.Man;
            bool woman = GUILayout.Toggle(_speaker == Speaker.Woman, "Woman");
            if (woman) _speaker = Speaker.Woman;
            bool man_DuXiaoYao = GUILayout.Toggle(_speaker == Speaker.Man_DuXiaoYao, "Man_DXY");
            if (man_DuXiaoYao) _speaker = Speaker.Man_DuXiaoYao;
            bool woman_DuYaYa = GUILayout.Toggle(_speaker == Speaker.Woman_DuYaYa, "Woman_DYY");
            if (woman_DuYaYa) _speaker = Speaker.Woman_DuYaYa;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Volume:", GUILayout.Width(80));
            _volume = EditorGUILayout.IntSlider(_volume, 0, 15);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Speed:", GUILayout.Width(80));
            _speed = EditorGUILayout.IntSlider(_speed, 0, 9);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Pitch:", GUILayout.Width(80));
            _pitch = EditorGUILayout.IntSlider(_pitch, 0, 9);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        private void SynthesisButtonGUI()
        {
            GUI.enabled = (APIKEY != "" && SECRETKEY != "" && _synthesisText != "" && _saveName != "");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Synthesis", "LargeButton"))
            {
                string path = string.Format("{0}{1}/{2}.{3}", Application.dataPath, _savePath, _saveName, _format);
                SynthesisInEditor(_synthesisText, path, _format, _timeout, _speaker, _volume, _speed, _pitch);
            }
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        /// <summary>
        /// 合成语音，并保存语音文件（编辑器内）
        /// </summary>
        /// <param name="text">合成文本</param>
        /// <param name="savePath">语音文件保存路径</param>
        /// <param name="timeout">超时时长</param>
        /// <param name="audioType">音频文件格式</param>
        /// <param name="speaker">发音人</param>
        /// <param name="volume">音量</param>
        /// <param name="speed">音速</param>
        /// <param name="pitch">音调</param>
        private void SynthesisInEditor(string text, string savePath, SynthesisType audioType = SynthesisType.MP3, int timeout = 60000, Speaker speaker = Speaker.Woman_DuYaYa, int volume = 15, int speed = 5, int pitch = 5)
        {
            if (string.IsNullOrEmpty(text) || text == "" || Encoding.Default.GetByteCount(text) >= 1024)
            {
                GlobalTools.LogError("合成语音失败：文本为空或长度超出了1024字节的限制！");
                return;
            }
            if (File.Exists(savePath))
            {
                GlobalTools.LogError("合成语音失败：已存在音频文件 " + savePath);
                return;
            }

            Tts tts = new Tts(APIKEY, SECRETKEY);
            tts.Timeout = timeout;
            _ttsOptions["spd"] = Mathf.Clamp(speed, 0, 9);
            _ttsOptions["pit"] = Mathf.Clamp(pitch, 0, 9);
            _ttsOptions["vol"] = Mathf.Clamp(volume, 0, 15);
            _ttsOptions["per"] = (int)speaker;
            _ttsOptions["aue"] = (int)audioType;
            TtsResponse response = tts.Synthesis(text, _ttsOptions);
            if (response.Success)
            {
                File.WriteAllBytes(savePath, response.Data);
                AssetDatabase.Refresh();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath(GlobalTools.StringConcat("Assets", _savePath, "/", _saveName, ".", _format.ToString()), typeof(AudioClip));
            }
            else
            {
                GlobalTools.LogError("合成语音失败：" + response.ErrorCode + " " + response.ErrorMsg);
            }
        }

        private void UnityWebRequestAsyncOperationDone(AsyncOperation asyncOperation)
        {
            UnityWebRequestAsyncOperation async = asyncOperation as UnityWebRequestAsyncOperation;
            if (async != null)
            {
                if (string.IsNullOrEmpty(async.webRequest.error))
                {
                    JsonData json = GlobalTools.StringToJson(async.webRequest.downloadHandler.text);
                    TOKEN = json["access_token"].ToString();
                    Repaint();
                }
                else
                {
                    GlobalTools.LogError("获取Token失败：" + async.webRequest.responseCode + " " + async.webRequest.error);
                }
            }
            else
            {
                GlobalTools.LogError("获取Token失败：错误的请求操作！");
            }
        }
    }
}