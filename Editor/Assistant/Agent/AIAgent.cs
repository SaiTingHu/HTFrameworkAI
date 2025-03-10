namespace HT.Framework.AI
{
    /// <summary>
    /// AI智能体
    /// </summary>
    public abstract class AIAgent
    {
        /// <summary>
        /// 智能体名称
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 初始化智能体
        /// </summary>
        public abstract void InitAgent();
        /// <summary>
        /// 向智能体发送指令
        /// </summary>
        /// <param name="content">用户指令</param>
        /// <param name="code">代码内容</param>
        /// <param name="folderPath">选择的文件夹路径</param>
        /// <param name="agentReply">智能体回复（智能体处理当前指令时可能会多次回复，参数1：当前指令是否处理完毕，参数2：回复文本内容）</param>
        public abstract void SendInstruction(string content, string code, string folderPath, HTFAction<bool, string> agentReply);
    }
}