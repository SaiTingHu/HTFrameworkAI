namespace HT.Framework.AI
{
    /// <summary>
    /// AI助手实用工具
    /// </summary>
    public static class AssistantUtility
    {
        /// <summary>
        /// 拆分AI助手回复的内容
        /// </summary>
        /// <param name="fullContent">完整内容</param>
        /// <param name="think">思考内容</param>
        /// <param name="content">正式内容</param>
        public static void SplitContent(string fullContent, out string think, out string content)
        {
            string[] strs = fullContent.Split("</think>");
            if (strs.Length == 2)
            {
                think = strs[0].Replace("<think>", "").Trim();
                content = strs[1].Trim();
            }
            else
            {
                think = null;
                content = fullContent.Trim();
            }
        }
    }
}