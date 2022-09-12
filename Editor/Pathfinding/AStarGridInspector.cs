using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    [GiteeURL("https://gitee.com/SaiTingHu/HTFrameworkAI")]
    [CSDNBlogURL("https://wanderer.blog.csdn.net/article/details/103761142")]
    [GithubURL("https://github.com/SaiTingHu/HTFrameworkAI")]
    [CustomEditor(typeof(AStarGrid))]
    internal sealed class AStarGridInspector : HTFEditor<AStarGrid>
    {
        protected override void OnInspectorDefaultGUI()
        {
            base.OnInspectorDefaultGUI();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Evaluation Type", GUILayout.Width(LabelWidth));
            if (GUILayout.Button(Target.EvaluationType, EditorGlobalTools.Styles.MiniPopup, GUILayout.Width(EditorGUIUtility.currentViewWidth - LabelWidth - 25)))
            {
                GenericMenu gm = new GenericMenu();
                List<Type> types = ReflectionToolkit.GetTypesInRunTimeAssemblies(type =>
                {
                    return type.IsSubclassOf(typeof(AStarEvaluation)) && !type.IsAbstract;
                });
                for (int i = 0; i < types.Count; i++)
                {
                    int j = i;
                    gm.AddItem(new GUIContent(types[j].FullName), Target.EvaluationType == types[j].FullName, () =>
                    {
                        Undo.RecordObject(target, "Set Evaluation");
                        Target.EvaluationType = types[j].FullName;
                        HasChanged();
                    });
                }
                gm.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            PropertyField(nameof(AStarGrid.Size), "Size");
            PropertyField(nameof(AStarGrid.NodeRadius), "Node Radius");
            PropertyField(nameof(AStarGrid.IsIgnoreOblique), "Ignore Oblique");
            PropertyField(nameof(AStarGrid.IsAutoGenerate), "Auto Generate");
            PropertyField(nameof(AStarGrid.IsHideFindFailedLog), "Hide Find Failed Log");
        }
        protected override void OnInspectorRuntimeGUI()
        {
            base.OnInspectorRuntimeGUI();

            PropertyField(nameof(AStarGrid.IsShowIndex), "Show Index");
        }
    }
}
