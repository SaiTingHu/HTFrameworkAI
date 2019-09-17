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
        /// <param name="word">文字</param>
        /// <param name="pinyin">拼音</param>
        /// <param name="tone">音标</param>
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
                _customTones.Add(word, Main.m_ReferencePool.Spawn<CustomTone>().Fill(word, pinyin, tone));
            }
        }
        /// <summary>
        /// 移除自定义音标
        /// </summary>
        /// <param name="word">文字</param>
        public void RemoveCustomTone(string word)
        {
            if (_customTones.ContainsKey(word))
            {
                Main.m_ReferencePool.Despawn(_customTones[word]);
                _customTones.Remove(word);
            }
        }
        /// <summary>
        /// 清空所有自定义音标
        /// </summary>
        public void ClearCustomTone()
        {
            foreach (KeyValuePair<string, CustomTone> tone in _customTones)
            {
                Main.m_ReferencePool.Despawn(tone.Value);
            }
            _customTones.Clear();
        }
        /// <summary>
        /// 应用规则
        /// </summary>
        /// <param name="synthesisText">待合成文本</param>
        /// <returns>应用规则后的待合成文本</returns>
        public string Apply(string synthesisText)
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
        public sealed class CustomTone : IReference
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

            public CustomTone()
            {
            }

            public CustomTone Fill(string word, string pinyin, int tone)
            {
                Word = word;
                Pinyin = pinyin;
                Tone = tone;
                return this;
            }

            public void Reset()
            {
                Word = "";
                Pinyin = "";
                Tone = 1;
            }

            public override string ToString()
            {
                return Word + "(" + Pinyin + Tone.ToString() + ")";
            }
        }
    }
}
