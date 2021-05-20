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
        protected override bool IsEnableRuntimeData
        {
            get
            {
                return false;
            }
        }

        protected override void OnInspectorDefaultGUI()
        {
            base.OnInspectorDefaultGUI();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Evaluation Type", GUILayout.Width(LabelWidth));
            if (GUILayout.Button(Target.EvaluationType, EditorGlobalTools.Styles.MiniPopup))
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

            GUILayout.BeginHorizontal();
            Vector2Field(Target.Size, out Target.Size, "Size");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            FloatField(Target.NodeRadius, out Target.NodeRadius, "Node Radius");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Toggle(Target.IsIgnoreOblique, out Target.IsIgnoreOblique, "Ignore Oblique");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Toggle(Target.IsAutoGenerate, out Target.IsAutoGenerate, "Auto Generate");
            GUILayout.EndHorizontal();
        }
    }
}
