using UnityEngine;

namespace HT.Framework.AI
{
    /// <summary>
    /// 通用文字识别（含位置信息版）
    /// </summary>
    public sealed class OCRGeneralRecognition : CharacterRecognitionModeBase
    {
        public override string API
        {
            get
            {
                return "https://aip.baidubce.com/rest/2.0/ocr/v1/general";
            }
        }

        public override string ContentType
        {
            get
            {
                return "application/x-www-form-urlencoded";
            }
        }

        public OCRGeneralRecognition(Texture2D image, RecognizeGranularity granularity = RecognizeGranularity.Big, LanguageType language = LanguageType.CHN_ENG, bool isDetectDirection = false, bool isDetectLanguage = false)
            : base(image, granularity, language, isDetectDirection, isDetectLanguage)
        { }

        public OCRGeneralRecognition(string imagePath, RecognizeGranularity granularity = RecognizeGranularity.Big, LanguageType language = LanguageType.CHN_ENG, bool isDetectDirection = false, bool isDetectLanguage = false)
            : base(imagePath, granularity, language, isDetectDirection, isDetectLanguage)
        { }
    }
}