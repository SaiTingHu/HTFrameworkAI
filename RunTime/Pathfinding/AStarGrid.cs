using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("HTFramework.AI.Editor")]

namespace HT.Framework.AI
{
    /// <summary>
    /// A*网格
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("HTFramework AI/A* Grid")]
    public sealed class AStarGrid : HTBehaviour
    {
        /// <summary>
        /// 估价算法类型【请勿在代码中修改】
        /// </summary>
        [SerializeField] internal string EvaluationType = "HT.Framework.AI.AStarManhattan";
        /// <summary>
        /// 网格平面的大小
        /// </summary>
        public Vector2 Size = new Vector2(10, 10);
        /// <summary>
        /// 节点的半径
        /// </summary>
        public float NodeRadius = 0.5f;
        /// <summary>
        /// 是否忽略斜对角
        /// </summary>
        public bool IsIgnoreOblique = false;
        /// <summary>
        /// 是否自动生成网格
        /// </summary>
        public bool IsAutoGenerate = true;
#if UNITY_EDITOR
        /// <summary>
        /// 是否显示节点索引
        /// </summary>
        public bool IsShowIndex = false;
#endif

        //估价算法
        private AStarEvaluation _evaluation;
        //节点的直径
        private float _nodeDiameter;
        //网格节点组的宽度，高度
        private int _nodesWidth, _nodesHeight;
        //网格的所有节点
        private AStarNode[,] _nodes;
        //寻路的结果路径
        private List<AStarNode> _resultPath = new List<AStarNode>();
        //寻可行走节点的结果组
        private List<AStarNode> _resultNodes = new List<AStarNode>();
        //节点的相邻节点组
        private List<AStarNode> _neighbors = new List<AStarNode>();
        //寻路的开启列表
        private List<AStarNode> _openList = new List<AStarNode>();
        //寻路的开启列表
        private HashSet<AStarNode> _openSet = new HashSet<AStarNode>();
        //寻路的关闭列表
        private HashSet<AStarNode> _closeSet = new HashSet<AStarNode>();

        protected override void Awake()
        {
            base.Awake();

            Type type = ReflectionToolkit.GetTypeInRunTimeAssemblies(EvaluationType);
            if (type != null)
            {
                _evaluation = Activator.CreateInstance(type) as AStarEvaluation;
                if (_evaluation == null)
                {
                    Log.Error("A*：初始化网格失败，丢失了估价算法 " + EvaluationType);
                    return;
                }
            }
            else
            {
                Log.Error("A*：初始化网格失败，丢失了估价算法 " + EvaluationType);
                return;
            }

            if (IsAutoGenerate)
            {
                GenerateGrid();
            }
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gizmos.DrawWireCube(transform.position, new Vector3(Size.x, 1, Size.y));

            if (_nodes == null)
                return;

            if (IsShowIndex)
            {
                foreach (AStarNode node in _nodes)
                {
                    UnityEditor.Handles.Label(node.WorldPoint, $"<{node.XIndex},{node.YIndex}>");
                }
            }
            else
            {
                foreach (AStarNode node in _nodes)
                {
                    Gizmos.color = node.IsCanWalk ? Color.white : Color.red;
                    Gizmos.DrawCube(node.WorldPoint, Vector3.one * (_nodeDiameter - 0.1f));
                }

                if (_resultPath != null)
                {
                    for (int i = 0; i < _resultPath.Count; i++)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawCube(_resultPath[i].WorldPoint, Vector3.one * (_nodeDiameter - 0.1f));
                    }
                }

                if (_resultNodes != null)
                {
                    for (int i = 0; i < _resultNodes.Count; i++)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawCube(_resultNodes[i].WorldPoint, Vector3.one * (_nodeDiameter - 0.1f));
                    }
                }
            }
