namespace HT.Framework.AI
{
    /// <summary>
    /// A*估价算法
    /// </summary>
    public abstract class AStarEvaluation
    {
        /// <summary>
        /// 估价
        /// </summary>
        public abstract int Evaluation(AStarNode nodea, AStarNode nodeb);
    }
}
