using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    [CustomEditor(typeof(AStarGrid))]
    public sealed class AStarGridInspector : HTFEditor<AStarGrid>
    {
        protected override void OnInspectorDefaultGUI()
        {
            base.OnInspectorDefaultGUI();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Evaluation Type ", GUILayout.Width(100));
            if (GUILayout.Button(Target.EvaluationType, "MiniPopup"))
            {
                GenericMenu gm = new GenericMenu();
                List<Type> types = GlobalTools.GetTypesInRunTimeAssemblies();
                for (int i = 0; i < types.Count; i++)
                {
                    if (types[i].BaseType == typeof(AStarEvaluation))
                    {
                        int j = i;
                        gm.AddItem(new GUIContent(types[j].FullName), Target.EvaluationType == types[j].FullName, () =>
                        {
                            Undo.RecordObject(target, "Set Evaluation");
                            Target.EvaluationType = types[j].FullName;
                            HasChanged();
                        });
                    }
                }
                gm.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Size", GUILayout.Width(100));
            Vector2Field(Target.Size, out Target.Size, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Node Radius", GUILayout.Width(100));
            FloatField(Target.NodeRadius, out Target.NodeRadius, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Toggle(Target.IsIgnoreOblique, out Target.IsIgnoreOblique, "Ignore Oblique");
            GUILayout.EndHorizontal();
        }
    }
}
