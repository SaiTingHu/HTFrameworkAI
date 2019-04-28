using Baidu.Aip.Speech;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    public sealed class EditorSpeecher : EditorWindow
    {
        [MenuItem("HTFramework.AI/Editor Speecher")]
        private static void OpenEditorSpeecher()
        {
            EditorSpeecher window = GetWindow<EditorSpeecher>();
            window.titleContent.text = "Speecher";
            window.position = new Rect(200, 200, 400, 400);
            window.Show();
        }

        private string _synthesisText = "";
        private Vector2 _synthesisTextScroll = Vector2.zero;
        private string _savePath = "";
        private string _saveName = "NewAudio";
        private AudioType _format = AudioType.MP3;
        private int _timeout = 60000;
        private Speaker _speaker = Speaker.Woman;
        private int _volume = 15;
        private int _speed = 5;
        private int _pitch = 5;

        private void OnGUI()
        {
            TitleGUI();
            SynthesisTextGUI();
            SynthesisArgsGUI();
        }
        private void TitleGUI()
        {
            GUILayout.BeginHorizontal("Toolbar");
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
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("DD HeaderStyle");
            GUILayout.FlexibleSpace();
            GUILayout.Label("Speech Synthesis in Editor");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
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
            _format = (AudioType)EditorGUILayout.EnumPopup(_format);
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

            GUI.enabled = (_synthesisText != "" && _saveName != "");

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
        private void SynthesisInEditor(string text, string savePath, AudioType audioType = AudioType.MP3, int timeout = 60000, Speaker speaker = Speaker.Woman_DuYaYa, int volume = 15, int speed = 5, int pitch = 5)
        {
            if (string.IsNullOrEmpty(text) || text == "" || Encoding.Default.GetByteCount(text) >= 1024)
            {
                Debug.LogError("合成语音失败：文本为空或长度超出了1024字节的限制！");
                return;
            }
            if (File.Exists(savePath))
            {
                Debug.LogError("合成语音失败：已存在音频文件 " + savePath);
                return;
            }
            
            Tts tts = Speecher.NewTts();
            tts.Timeout = timeout;
            Dictionary<string, object> TtsOptions = new Dictionary<string, object>();
            TtsOptions["spd"] = Mathf.Clamp(speed, 0, 9);
            TtsOptions["pit"] = Mathf.Clamp(pitch, 0, 9);
            TtsOptions["vol"] = Mathf.Clamp(volume, 0, 15);
            TtsOptions["per"] = (int)speaker;
            TtsOptions["aue"] = (int)audioType;
            TtsResponse response = tts.Synthesis(text, TtsOptions);
            if (response.Success)
            {
                File.WriteAllBytes(savePath, response.Data);
                AssetDatabase.Refresh();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath(string.Format("Assets{0}/{1}.{2}", _savePath, _saveName, _format), typeof(AudioClip));
            }
            else
            {
                Debug.LogError("合成语音失败：" + response.ErrorCode + " " + response.ErrorMsg);
            }
        }
    }
}
