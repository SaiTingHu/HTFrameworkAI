using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace HT.Framework.AI
{
    /// <summary>
    /// 文字识别器
    /// </summary>
    public static class CharacterRecognitioner
    {
        private static readonly string TOKENAPI = "https://aip.baidubce.com/oauth/2.0/token";
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
                if (!request.isNetworkError && !request.isHttpError)
                {
                    JsonData jsonData = GlobalTools.StringToJson(request.downloadHandler.text);
                    TOKEN = jsonData["access_token"].ToString();
                }
                else
                {
                    GlobalTools.LogError("获取TOKEN失败：" + request.responseCode + " " + request.error);
                }
            }
        }

        /// <summary>
        /// 文字识别
        /// </summary>
        /// <param name="recognitionMode">识别方式</param>
        /// <returns>文字识别的协程</returns>
        public static Coroutine Recognition(CharacterRecognitionModeBase recognitionMode)
        {
            if (recognitionMode == null || recognitionMode.ImageSource == null)
            {
                GlobalTools.LogError("文字识别失败：识别方式和识别图像源均不能为空！");
                return null;
            }

            return Main.Current.StartCoroutine(RecognitionCoroutine(recognitionMode));
        }
        private static IEnumerator RecognitionCoroutine(CharacterRecognitionModeBase recognitionMode)
        {
            string url = string.Format("{0}?access_token={1}", recognitionMode.API, TOKEN);

            WWWForm form = new WWWForm();
            form.AddField("image", recognitionMode.GetImageSourceByBase64());
            form.AddField("recognize_granularity", recognitionMode.Granularity == CharacterRecognitionModeBase.RecognizeGranularity.Big ? "big" : "small");
            form.AddField("language_type", recognitionMode.Language.ToString());
            form.AddField("detect_direction", recognitionMode.IsDetectDirection ? "true" : "false");
            form.AddField("detect_language", recognitionMode.IsDetectLanguage ? "true" : "false");

            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                request.SetRequestHeader("Content-Type", recognitionMode.ContentType);
                yield return request.SendWebRequest();
                if (!request.isNetworkError && !request.isHttpError)
                {
                    JsonData jsonData = GlobalTools.StringToJson(request.downloadHandler.text);
                    if (jsonData.Keys.Contains("error_code"))
                    {
                        GlobalTools.LogError("文字识别失败：" + jsonData["error_code"].ToString() + " " + jsonData["error_msg"].ToString());

                        recognitionMode.FailHandler?.Invoke();
                    }
                    else
                    {
                        recognitionMode.SuccessHandler?.Invoke(new OCRResponse(jsonData));
                    }
                }
                else
                {
                    GlobalTools.LogError("文字识别失败：" + request.responseCode + " " + request.error);

                    recognitionMode.FailHandler?.Invoke();
                }
            }
        }
    }
}