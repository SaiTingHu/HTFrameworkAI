using Baidu.Aip.Speech;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HT.Framework.AI
{
    /// <summary>
    /// 语音管理器
    /// </summary>
    public static class Speecher
    {
        private static string APIKEY = "";
        private static string SECRETKEY = "";
        private static string TOKEN = "";
        private static SpeechCoroutiner Coroutiner;
        private static Queue<Tts> Ttss = new Queue<Tts>();
        private static Dictionary<string, object> TtsOptions = new Dictionary<string, object>()
        {
             {"spd", 5},
             {"pit", 5},
             {"vol", 15},
             {"per", 4},
             {"aue", 6}
        };

        public static void SetAPIKEY(string apiKey)
        {
            APIKEY = apiKey;
        }
        public static void SetSECRETKEY(string SecretKey)
        {
            SECRETKEY = SecretKey;
        }
        public static void SetTOKEN(string token)
        {
            TOKEN = token;
        }

        private static Tts GetTts()
        {
            Tts tts;
            if (Ttss.Count > 0)
            {
                tts = Ttss.Dequeue();
            }
            else
            {
                tts = new Tts(APIKEY, SECRETKEY);
            }
            return tts;
        }
        private static void RecycleTts(Tts tts)
        {
            Ttss.Enqueue(tts);
        }

        /// <summary>
        /// 合成语音
        /// </summary>
        /// <param name="text">合成文本</param>
        /// <param name="handler">合成完毕后的处理者</param>
        /// <param name="timeout">超时时长</param>
        /// <param name="speaker">发音人</param>
        /// <param name="volume">音量</param>
        /// <param name="speed">音速</param>
        /// <param name="pitch">音调</param>
        public static void SynthesisByTOKEN(string text, HTFAction<AudioClip> handler, int timeout = 60000, Speaker speaker = Speaker.Woman_DuYaYa, int volume = 15, int speed = 5, int pitch = 5)
        {
            if (string.IsNullOrEmpty(text) || text == "" || Encoding.Default.GetByteCount(text) >= 1024)
            {
                Debug.LogError("合成语音失败：文本为空或长度超出了1024字节的限制！");
                return;
            }

            if (!Coroutiner)
            {
                GameObject obj = new GameObject("Coroutiner");
                Coroutiner = obj.AddComponent<SpeechCoroutiner>();
                Object.DontDestroyOnLoad(obj);
            }

            Coroutiner.StartCoroutine(SynthesisByTOKENCoroutine(text, handler, timeout, speaker, volume, speed, pitch));
        }
        /// <summary>
        /// 合成语音
        /// </summary>
        /// <param name="text">合成文本</param>
        /// <param name="rule">合成规则</param>
        /// <param name="handler">合成完毕后的处理者</param>
        /// <param name="timeout">超时时长</param>
        /// <param name="speaker">发音人</param>
        /// <param name="volume">音量</param>
        /// <param name="speed">音速</param>
        /// <param name="pitch">音调</param>
        public static void SynthesisByTOKEN(string text, SynthesisRule rule, HTFAction<AudioClip> handler, int timeout = 60000, Speaker speaker = Speaker.Woman_DuYaYa, int volume = 15, int speed = 5, int pitch = 5)
        {
            if (string.IsNullOrEmpty(text) || text == "" || Encoding.Default.GetByteCount(text) >= 1024)
            {
                Debug.LogError("合成语音失败：文本为空或长度超出了1024字节的限制！");
                return;
            }

            if (!Coroutiner)
            {
                GameObject obj = new GameObject("Coroutiner");
                Coroutiner = obj.AddComponent<SpeechCoroutiner>();
                Object.DontDestroyOnLoad(obj);
            }

            text = rule.ApplyCustomTone(text);
            Coroutiner.StartCoroutine(SynthesisByTOKENCoroutine(text, handler, timeout, speaker, volume, speed, pitch));
        }
        private static IEnumerator SynthesisByTOKENCoroutine(string text, HTFAction<AudioClip> handler, int timeout, Speaker speaker, int volume, int speed, int pitch)
        {
            string url = string.Format("http://tsn.baidu.com/text2audio?tex={0}&tok={1}&cuid={2}&ctp={3}&lan={4}&spd={5}&pit={6}&vol={7}&per={8}&aue={9}",
                text, TOKEN, SystemInfo.deviceUniqueIdentifier, 1, "zh", speed, pitch, volume, (int)speaker, 6);

            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
            yield return request.SendWebRequest();

            if (string.IsNullOrEmpty(request.error))
            {
                string type = request.GetResponseHeader("Content-Type");
                if (type.Contains("audio"))
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

                    if (handler != null)
                        handler(clip);
                }
                else
                {
                    Debug.LogError("合成语音失败：获取到错误的合成结果类型 " + type);
                }
            }
            else
            {
                Debug.LogError("合成语音失败：" + request.responseCode + " " + request.error);
            }
        }

        /// <summary>
        /// 合成语音
        /// </summary>
        /// <param name="text">合成文本</param>
        /// <param name="handler">合成完毕后的处理者</param>
        /// <param name="timeout">超时时长</param>
        /// <param name="speaker">发音人</param>
        /// <param name="volume">音量</param>
        /// <param name="speed">音速</param>
        /// <param name="pitch">音调</param>
        public static void SynthesisByKEY(string text, HTFAction<AudioClip> handler, int timeout = 60000, Speaker speaker = Speaker.Woman_DuYaYa, int volume = 15, int speed = 5, int pitch = 5)
        {
            if (string.IsNullOrEmpty(text) || text == "" || Encoding.Default.GetByteCount(text) >= 1024)
            {
                Debug.LogError("合成语音失败：文本为空或长度超出了1024字节的限制！");
                return;
            }

            if (!Coroutiner)
            {
                GameObject obj = new GameObject("Coroutiner");
                Coroutiner = obj.AddComponent<SpeechCoroutiner>();
                Object.DontDestroyOnLoad(obj);
            }

            Coroutiner.StartCoroutine(SynthesisByKEYCoroutine(text, handler, timeout, speaker, volume, speed, pitch));
        }
        /// <summary>
        /// 合成语音
        /// </summary>
        /// <param name="text">合成文本</param>
        /// <param name="rule">合成规则</param>
        /// <param name="handler">合成完毕后的处理者</param>
        /// <param name="timeout">超时时长</param>
        /// <param name="speaker">发音人</param>
        /// <param name="volume">音量</param>
        /// <param name="speed">音速</param>
        /// <param name="pitch">音调</param>
        public static void SynthesisByKEY(string text, SynthesisRule rule, HTFAction<AudioClip> handler, int timeout = 60000, Speaker speaker = Speaker.Woman_DuYaYa, int volume = 15, int speed = 5, int pitch = 5)
        {
            if (string.IsNullOrEmpty(text) || text == "" || Encoding.Default.GetByteCount(text) >= 1024)
            {
                Debug.LogError("合成语音失败：文本为空或长度超出了1024字节的限制！");
                return;
            }

            if (!Coroutiner)
            {
                GameObject obj = new GameObject("Coroutiner");
                Coroutiner = obj.AddComponent<SpeechCoroutiner>();
                Object.DontDestroyOnLoad(obj);
            }

            text = rule.ApplyCustomTone(text);
            Coroutiner.StartCoroutine(SynthesisByKEYCoroutine(text, handler, timeout, speaker, volume, speed, pitch));
        }
        private static IEnumerator SynthesisByKEYCoroutine(string text, HTFAction<AudioClip> handler, int timeout, Speaker speaker, int volume, int speed, int pitch)
        {
            Tts tts = GetTts();
            tts.Timeout = timeout;
            TtsOptions["spd"] = Mathf.Clamp(speed, 0, 9);
            TtsOptions["pit"] = Mathf.Clamp(pitch, 0, 9);
            TtsOptions["vol"] = Mathf.Clamp(volume, 0, 15);
            TtsOptions["per"] = (int)speaker;
            TtsOptions["aue"] = 6;
            TtsResponse response = tts.Synthesis(text, TtsOptions);
            yield return response;
            if (response.Success)
            {
                AudioClip audioClip = SpeechUtility.ToAudioClip(response.Data);
                yield return audioClip;

                if (handler != null)
                    handler(audioClip);
            }
            else
            {
                Debug.LogError("合成语音失败：" + response.ErrorCode + " " + response.ErrorMsg);
            }
            RecycleTts(tts);
        }

        /// <summary>
        /// 合成语音，并保存语音文件
        /// </summary>
        /// <param name="text">合成文本</param>
        /// <param name="savePath">语音文件保存路径</param>
        /// <param name="timeout">超时时长</param>
        /// <param name="audioType">音频文件格式</param>
        /// <param name="speaker">发音人</param>
        /// <param name="volume">音量</param>
        /// <param name="speed">音速</param>
        /// <param name="pitch">音调</param>
        public static void SynthesisByKEY(string text, string savePath, SynthesisType audioType = SynthesisType.MP3, int timeout = 60000, Speaker speaker = Speaker.Woman_DuYaYa, int volume = 15, int speed = 5, int pitch = 5)
        {
            if (string.IsNullOrEmpty(text) || text == "" || Encoding.Default.GetByteCount(text) >= 1024)
            {
                Debug.LogError("合成语音失败：文本为空或长度超出了1024字节的限制！");
                return;
            }

            if (!Coroutiner)
            {
                GameObject obj = new GameObject("Coroutiner");
                Coroutiner = obj.AddComponent<SpeechCoroutiner>();
                Object.DontDestroyOnLoad(obj);
            }

            Coroutiner.StartCoroutine(SynthesisByKEYCoroutine(text, savePath, audioType, timeout, speaker, volume, speed, pitch));
        }
        /// <summary>
        /// 合成语音，并保存语音文件
        /// </summary>
        /// <param name="text">合成文本</param>
        /// <param name="rule">合成规则</param>
        /// <param name="savePath">语音文件保存路径</param>
        /// <param name="timeout">超时时长</param>
        /// <param name="audioType">音频文件格式</param>
        /// <param name="speaker">发音人</param>
        /// <param name="volume">音量</param>
        /// <param name="speed">音速</param>
        /// <param name="pitch">音调</param>
        public static void SynthesisByKEY(string text, SynthesisRule rule, string savePath, SynthesisType audioType = SynthesisType.MP3, int timeout = 60000, Speaker speaker = Speaker.Woman_DuYaYa, int volume = 15, int speed = 5, int pitch = 5)
        {
            if (string.IsNullOrEmpty(text) || text == "" || Encoding.Default.GetByteCount(text) >= 1024)
            {
                Debug.LogError("合成语音失败：文本为空或长度超出了1024字节的限制！");
                return;
            }

            if (!Coroutiner)
            {
                GameObject obj = new GameObject("Coroutiner");
                Coroutiner = obj.AddComponent<SpeechCoroutiner>();
                Object.DontDestroyOnLoad(obj);
            }

            text = rule.ApplyCustomTone(text);
            Coroutiner.StartCoroutine(SynthesisByKEYCoroutine(text, savePath, audioType, timeout, speaker, volume, speed, pitch));
        }
        private static IEnumerator SynthesisByKEYCoroutine(string text, string savePath, SynthesisType audioType, int timeout, Speaker speaker, int volume, int speed, int pitch)
        {
            Tts tts = GetTts();
            tts.Timeout = timeout;
            TtsOptions["spd"] = Mathf.Clamp(speed, 0, 9);
            TtsOptions["pit"] = Mathf.Clamp(pitch, 0, 9);
            TtsOptions["vol"] = Mathf.Clamp(volume, 0, 15);
            TtsOptions["per"] = (int)speaker;
            TtsOptions["aue"] = (int)audioType;
            TtsResponse response = tts.Synthesis(text, TtsOptions);
            yield return response;
            if (response.Success)
            {
                File.WriteAllBytes(savePath, response.Data);
                yield return null;
            }
            else
            {
                Debug.LogError("合成语音失败：" + response.ErrorCode + " " + response.ErrorMsg);
            }
            RecycleTts(tts);
        }
    }

    /// <summary>
    /// 发音人
    /// </summary>
    public enum Speaker
    {
        /// <summary>
        /// 普通女声
        /// </summary>
        Woman = 0,
        /// <summary>
        /// 普通男声
        /// </summary>
        Man = 1,
        /// <summary>
        /// 情感女声-度丫丫
        /// </summary>
        Woman_DuYaYa = 4,
        /// <summary>
        /// 情感男声-度逍遥
        /// </summary>
        Man_DuXiaoYao = 3
    }

    /// <summary>
    /// 合成的音频格式
    /// </summary>
    public enum SynthesisType
    {
        /// <summary>
        /// MP3格式
        /// </summary>
        MP3 = 3,
        /// <summary>
        /// WAV格式
        /// </summary>
        WAV = 6,
    }
}
