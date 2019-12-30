using System.Collections.Generic;
using UnityEngine;

namespace HT.Framework.AI
{
    /// <summary>
    /// 文字识别响应数据
    /// </summary>
    public sealed class OCRResponse
    {
        /// <summary>
        /// ID
        /// </summary>
        public string LogID { get; private set; }
        /// <summary>
        /// 图像方向
        /// </summary>
        public DirectionType Direction { get; private set; }
        /// <summary>
        /// 文字数量
        /// </summary>
        public uint WordsCount { get; private set; }
        /// <summary>
        /// 文字语言
        /// </summary>
        public int Language { get; private set; }
        /// <summary>
        /// 所有文字
        /// </summary>
        public List<Word> Words { get; private set; }

        public OCRResponse(JsonData jsonData)
        {
            LogID = jsonData["log_id"].ToString();
            Direction = jsonData.Keys.Contains("direction") ? (DirectionType)(int)jsonData["direction"] : DirectionType.None;
            WordsCount = uint.Parse(jsonData["words_result_num"].ToString());
            Language = jsonData.Keys.Contains("language") ? (int)jsonData["language"] : -1;

            if (jsonData.Keys.Contains("words_result"))
            {
                Words = new List<Word>();
                JsonData words = jsonData["words_result"];
                for (int i = 0; i < words.Count; i++)
                {
                    Word word = new Word(words[i]);
                    Words.Add(word);
                }
            }
        }
        
        /// <summary>
        /// 文字
        /// </summary>
        public sealed class Word
        {
            /// <summary>
            /// 文字内容
            /// </summary>
            public string Content { get; private set; }
            /// <summary>
            /// 文字位置
            /// </summary>
            public Rect Location { get; private set; }
            /// <summary>
            /// 所有字符
            /// </summary>
            public List<Char> Chars { get; private set; }

            public Word(JsonData word)
            {
                Content = word["words"].ToString();

                if (word.Keys.Contains("location"))
                {
                    Location = new Rect((int)word["location"]["left"], (int)word["location"]["top"], (int)word["location"]["width"], (int)word["location"]["height"]);
                }

                if (word.Keys.Contains("chars"))
                {
                    Chars = new List<Char>();
                    JsonData chars = word["chars"];
                    for (int i = 0; i < chars.Count; i++)
                    {
                        Char cha = new Char(chars[i]);
                        Chars.Add(cha);
                    }
                }
            }
        }

        /// <summary>
        /// 字符
        /// </summary>
        public sealed class Char
        {
            /// <summary>
            /// 字符内容
            /// </summary>
            public char Content { get; private set; }
            /// <summary>
            /// 字符位置
            /// </summary>
            public Rect Location { get; private set; }

            public Char(JsonData cha)
            {
                Content = char.Parse(cha["char"].ToString());

                if (cha.Keys.Contains("location"))
                {
                    Location = new Rect((int)cha["location"]["left"], (int)cha["location"]["top"], (int)cha["location"]["width"], (int)cha["location"]["height"]);
                }
            }
        }

        /// <summary>
        /// 图像方向类型
        /// </summary>
        public enum DirectionType
        {
            /// <summary>
            /// 未定义
            /// </summary>
            None = -1,
            /// <summary>
            /// 正向
            /// </summary>
            Forward = 0,
            /// <summary>
            /// 逆时针90度
            /// </summary>
            Anticlockwise90 = 1,
            /// <summary>
            /// 逆时针180度
            /// </summary>
            Anticlockwise180 = 2,
            /// <summary>
            /// 逆时针270度
            /// </summary>
            Anticlockwise270 = 3,
        }
    }
}