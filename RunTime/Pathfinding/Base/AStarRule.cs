namespace HT.Framework.AI
{
    /// <summary>
    /// A*搜寻规则
    /// </summary>
    public abstract class AStarRule
    {
        /// <summary>
        /// 应用规则
        /// </summary>
        public abstract void Apply(AStarNode node);
    }
}
