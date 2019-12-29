using System;
using System.IO;
using UnityEngine;

namespace HT.Framework.AI
{
    /// <summary>
    /// 文字识别方式
    /// </summary>
    public abstract class CharacterRecognitionModeBase
    {
        /// <summary>
        /// 接口
        /// </summary>
        public abstract string API { get; }
        /// <summary>
        /// ContentType
        /// </summary>
        public abstract string ContentType { get; }
        /// <summary>
        /// 图像源
        /// </summary>
        public Texture2D ImageSource;
        /// <summary>
        /// 识别粒度
        /// </summary>
        public RecognizeGranularity Granularity;
        /// <summary>
        /// 识别语言
        /// </summary>
        public LanguageType Language;
        /// <summary>
        /// 是否检测图像朝向
        /// </summary>
        public bool IsDetectDirection;
        /// <summary>
        /// 是否检测语言
        /// </summary>
        public bool IsDetectLanguage;
        /// <summary>
        /// 识别成功处理者
        /// </summary>
        public HTFAction<OCRResponse> SuccessHandler;
        /// <summary>
        /// 识别失败处理者
        /// </summary>
        public HTFAction FailHandler;

        public CharacterRecognitionModeBase(Texture2D image, RecognizeGranularity granularity = RecognizeGranularity.Big, LanguageType language = LanguageType.CHN_ENG, bool isDetectDirection = false, bool isDetectLanguage = false)
        {
            ImageSource = image;
            Granularity = granularity;
            Language = language;
            IsDetectDirection = isDetectDirection;
            IsDetectLanguage = isDetectLanguage;
        }

        public CharacterRecognitionModeBase(string imagePath, RecognizeGranularity granularity = RecognizeGranularity.Big, LanguageType language = LanguageType.CHN_ENG, bool isDetectDirection = false, bool isDetectLanguage = false)
        {
            if (File.Exists(imagePath))
            {
                using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);

                    ImageSource = new Texture2D(80, 80);
                    ImageSource.LoadImage(buffer);
                }
            }
            else
            {
                ImageSource = null;
            }

            Granularity = granularity;
            Language = language;
            IsDetectDirection = isDetectDirection;
            IsDetectLanguage = isDetectLanguage;
        }

        public string GetImageSourceByBase64()
        {
            if (ImageSource != null)
            {
                return Convert.ToBase64String(ImageSource.EncodeToJPG());
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 识别粒度
        /// </summary>
        public enum RecognizeGranularity
        {
            Big,
            Small
        }

        /// <summary>
        /// 识别语言
        /// </summary>
        public enum LanguageType
        {
            /// <summary>
            /// 中英文混合
            /// </summary>
            CHN_ENG,
            /// <summary>
            /// 英文
            /// </summary>
            ENG,
            /// <summary>
            /// 日语
            /// </summary>
            JAP,
            /// <summary>
            /// 韩语
            /// </summary>
            KOR,
            /// <summary>
            /// 法语
            /// </summary>
            FRE,
            /// <summary>
            /// 西班牙语
            /// </summary>
            SPA,
            /// <summary>
            /// 葡萄牙语
            /// </summary>
            POR,
            /// <summary>
            /// 德语
            /// </summary>
            GER,
            /// <summary>
            /// 意大利语
            /// </summary>
            ITA,
            /// <summary>
            /// 俄语
            /// </summary>
            RUS
        }
    }
}