using System.Collections.Generic;

namespace HT.Framework.AI
{
    /// <summary>
    /// 语音合成规则
    /// </summary>
    public sealed class SynthesisRule
    {
        private Dictionary<string, CustomTone> _customTones = new Dictionary<string, CustomTone>();

        /// <summary>
        /// 添加自定义音标
        /// </summary>
        public void AddCustomTone(string word, string pinyin, int tone)
        {
            if (_customTones.ContainsKey(word))
            {
                CustomTone ct = _customTones[word];
                ct.Word = word;
                ct.Pinyin = pinyin;
                ct.Tone = tone;
            }
            else
            {
                _customTones.Add(word, new CustomTone(word, pinyin, tone));
            }
        }
        /// <summary>
        /// 移除自定义音标
        /// </summary>
        public void RemoveCustomTone(string word)
        {
            if (_customTones.ContainsKey(word))
            {
                _customTones.Remove(word);
            }
        }
        /// <summary>
        /// 清空所有自定义音标
        /// </summary>
        public void ClearCustomTone()
        {
            _customTones.Clear();
        }
        /// <summary>
        /// 应用所有自定义音标
        /// </summary>
        public string ApplyCustomTone(string synthesisText)
        {
            foreach (KeyValuePair<string, CustomTone> customTone in _customTones)
            {
                synthesisText = synthesisText.Replace(customTone.Key, customTone.Value.ToString());
            }
            return synthesisText;
        }

        /// <summary>
        /// 自定义音标
        /// </summary>
        public sealed class CustomTone
        {
            /// <summary>
            /// 文字
            /// </summary>
            public string Word;
            /// <summary>
            /// 汉语拼音
            /// </summary>
            public string Pinyin;
            /// <summary>
            /// 音调，对应1、2、3、4声
            /// </summary>
            public int Tone;

            public CustomTone(string word, string pinyin, int tone)
            {
                Word = word;
                Pinyin = pinyin;
                Tone = tone;
            }

            public override string ToString()
            {
                return string.Format("{0}({1}{2})", Word, Pinyin, Tone);
            }
        }
    }
}