#endif
        }

        /// <summary>
        /// 生成网格
        /// </summary>
        public void GenerateGrid()
        {
            _nodeDiameter = NodeRadius * 2;
            _nodesWidth = Mathf.RoundToInt(Size.x / _nodeDiameter);
            _nodesHeight = Mathf.RoundToInt(Size.y / _nodeDiameter);
            _nodes = new AStarNode[_nodesWidth, _nodesHeight];

            CreateGrid();
        }

        /// <summary>
        /// 寻路
        /// </summary>
        /// <param name="startIndex">起点的索引</param>
        /// <param name="endIndex">终点的索引</param>
        /// <param name="rule">搜寻规则</param>
        /// <returns>结果路径</returns>
        public List<AStarNode> Pathfinding(Vector2Int startIndex, Vector2Int endIndex, AStarRule rule = null)
        {
            if (startIndex.x < 0 || startIndex.x >= _nodesWidth || startIndex.y < 0 || startIndex.y >= _nodesHeight
                || endIndex.x < 0 || endIndex.x >= _nodesWidth || endIndex.y < 0 || endIndex.y >= _nodesHeight)
            {
                Log.Warning("A*：寻路失败，起点或终点的索引超出了网格的大小！");
                return null;
            }

            if (rule != null)
            {
                foreach (AStarNode node in _nodes)
                {
                    rule.Apply(node);
                }
            }
            
            return Pathfinding(_nodes[startIndex.x, startIndex.y], _nodes[endIndex.x, endIndex.y]);
        }

        /// <summary>
        /// 寻路
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <param name="rule">搜寻规则</param>
        /// <returns>结果路径</returns>
        public List<AStarNode> Pathfinding(Vector3 startPoint, Vector3 endPoint, AStarRule rule = null)
        {
            if (rule != null)
            {
                foreach (AStarNode node in _nodes)
                {
                    rule.Apply(node);
                }
            }
            
            return Pathfinding(GetNode(startPoint), GetNode(endPoint));
        }

        /// <summary>
        /// 寻可行走节点
        /// </summary>
        /// <param name="startIndex">起点</param>
        /// <param name="cost">可行走节点到起点的最大估价</param>
        /// <param name="rule">搜寻规则</param>
        /// <returns>结果节点组</returns>
        public List<AStarNode> WalkableNodefinding(Vector2Int startIndex, int cost, AStarRule rule = null)
        {
            if (startIndex.x < 0 || startIndex.x >= _nodesWidth || startIndex.y < 0 || startIndex.y >= _nodesHeight)
            {
                Log.Warning("A*：寻可行走节点失败，起点的索引超出了网格的大小！");
                return null;
            }

            if (rule != null)
            {
                foreach (AStarNode node in _nodes)
                {
                    rule.Apply(node);
                }
            }

            return WalkableNodefinding(_nodes[startIndex.x, startIndex.y], cost);
        }

        /// <summary>
        /// 寻可行走节点
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="cost">可行走节点到起点的最大估价</param>
        /// <param name="rule">搜寻规则</param>
        /// <returns>结果节点组</returns>
        public List<AStarNode> WalkableNodefinding(Vector3 startPoint, int cost, AStarRule rule = null)
        {
            if (rule != null)
            {
                foreach (AStarNode node in _nodes)
                {
                    rule.Apply(node);
                }
            }

            return WalkableNodefinding(GetNode(startPoint), cost);
        }

        private void CreateGrid()
        {
            //从网格平面的左下角坐标开始
            Vector3 startPoint = transform.position - Size.x / 2 * Vector3.right - Size.y / 2 * Vector3.forward;

            for (int i = 0; i < _nodesWidth; i++)
            {
                for (int j = 0; j < _nodesHeight; j++)
                {
                    Vector3 worldPoint = startPoint + Vector3.right * (i * _nodeDiameter + NodeRadius) + Vector3.forward * (j * _nodeDiameter + NodeRadius);
                    _nodes[i, j] = new AStarNode(worldPoint, i, j);
                }
            }
        }

        private AStarNode GetNode(Vector3 worldPoint)
        {
            Vector3 pos = worldPoint - transform.position;

            float percentX = (pos.x + Size.x / 2) / Size.x;
            float percentY = (pos.z + Size.y / 2) / Size.y;

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((_nodesWidth - 1) * percentX);
            int y = Mathf.RoundToInt((_nodesHeight - 1) * percentY);

            return _nodes[x, y];
        }

        private void GetNeighbor(AStarNode node)
        {
            _neighbors.Clear();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    if (IsIgnoreOblique)
                    {
                        if (i == -1 && j == -1) continue;
                        if (i == -1 && j == 1) continue;
                        if (i == 1 && j == -1) continue;
                        if (i == 1 && j == 1) continue;
                    }

                    int x = node.XIndex + i;
                    int y = node.YIndex + j;

                    if (x >= 0 && x < _nodesWidth && y >= 0 && y < _nodesHeight)
                    {
                        _neighbors.Add(_nodes[x, y]);
                    }
                }
            }
        }

        private List<AStarNode> Pathfinding(AStarNode startNode, AStarNode endNode)
        {
            //重置起点的估价
            startNode.GCost = 0;
            startNode.HCost = 0;

            _openList.Clear();
            _openSet.Clear();
            _closeSet.Clear();

            //将开始节点加入到开启列表中
            _openList.Add(startNode);
            _openSet.Add(startNode);
            while (_openList.Count > 0)
            {
                //从开启列表中找到总估价最低，且距离终点最近的点
                AStarNode currentNode = _openList[0];
                for (int i = 1; i < _openList.Count; i++)
                {
                    if (_openList[i].TotalCost <= currentNode.TotalCost && _openList[i].HCost < currentNode.HCost)
                    {
                        currentNode = _openList[i];
                    }
                }
                _openList.Remove(currentNode);
                _openSet.Remove(currentNode);
                _closeSet.Add(currentNode);

                //如果当前为终点节点，则创建最终路径 
                if (currentNode == endNode)
                {
                    return GeneratePath(startNode, endNode);
                }

                //遍历当前节点的相邻节点，更新估价并加入开启列表
                GetNeighbor(currentNode);
                for (int i = 0; i < _neighbors.Count; i++)
                {
                    if (!_neighbors[i].IsCanWalk || _closeSet.Contains(_neighbors[i]))
                        continue;

                    int gCost = currentNode.GCost + _evaluation.Evaluation(currentNode, _neighbors[i]) + _neighbors[i].OCost;
                    if (gCost < _neighbors[i].GCost || !_openSet.Contains(_neighbors[i]))
                    {
                        _neighbors[i].GCost = gCost;
                        _neighbors[i].HCost = _evaluation.Evaluation(_neighbors[i], endNode);
                        _neighbors[i].Parent = currentNode;
                        if (!_openSet.Contains(_neighbors[i]))
                        {
                            _openList.Add(_neighbors[i]);
                            _openSet.Add(_neighbors[i]);
                        }
                    }
                }
            }

            Log.Warning("A*：寻路失败，未找到合适的路径！");
            return null;
        }
        
        private List<AStarNode> GeneratePath(AStarNode startNode, AStarNode endNode)
        {
            _resultPath.Clear();
            AStarNode temp = endNode;
            while (temp != startNode)
            {
                _resultPath.Add(temp);
                temp = temp.Parent;
            }
            _resultPath.Reverse();
            return _resultPath;
        }

        private List<AStarNode> WalkableNodefinding(AStarNode startNode, int cost)
        {
            IsIgnoreOblique = true;

            //重置起点的估价
            startNode.GCost = 0;
            startNode.HCost = 0;

            _openList.Clear();
            _openSet.Clear();
            _closeSet.Clear();

            //将开始节点加入到开启列表中
            _openList.Add(startNode);
            _openSet.Add(startNode);
            while (_openList.Count > 0)
            {
                //从开启列表中任意取出一个节点
                AStarNode currentNode = _openList[0];

                _openList.Remove(currentNode);
                _openSet.Remove(currentNode);
                _closeSet.Add(currentNode);

                //遍历当前节点的相邻节点，更新估价，估价低于限制估价的加入开启列表，否则加入关闭列表
                GetNeighbor(currentNode);
                for (int i = 0; i < _neighbors.Count; i++)
                {
                    if (!_neighbors[i].IsCanWalk || _closeSet.Contains(_neighbors[i]))
                        continue;

                    int gCost = currentNode.GCost + _evaluation.Evaluation(currentNode, _neighbors[i]) + _neighbors[i].OCost;
                    if (gCost <= cost)
                    {
                        if (gCost < _neighbors[i].GCost || !_openSet.Contains(_neighbors[i]))
                        {
                            _neighbors[i].GCost = gCost;
                            _neighbors[i].Parent = currentNode;
                            if (!_openSet.Contains(_neighbors[i]))
                            {
                                _openList.Add(_neighbors[i]);
                                _openSet.Add(_neighbors[i]);
                            }
                        }
                    }
                    else
                    {
                        _neighbors[i].GCost = gCost;
                        _neighbors[i].Parent = currentNode;

                        if (_openSet.Contains(_neighbors[i]))
                        {
                            _openList.Remove(_neighbors[i]);
                            _openSet.Remove(_neighbors[i]);
                        }
                        _closeSet.Add(_neighbors[i]);
                    }
                }
            }

            //找到所有估价合格的节点
            _resultNodes.Clear();
            foreach (AStarNode node in _closeSet)
            {
                if (node.GCost <= cost)
                {
                    _resultNodes.Add(node);
                }
            }
            return _resultNodes;
        }
    }
}
