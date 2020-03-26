using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HT.Framework.AI
{
    /// <summary>
    /// HT.Framework.AI编辑器全局工具
    /// </summary>
    public static class EditorGlobalToolsAI
    {
        #region 层级视图新建菜单
        /// <summary>
        /// 新建A*网格
        /// </summary>
        [@MenuItem("GameObject/HTFramework.AI/A* Grid", false, 0)]
        private static void CreateAStarGrid()
        {
            GameObject aStar = new GameObject();
            aStar.name = "New AStarGrid";
            aStar.transform.localPosition = Vector3.zero;
            aStar.transform.localRotation = Quaternion.identity;
            aStar.transform.localScale = Vector3.one;
            aStar.AddComponent<AStarGrid>();
            Selection.activeGameObject = aStar;
            EditorSceneManager.MarkSceneDirty(aStar.scene);
        }
        #endregion
    }
}