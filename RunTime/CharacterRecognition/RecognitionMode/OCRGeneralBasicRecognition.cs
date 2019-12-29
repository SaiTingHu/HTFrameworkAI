using UnityEngine;

namespace HT.Framework.AI
{
    /// <summary>
    /// 通用文字识别
    /// </summary>
    public sealed class OCRGeneralBasicRecognition : CharacterRecognitionModeBase
    {
        public override string API
        {
            get
            {
                return "https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic";
            }
        }

        public override string ContentType
        {
            get
            {
                return "application/x-www-form-urlencoded";
            }
        }

        public OCRGeneralBasicRecognition(Texture2D image, RecognizeGranularity granularity = RecognizeGranularity.Big, LanguageType language = LanguageType.CHN_ENG, bool isDetectDirection = false, bool isDetectLanguage = false)
            : base(image, granularity, language, isDetectDirection, isDetectLanguage)
        { }

        public OCRGeneralBasicRecognition(string imagePath, RecognizeGranularity granularity = RecognizeGranularity.Big, LanguageType language = LanguageType.CHN_ENG, bool isDetectDirection = false, bool isDetectLanguage = false)
            : base(imagePath, granularity, language, isDetectDirection, isDetectLanguage)
        { }
    }
}