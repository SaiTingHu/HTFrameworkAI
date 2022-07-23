using UnityEngine;

namespace HT.Framework.AI
{
    /// <summary>
    /// A*节点
    /// </summary>
    public sealed class AStarNode
    {
        /// <summary>
        /// 节点是否可以通行
        /// </summary>
        public bool IsCanWalk { get; set; } = true;
        /// <summary>
        /// 节点的世界坐标
        /// </summary>
        public Vector3 WorldPoint { get; private set; }
        /// <summary>
        /// 节点在网格上的X位置
        /// </summary>
        public int XIndex { get; private set; }
        /// <summary>
        /// 节点在网格上的Y位置
        /// </summary>
        public int YIndex { get; private set; }
        /// <summary>
        /// 当前节点自身的固定估价
        /// </summary>
        public int OCost { get; set; } = 0;
        /// <summary>
        /// 起始点到当前节点的估价
        /// </summary>
        public int GCost { get; internal set; } = 0;
        /// <summary>
        /// 当前节点到终点的估价
        /// </summary>
        public int HCost { get; internal set; } = 0;
        /// <summary>
        /// 父节点
        /// </summary>
        public AStarNode Parent { get; internal set; }

        /// <summary>
        /// 总估价
        /// </summary>
        public int TotalCost
        {
            get
            {
                return GCost + HCost + OCost;
            }
        }
        
        public AStarNode(Vector3 worldPoint, int xIndex, int yIndex)
        {
            WorldPoint = worldPoint;
            XIndex = xIndex;
            YIndex = yIndex;
        }
    }
}
