using System.Collections.Generic;

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
        /// 与智能管家的聊天记录
        /// </summary>
        public List<ChatSession.ChatMessage> Messages { get; protected set; } = new List<ChatSession.ChatMessage>();

        /// <summary>
        /// 初始化智能管家
        /// </summary>
        public abstract void InitAgent();
        /// <summary>
        /// 向智能管家发送消息
        /// </summary>
        /// <param name="content">用户消息</param>
        /// <param name="endCallback">结束回调</param>
        public abstract void SendMessage(string content, HTFAction endCallback);
    }
}