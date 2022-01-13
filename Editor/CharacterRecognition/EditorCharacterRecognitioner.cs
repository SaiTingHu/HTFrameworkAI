using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace HT.Framework.AI
{
    public sealed class EditorCharacterRecognitioner : HTFEditorWindow
    {
        [MenuItem("HTFramework.AI/Character Recognition/Editor Character Recognitioner")]
        private static void OpenEditorCharacterRecognitioner()
        {
            EditorCharacterRecognitioner window = GetWindow<EditorCharacterRecognitioner>();
            window.titleContent.text = "Character Recognitioner";
            window.minSize = new Vector2(400, 400);
            window.maxSize = new Vector2(400, 400);
            window.Show();
        }

        private readonly string[] Modes = new string[] {
            "通用",
            "通用（含位置信息）",
            "高精度",
            "高精度（含位置信息）" };
        private readonly string[] APIs = new string[] {
            "https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic",
            "https://aip.baidubce.com/rest/2.0/ocr/v1/general",
            "https://aip.baidubce.com/rest/2.0/ocr/v1/accurate_basic",
            "https://aip.baidubce.com/rest/2.0/ocr/v1/accurate" };
        private readonly string[] ContentTypes = new string[] {
            "application/x-www-form-urlencoded",
            "application/x-www-form-urlencoded",
            "application/x-www-form-urlencoded",
            "application/x-www-form-urlencoded" };
        private readonly string TOKENAPI = "https://aip.baidubce.com/oauth/2.0/token";
        private string APIKEY = "";
        private string SECRETKEY = "";
        private string TOKEN = "";
        private Texture2D _texture;
        private OCRResponse _response;
        private string _result = "";
        private Vector2 _resultScroll = Vector2.zero;
        private bool _isRecognition = false;

        private int _mode = 0;
        private string _api = "https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic";
        private string _contentType = "application/x-www-form-urlencoded";
        private CharacterRecognitionModeBase.RecognizeGranularity _granularity = CharacterRecognitionModeBase.RecognizeGranularity.Big;
        private CharacterRecognitionModeBase.LanguageType _language = CharacterRecognitionModeBase.LanguageType.CHN_ENG;
        private bool _isDetectDirection = false;
        private bool _isDetectLanguage = false;

        protected override string HelpUrl => "https://wanderer.blog.csdn.net/article/details/103765003";

        protected override void OnEnable()
        {
            base.OnEnable();

            APIKEY = EditorPrefs.GetString(EditorPrefsTableAI.CR_APIKEY, "");
            SECRETKEY = EditorPrefs.GetString(EditorPrefsTableAI.CR_SECRETKEY, "");
            TOKEN = EditorPrefs.GetString(EditorPrefsTableAI.CR_TOKEN, "");
        }
        protected override void OnTitleGUI()
        {
            base.OnTitleGUI();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Console Login", EditorStyles.toolbarButton))
            {
                Application.OpenURL(@"https://login.bce.baidu.com/");
            }
        }
        protected override void OnBodyGUI()
        {
            base.OnBodyGUI();

            RecognitionIdentityGUI();
            RecognitionTexture();
            ResultGUI();
            RecognitionArgsGUI();
            RecognitionButtonGUI();
        }
        private void RecognitionIdentityGUI()
        {
            GUILayout.BeginHorizontal("DD HeaderStyle");
            GUILayout.FlexibleSpace();
            GUILayout.Label("Character Recognition in Editor");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.BeginVertical(EditorGlobalTools.Styles.Box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("API Key:", GUILayout.Width(80));
            string apikey = EditorGUILayout.PasswordField(APIKEY);
            if (apikey != APIKEY)
            {
                APIKEY = apikey;
                EditorPrefs.SetString(EditorPrefsTableAI.CR_APIKEY, APIKEY);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Secret Key:", GUILayout.Width(80));
            string secretkey = EditorGUILayout.PasswordField(SECRETKEY);
            if (secretkey != SECRETKEY)
            {
                SECRETKEY = secretkey;
                EditorPrefs.SetString(EditorPrefsTableAI.CR_SECRETKEY, SECRETKEY);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Token:", GUILayout.Width(80));
            EditorGUILayout.TextField(TOKEN);
            GUI.enabled = (APIKEY != "" && SECRETKEY != "");
            if (GUILayout.Button("Generate", EditorStyles.miniButton, GUILayout.Width(70)))
            {
                string uri = string.Format("{0}?grant_type=client_credentials&client_id={1}&client_secret={2}", TOKENAPI, APIKEY, SECRETKEY);
                UnityWebRequest request = UnityWebRequest.Get(uri);
                UnityWebRequestAsyncOperation async = request.SendWebRequest();
                async.completed += GenerateTOKENDone;
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        private void RecognitionTexture()
        {
            GUILayout.BeginVertical(EditorGlobalTools.Styles.Box);
            GUILayout.Label("Recognition Texture:");
            _texture = EditorGUILayout.ObjectField(_texture, typeof(Texture2D), false) as Texture2D;
            GUILayout.EndVertical();
        }
        private void ResultGUI()
        {
            GUILayout.BeginVertical(EditorGlobalTools.Styles.Box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Result:");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear", EditorStyles.miniButton))
            {
                _response = null;
                _result = "";
            }
            GUILayout.EndHorizontal();

            _resultScroll = GUILayout.BeginScrollView(_resultScroll);
            EditorGUILayout.TextArea(_result);
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
        private void RecognitionArgsGUI()
        {
            GUILayout.BeginVertical(EditorGlobalTools.Styles.Box);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Recognition Mode:", GUILayout.Width(120));
            int mode = EditorGUILayout.Popup(_mode, Modes);
            if (mode != _mode)
            {
                _mode = mode;
                _api = APIs[_mode];
                _contentType = ContentTypes[_mode];
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Granularity:", GUILayout.Width(120));
            if (GUILayout.Button(_granularity.GetRemark(), EditorGlobalTools.Styles.MiniPopup))
            {
                GenericMenu gm = new GenericMenu();
                foreach (var granularity in typeof(CharacterRecognitionModeBase.RecognizeGranularity).GetEnumValues())
                {
                    CharacterRecognitionModeBase.RecognizeGranularity g = (CharacterRecognitionModeBase.RecognizeGranularity)granularity;
                    gm.AddItem(new GUIContent(g.GetRemark()), _granularity == g, () =>
                    {
                        _granularity = g;
                    });
                }
                gm.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Language:", GUILayout.Width(120));
            if (GUILayout.Button(_language.GetRemark(), EditorGlobalTools.Styles.MiniPopup))
            {
                GenericMenu gm = new GenericMenu();
                foreach (var language in typeof(CharacterRecognitionModeBase.LanguageType).GetEnumValues())
                {
                    CharacterRecognitionModeBase.LanguageType l = (CharacterRecognitionModeBase.LanguageType)language;
                    gm.AddItem(new GUIContent(l.GetRemark()), _language == l, () =>
                    {
                        _language = l;
                    });
                }
                gm.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            GUI.enabled = false;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Detect Direction:", GUILayout.Width(120));
            _isDetectDirection = EditorGUILayout.Toggle(_isDetectDirection);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Detect Language:", GUILayout.Width(120));
            _isDetectLanguage = EditorGUILayout.Toggle(_isDetectLanguage);
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            GUILayout.EndVertical();
        }
        private void RecognitionButtonGUI()
        {
            GUI.enabled = !_isRecognition && TOKEN != "" && _texture != null;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Recognition", EditorGlobalTools.Styles.LargeButton))
            {
                RecognitionInEditor();
            }
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        private void RecognitionInEditor()
        {
            string url = string.Format("{0}?access_token={1}", _api, TOKEN);

            WWWForm form = new WWWForm();
            form.AddField("image", Convert.ToBase64String(_texture.EncodeToJPG()));
            form.AddField("recognize_granularity", _granularity == CharacterRecognitionModeBase.RecognizeGranularity.Big ? "big" : "small");
            form.AddField("language_type", _language.ToString());
            form.AddField("detect_direction", _isDetectDirection ? "true" : "false");
            form.AddField("detect_language", _isDetectLanguage ? "true" : "false");

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Content-Type", _contentType);
            UnityWebRequestAsyncOperation async = request.SendWebRequest();
            async.completed += RecognitionDone;
            _isRecognition = true;
        }
        private void GenerateTOKENDone(AsyncOperation asyncOperation)
        {
            UnityWebRequestAsyncOperation async = asyncOperation as UnityWebRequestAsyncOperation;
            if (async != null)
            {
                if (!async.webRequest.isNetworkError && !async.webRequest.isHttpError)
                {
                    JsonData jsonData = JsonToolkit.StringToJson(async.webRequest.downloadHandler.text);
                    if (jsonData != null)
                    {
                        TOKEN = jsonData.GetValueInSafe("access_token", "");
                        EditorPrefs.SetString(EditorPrefsTableAI.CR_TOKEN, TOKEN);
                        Repaint();
                    }
                    else
                    {
                        Log.Error("获取TOKEN失败：" + async.webRequest.downloadHandler.text);
                    }
                }
                else
                {
                    Log.Error("获取Token失败：" + async.webRequest.responseCode + " " + async.webRequest.error);
                }
                async.webRequest.Dispose();
            }
            else
            {
                Log.Error("获取Token失败：错误的请求操作！");
            }
        }
        private void RecognitionDone(AsyncOperation asyncOperation)
        {
            UnityWebRequestAsyncOperation async = asyncOperation as UnityWebRequestAsyncOperation;
            if (async != null)
            {
                if (!async.webRequest.isNetworkError && !async.webRequest.isHttpError)
                {
                    JsonData jsonData = JsonToolkit.StringToJson(async.webRequest.downloadHandler.text);
                    if (jsonData != null)
                    {
                        if (jsonData.Keys.Contains("error_code"))
                        {
                            Log.Error("文字识别失败：" + jsonData.GetValueInSafe("error_code", "") + " " + jsonData.GetValueInSafe("error_msg", ""));
                        }
                        else
                        {
                            _response = new OCRResponse(jsonData);

                            StringBuilder builder = new StringBuilder();
                            for (int i = 0; i < _response.Words.Count; i++)
                            {
                                builder.Append(_response.Words[i].Content + "\r\n");
                            }
                            _result = builder.ToString();
                        }
                    }
                    else
                    {
                        Log.Error("文字识别失败：" + async.webRequest.downloadHandler.text);
                    }
                }
                else
                {
                    Log.Error("文字识别失败：" + async.webRequest.responseCode + " " + async.webRequest.error);
                }
                async.webRequest.Dispose();
            }
            else
            {
                Log.Error("文字识别失败：错误的请求操作！");
            }
            _isRecognition = false;
        }
    }
}