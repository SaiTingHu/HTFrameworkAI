using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    [CustomEditor(typeof(AStarGrid))]
    public sealed class AStarGridInspector : ModuleEditor
    {
        private AStarGrid _target;

        private void OnEnable()
        {
            _target = target as AStarGrid;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Evaluation Type ", GUILayout.Width(100));
            if (GUILayout.Button(_target.EvaluationType, "MiniPopup"))
            {
                GenericMenu gm = new GenericMenu();
                List<Type> types = GlobalTools.GetTypesInRunTimeAssemblies();
                for (int i = 0; i < types.Count; i++)
                {
                    if (types[i].BaseType == typeof(AStarEvaluation))
                    {
                        int j = i;
                        gm.AddItem(new GUIContent(types[j].FullName), _target.EvaluationType == types[j].FullName, () =>
                        {
                            Undo.RecordObject(target, "Set Evaluation");
                            _target.EvaluationType = types[j].FullName;
                            HasChanged();
                        });
                    }
                }
                gm.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Size", GUILayout.Width(100));
            Vector2Field(_target.Size, out _target.Size, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Node Radius", GUILayout.Width(100));
            FloatField(_target.NodeRadius, out _target.NodeRadius, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Toggle(_target.IsIgnoreOblique, out _target.IsIgnoreOblique, "Ignore Oblique");
            GUILayout.EndHorizontal();
        }
    }
}
