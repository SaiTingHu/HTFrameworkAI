using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    internal sealed class AIButlerSettingsWindow : HTFEditorWindow
    {
        public static void OpenWindow(AssistantWindow assistantWindow)
        {
            AIButlerSettingsWindow window = GetWindow<AIButlerSettingsWindow>();
            window.titleContent.text = "AIButler Settings";
            window._assistantWindow = assistantWindow;
            window.minSize = new Vector2(300, 150);
            window.maxSize = new Vector2(300, 150);
            window.Show();
        }

        private AssistantWindow _assistantWindow;
        private string _agent;

        protected override void OnEnable()
        {
            base.OnEnable();

            _agent = EditorPrefs.GetString(EditorPrefsTableAI.Assistant_AIButlerAgent, "<None>");
        }
        protected override void OnBodyGUI()
        {
            base.OnBodyGUI();

            GUILayout.BeginHorizontal();
            GUILayout.Label("智能管家代理", GUILayout.Width(100));
            if (GUILayout.Button(_agent, EditorStyles.popup))
            {
                GenericMenu gm = new GenericMenu();
                gm.AddItem(new GUIContent("<None>"), _agent == "<None>", () =>
                {
                    _agent = "<None>";
                });
                gm.AddSeparator("");
                List<Type> types = ReflectionToolkit.GetTypesInAllAssemblies(type =>
                {
                    return type.IsSubclassOf(typeof(AIButlerAgent)) && !type.IsAbstract;
                }, false);
                for (int i = 0; i < types.Count; i++)
                {
                    int j = i;
                    gm.AddItem(new GUIContent(types[j].FullName), _agent == types[j].FullName, () =>
                    {
                        _agent = types[j].FullName;
                    });
                }
                gm.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存", "ButtonLeft"))
            {
                EditorPrefs.SetString(EditorPrefsTableAI.Assistant_AIButlerAgent, _agent);
                Close();
            }
            if (GUILayout.Button("取消", "ButtonRight"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }
        private void Update()
        {
            if (_assistantWindow == null)
            {
                Close();
            }
        }
    }
}