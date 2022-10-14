using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HT.Framework.AI
{
    /// <summary>
    /// AI编辑器全局工具
    /// </summary>
    public static class EditorGlobalToolsAI
    {
        #region AI 【优先级200】
        /// <summary>
        /// 打开 Editor Character Recognitioner
        /// </summary>
        [MenuItem("HTFramework/★ AI/Editor Character Recognitioner", false, 200)]
        private static void OpenEditorCharacterRecognitioner()
        {
            EditorCharacterRecognitioner window = EditorWindow.GetWindow<EditorCharacterRecognitioner>();
            window.titleContent.text = "Character Recognitioner";
            window.minSize = new Vector2(400, 400);
            window.maxSize = new Vector2(400, 400);
            window.Show();
        }

        /// <summary>
        /// 打开 Editor Speecher
        /// </summary>
        [MenuItem("HTFramework/★ AI/Editor Speecher", false, 201)]
        private static void OpenEditorSpeecher()
        {
            EditorSpeecher window = EditorWindow.GetWindow<EditorSpeecher>();
            window.titleContent.text = "Speecher";
            window.minSize = new Vector2(400, 400);
            window.maxSize = new Vector2(400, 400);
            window.Show();
        }
        #endregion

        #region 层级视图新建菜单 【优先级200】
        /// <summary>
        /// 新建A*网格
        /// </summary>
        [MenuItem("GameObject/HTFramework/★ AI/A* Grid", false, 200)]
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