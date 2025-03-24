using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HT.Framework.AI
{
    /// <summary>
    /// AI编辑器全局工具
    /// </summary>
    public static class EditorGlobalToolsAI
    {
        #region AI 【优先级200】
        /// <summary>
        /// 打开 Assistant Window
        /// </summary>
        [MenuItem("HTFramework/★ AI/Assistant &A", false, 200)]
        private static void OpenAssistantWindow()
        {
            AssistantWindow.OpenWindow(null);
        }

        /// <summary>
        /// 打开 Editor Character Recognitioner
        /// </summary>
        [MenuItem("HTFramework/★ AI/Character Recognitioner", false, 220)]
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
        [MenuItem("HTFramework/★ AI/Speecher", false, 221)]
        private static void OpenEditorSpeecher()
        {
            EditorSpeecher window = EditorWindow.GetWindow<EditorSpeecher>();
            window.titleContent.text = "Speecher";
            window.minSize = new Vector2(400, 400);
            window.maxSize = new Vector2(400, 400);
            window.Show();
        }

        [MenuItem("CONTEXT/Component/★ Ask AI Assistant")]
        public static void AskAIAssistantComponent(MenuCommand cmd)
        {
            if (cmd.context is Component)
            {
                if (cmd.context is MonoBehaviour && cmd.context is not UIBehaviour)
                {
                    Log.Warning("继承至 MonoBehaviour 的脚本不支持 Ask AI Assistant。");
                }
                else
                {
                    Component component = cmd.context as Component;
                    AssistantWindow.OpenWindow($"Unity引擎中{component.GetType().Name}组件的参数及用法？");
                }
            }
        }
        #endregion

        #region 层级视图新建菜单 【优先级200】
        /// <summary>
        /// 新建A*网格
        /// </summary>
        [MenuItem("GameObject/HTFramework/★ AI/A* Grid", false, 200)]
        private static void CreateAStarGrid()
        {
            GameObject[] objs = Selection.gameObjects;
            if (objs == null || objs.Length == 0)
            {
                GameObject aStar = new GameObject();
                aStar.name = "New AStarGrid";
                aStar.transform.localPosition = Vector3.zero;
                aStar.transform.localRotation = Quaternion.identity;
                aStar.transform.localScale = Vector3.one;
                aStar.AddComponent<AStarGrid>();
                Selection.activeGameObject = aStar;
                EditorUtility.SetDirty(aStar);
                EditorSceneManager.MarkSceneDirty(aStar.scene);
            }
            else
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    objs[i].AddComponent<AStarGrid>();
                    EditorUtility.SetDirty(objs[i]);
                    EditorSceneManager.MarkSceneDirty(objs[i].scene);
                }
            }
        }
        #endregion
    }
}