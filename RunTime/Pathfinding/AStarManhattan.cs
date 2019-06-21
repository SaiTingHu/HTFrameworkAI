using UnityEngine;

namespace HT.Framework.AI
{
    /// <summary>
    /// A*曼哈顿估价算法
    /// </summary>
    public sealed class AStarManhattan : AStarEvaluation
    {
        public override int Evaluation(AStarNode nodea, AStarNode nodeb)
        {
            return Mathf.Abs(nodea.XIndex - nodeb.XIndex) + Mathf.Abs(nodea.YIndex - nodeb.YIndex);
        }
    }
}
