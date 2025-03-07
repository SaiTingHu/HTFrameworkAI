namespace HT.Framework.AI
{
    /// <summary>
    /// 智能管家的行为代理
    /// </summary>
    public abstract class AIButlerAgent
    {
        /// <summary>
        /// 智能管家姓名
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 初始化智能管家
        /// </summary>
        public abstract void InitAgent();
        /// <summary>
        /// 向智能管家发送消息
        /// </summary>
        /// <param name="content">用户消息</param>
        /// <param name="code">代码内容</param>
        /// <param name="folderPath">选择的文件夹路径</param>
        /// <param name="endCallback">结束回调</param>
        public abstract void SendMessage(string content, string code, string folderPath, HTFAction<string> endCallback);
    }
}