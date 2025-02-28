namespace HT.Framework.AI
{
    /// <summary>
    /// HT.Framework.AI编辑器配置表
    /// </summary>
    internal static class EditorPrefsTableAI
    {
        #region Editor PrefsKey
        /// <summary>
        /// APIKEY
        /// </summary>
        public static readonly string Speech_APIKEY = "HT.Framework.AI.Speech.APIKEY";
        /// <summary>
        /// SECRETKEY
        /// </summary>
        public static readonly string Speech_SECRETKEY = "HT.Framework.AI.Speech.SECRETKEY";
        /// <summary>
        /// TOKEN
        /// </summary>
        public static readonly string Speech_TOKEN = "HT.Framework.AI.Speech.TOKEN";

        /// <summary>
        /// APIKEY
        /// </summary>
        public static readonly string CR_APIKEY = "HT.Framework.AI.CR.APIKEY";
        /// <summary>
        /// SECRETKEY
        /// </summary>
        public static readonly string CR_SECRETKEY = "HT.Framework.AI.CR.SECRETKEY";
        /// <summary>
        /// TOKEN
        /// </summary>
        public static readonly string CR_TOKEN = "HT.Framework.AI.CR.TOKEN";

        /// <summary>
        /// AI助手的 Model 配置
        /// </summary>
        public static readonly string Assistant_Model = "HT.Framework.AI.Assistant.Model";
        /// <summary>
        /// AI助手的 Stream 配置
        /// </summary>
        public static readonly string Assistant_Stream = "HT.Framework.AI.Assistant.Stream";
        /// <summary>
        /// AI助手的 BaseAddress 配置
        /// </summary>
        public static readonly string Assistant_BaseAddress = "HT.Framework.AI.Assistant.BaseAddress";
        /// <summary>
        /// AI助手的 API 配置
        /// </summary>
        public static readonly string Assistant_API = "HT.Framework.AI.Assistant.API";
        /// <summary>
        /// AI助手的 Timeout 配置
        /// </summary>
        public static readonly string Assistant_Timeout = "HT.Framework.AI.Assistant.Timeout";
        /// <summary>
        /// AI助手的 Round 配置
        /// </summary>
        public static readonly string Assistant_Round = "HT.Framework.AI.Assistant.Round";
        /// <summary>
        /// AI助手的 IsLogInEditor 配置
        /// </summary>
        public static readonly string Assistant_IsLogInEditor = "HT.Framework.AI.Assistant.IsLogInEditor";
        /// <summary>
        /// AI助手的 ShowThink 配置
        /// </summary>
        public static readonly string Assistant_ShowThink = "HT.Framework.AI.Assistant.ShowThink";
        /// <summary>
        /// AI助手的 SavePath 配置
        /// </summary>
        public static readonly string Assistant_SavePath = "HT.Framework.AI.Assistant.SavePath";
        /// <summary>
        /// AI助手的 EnableAIButler 配置
        /// </summary>
        public static readonly string Assistant_EnableAIButler = "HT.Framework.AI.Assistant.EnableAIButler";
        /// <summary>
        /// AI智能管家的 Agent 配置
        /// </summary>
        public static readonly string AIButler_Agent = "HT.Framework.AI.AIButler.Agent";
        /// <summary>
        /// AI智能管家的 Permission.OpenURL 配置
        /// </summary>
        public static readonly string AIButler_PermissionOpenURL = "HT.Framework.AI.AIButler.Permission.OpenURL";
        /// <summary>
        /// AI智能管家的 Permission.OpenProgram 配置
        /// </summary>
        public static readonly string AIButler_PermissionOpenProgram = "HT.Framework.AI.AIButler.Permission.OpenProgram";
        /// <summary>
        /// AI智能管家的 Permission.RunCode 配置
        /// </summary>
        public static readonly string AIButler_PermissionRunCode = "HT.Framework.AI.AIButler.Permission.RunCode";
        /// <summary>
        /// AI智能管家的 Permission.ReadFile 配置
        /// </summary>
        public static readonly string AIButler_PermissionReadFile = "HT.Framework.AI.AIButler.Permission.ReadFile";
        /// <summary>
        /// AI智能管家的 Permission.WriteFile 配置
        /// </summary>
        public static readonly string AIButler_PermissionWriteFile = "HT.Framework.AI.AIButler.Permission.WriteFile";
        #endregion
    }
}