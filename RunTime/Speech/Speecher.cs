using System.Collections;
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
        private static readonly string TOKENAPI = "https://openapi.baidu.com/oauth/2.0/token";
        private static readonly string SynthesisAPI = "https://tsn.baidu.com/text2audio";
        private static readonly string RecognitionAPI = "https://vop.baidu.com/server_api";
        private static string APIKEY = "";
        private static string SECRETKEY = "";
        private static string TOKEN = "";

        /// <summary>
        /// 设置APIKEY
        /// </summary>
        /// <param name="apiKey">APIKEY</param>
        public static void SetAPIKEY(string apiKey)
        {
            APIKEY = apiKey;
        }
        /// <summary>
        /// 设置SECRETKEY
        /// </summary>
        /// <param name="SecretKey">SECRETKEY</param>
        public static void SetSECRETKEY(string SecretKey)
        {
            SECRETKEY = SecretKey;
        }
        /// <summary>
        /// 设置TOKEN
        /// </summary>
        /// <param name="token">TOKEN</param>
        public static void SetTOKEN(string token)
        {
            TOKEN = token;
        }

        /// <summary>
        /// 生成TOKEN
        /// </summary>
        /// <returns>生成TOKEN的协程</returns>
        public static Coroutine GenerateTOKEN()
        {
            return Main.Current.StartCoroutine(GenerateTOKENCoroutine());
        }
        private static IEnumerator GenerateTOKENCoroutine()
        {
            string url = string.Format("{0}?grant_type=client_credentials&client_id={1}&client_secret={2}", TOKENAPI, APIKEY, SECRETKEY);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    JsonData jsonData = JsonToolkit.StringToJson(request.downloadHandler.text);
                    if (jsonData != null)
                    {
                        TOKEN = jsonData.GetValueInSafe("access_token", "");
                    }
                    else
                    {
                        Log.Error("获取TOKEN失败：" + request.downloadHandler.text);
                    }
                }
                else
                {
                    Log.Error("获取TOKEN失败：" + request.responseCode + " " + request.error);
                }
            }
        }

        /// <summary>
        /// 合成语音
        /// </summary>
        /// <param name="text">合成文本</param>
        /// <param name="handler">合成完毕后的处理者</param>
        /// <param name="failHandler">合成失败的处理者</param>
        /// <param name="timeout">超时时长</param>
        /// <param name="speaker">发音人</param>
        /// <param name="volume">音量</param>
        /// <param name="speed">音速</param>
        /// <param name="pitch">音调</param>
        /// <returns>合成语音的协程</returns>
        public static Coroutine Synthesis(string text, HTFAction<AudioClip> handler, HTFAction failHandler, int timeout = 60000, Speaker speaker = Speaker.DuYaYa, int volume = 15, int speed = 5, int pitch = 5)
        {
            if (string.IsNullOrEmpty(text) || Encoding.UTF8.GetByteCount(text) >= 1024)
            {
                Log.Error("合成语音失败：文本为空或长度超出了1024字节的限制！");
                return null;
            }
            
            return Main.Current.StartCoroutine(SynthesisCoroutine(text, handler, failHandler, timeout, speaker, volume, speed, pitch));
        }
        /// <summary>
        /// 合成语音
        /// </summary>
        /// <param name="text">合成文本</param>
        /// <param name="rule">合成规则</param>
        /// <param name="handler">合成完毕后的处理者</param>
        /// <param name="failHandler">合成失败的处理者</param>
        /// <param name="timeout">超时时长</param>
        /// <param name="speaker">发音人</param>
        /// <param name="volume">音量</param>
        /// <param name="speed">音速</param>
        /// <param name="pitch">音调</param>
        /// <returns>合成语音的协程</returns>
        public static Coroutine Synthesis(string text, SynthesisRule rule, HTFAction<AudioClip> handler, HTFAction failHandler, int timeout = 60000, Speaker speaker = Speaker.DuYaYa, int volume = 15, int speed = 5, int pitch = 5)
        {
            if (string.IsNullOrEmpty(text) || Encoding.UTF8.GetByteCount(text) >= 1024)
            {
                Log.Error("合成语音失败：文本为空或长度超出了1024字节的限制！");
                return null;
            }
            
            text = rule.Apply(text);
            return Main.Current.StartCoroutine(SynthesisCoroutine(text, handler, failHandler, timeout, speaker, volume, speed, pitch));
        }
        private static IEnumerator SynthesisCoroutine(string text, HTFAction<AudioClip> handler, HTFAction failHandler, int timeout, Speaker speaker, int volume, int speed, int pitch)
        {
            string url = string.Format("{0}?tex='{1}'&tok={2}&cuid={3}&ctp={4}&lan={5}&spd={6}&pit={7}&vol={8}&per={9}&aue={10}",
                SynthesisAPI, text, TOKEN, SystemInfo.deviceUniqueIdentifier, 1, "zh", speed, pitch, volume, (int)speaker, 6);

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string value = Encoding.UTF8.GetString(request.downloadHandler.data);
                    JsonData jsonData = JsonToolkit.StringToJson(value);
                    if (jsonData != null)
                    {
                        Log.Error("合成语音失败：" + value);

                        handler?.Invoke(null);
                    }
                    else
                    {
                        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);

                        handler?.Invoke(audioClip);
                    }
                }
                else
                {
                    Log.Error("合成语音失败：" + request.responseCode + " " + request.error);

                    failHandler?.Invoke();
                }
            }
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
        /// <returns>合成语音的协程</returns>
        public static Coroutine Synthesis(string text, string savePath, SynthesisType audioType = SynthesisType.MP3, int timeout = 60000, Speaker speaker = Speaker.DuYaYa, int volume = 15, int speed = 5, int pitch = 5)
        {
            if (string.IsNullOrEmpty(text) || Encoding.UTF8.GetByteCount(text) >= 1024)
            {
                Log.Error("合成语音失败：文本为空或长度超出了1024字节的限制！");
                return null;
            }
            
            return Main.Current.StartCoroutine(SynthesisCoroutine(text, savePath, audioType, timeout, speaker, volume, speed, pitch));
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
        /// <returns>合成语音的协程</returns>
        public static Coroutine Synthesis(string text, SynthesisRule rule, string savePath, SynthesisType audioType = SynthesisType.MP3, int timeout = 60000, Speaker speaker = Speaker.DuYaYa, int volume = 15, int speed = 5, int pitch = 5)
        {
            if (string.IsNullOrEmpty(text) || Encoding.UTF8.GetByteCount(text) >= 1024)
            {
                Log.Error("合成语音失败：文本为空或长度超出了1024字节的限制！");
                return null;
            }
            
            text = rule.Apply(text);
            return Main.Current.StartCoroutine(SynthesisCoroutine(text, savePath, audioType, timeout, speaker, volume, speed, pitch));
        }
        private static IEnumerator SynthesisCoroutine(string text, string savePath, SynthesisType audioType, int timeout, Speaker speaker, int volume, int speed, int pitch)
        {
            string url = string.Format("{0}?tex='{1}'&tok={2}&cuid={3}&ctp={4}&lan={5}&spd={6}&pit={7}&vol={8}&per={9}&aue={10}",
                SynthesisAPI, text, TOKEN, SystemInfo.deviceUniqueIdentifier, 1, "zh", speed, pitch, volume, (int)speaker, (int)audioType);

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, audioType == SynthesisType.MP3 ? AudioType.MPEG : AudioType.WAV))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    File.WriteAllBytes(savePath, request.downloadHandler.data);
                }
                else
                {
                    Log.Error("合成语音失败：" + request.responseCode + " " + request.error);
                }
            }
        }

        /// <summary>
        /// 语音识别
        /// </summary>
        /// <param name="clip">语音音频</param>
        /// <param name="handler">识别成功处理者</param>
        /// <param name="failHandler">识别失败处理者</param>
        /// <returns>语音识别的协程</returns>
        public static Coroutine Recognition(AudioClip clip, HTFAction<string> handler, HTFAction failHandler)
        {
            if (clip == null)
            {
                Log.Error("语音识别失败：语音内容为空！");
                return null;
            }

            return Main.Current.StartCoroutine(RecognitionCoroutine(SpeechUtility.FromAudioClip(clip), handler, failHandler));
        }
        /// <summary>
        /// 语音识别
        /// </summary>
        /// <param name="data">语音音频数据</param>
        /// <param name="handler">识别成功处理者</param>
        /// <param name="failHandler">识别失败处理者</param>
        /// <returns>语音识别的协程</returns>
        public static Coroutine Recognition(byte[] data, HTFAction<string> handler, HTFAction failHandler)
        {
            if (data == null || data.Length <= 0)
            {
                Log.Error("语音识别失败：语音内容为空！");
                return null;
            }

            return Main.Current.StartCoroutine(RecognitionCoroutine(data, handler, failHandler));
        }
        private static IEnumerator RecognitionCoroutine(byte[] data, HTFAction<string> handler, HTFAction failHandler)
        {
            string url = string.Format("{0}?cuid={1}&token={2}", RecognitionAPI, SystemInfo.deviceUniqueIdentifier, TOKEN);

            WWWForm form = new WWWForm();
            form.AddBinaryData("audio", data);

            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                request.SetRequestHeader("Content-Type", "audio/pcm;rate=16000");
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    JsonData jsonData = JsonToolkit.StringToJson(request.downloadHandler.text);
                    string no = jsonData != null ? jsonData.GetValueInSafe("err_no", "") : "";
                    string msg = jsonData != null ? jsonData.GetValueInSafe("err_msg", "") : "";

                    if (no == "0")
                    {
                        string text = null;
                        if (jsonData.ContainsKey("result") && jsonData["result"].Count > 0 && jsonData["result"][0] != null)
                        {
                            text = jsonData["result"][0].ToString();
                        }
                        handler?.Invoke(text);
                    }
                    else
                    {
                        Log.Error("语音识别失败：" + msg);

                        failHandler?.Invoke();
                    }
                }
                else
                {
                    Log.Error("语音识别失败：" + request.responseCode + " " + request.error);

                    failHandler?.Invoke();
                }
            }
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
        [Remark("普通女声")]
        Woman = 0,
        /// <summary>
        /// 普通男声
        /// </summary>
        [Remark("普通男声")]
        Man = 1,
        /// <summary>
        /// 度丫丫
        /// </summary>
        [Remark("度丫丫")]
        DuYaYa = 4,
        /// <summary>
        /// 度逍遥
        /// </summary>
        [Remark("度逍遥")]
        DuXiaoYao = 3,
        /// <summary>
        /// 度小娇
        /// </summary>
        [Remark("【精品发音人】度小娇")]
        DuXiaoJiao = 5,
        /// <summary>
        /// 度博文
        /// </summary>
        [Remark("【精品发音人】度博文")]
        DuBoWen = 106,
        /// <summary>
        /// 度小童
        /// </summary>
        [Remark("【精品发音人】度小童")]
        DuXiaoTong = 110,
        /// <summary>
        /// 度小萌
        /// </summary>
        [Remark("【精品发音人】度小萌")]
        DuXiaoMeng = 111,
        /// <summary>
        /// 度米朵
        /// </summary>
        [Remark("【精品发音人】度米朵")]
        DuMiDuo = 103,
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