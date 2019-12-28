using UnityEngine;

namespace HT.Framework.AI
{
    /// <summary>
    /// HT.Framework.AI编辑器配置表
    /// </summary>
    public static class EditorPrefsTableAI
    {
        #region Speech
        /// <summary>
        /// APIKEY
        /// </summary>
        public static string Speech_APIKEY
        {
            get
            {
                return Application.productName + ".HT.Framework.AI.Speech.APIKEY";
            }
        }
        /// <summary>
        /// SECRETKEY
        /// </summary>
        public static string Speech_SECRETKEY
        {
            get
            {
                return Application.productName + ".HT.Framework.AI.Speech.SECRETKEY";
            }
        }
        /// <summary>
        /// TOKEN
        /// </summary>
        public static string Speech_TOKEN
        {
            get
            {
                return Application.productName + ".HT.Framework.AI.Speech.TOKEN";
            }
        }
        #endregion
    }
}
