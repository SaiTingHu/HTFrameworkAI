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
            window.minSize = new Vector2(300, 200);
            window.maxSize = new Vector2(300, 200);
            window.Show();
        }

        private AssistantWindow _assistantWindow;
        private string _agent;
        private bool _permissionOpenURL;
        private bool _permissionOpenProgram;
        private bool _permissionRunCode;
        private bool _permissionReadFile;
        private bool _permissionWriteFile;

        protected override void OnEnable()
        {
            base.OnEnable();

            _agent = EditorPrefs.GetString(EditorPrefsTableAI.AIButler_Agent, "<None>");
            _permissionOpenURL = EditorPrefs.GetBool(EditorPrefsTableAI.AIButler_PermissionOpenURL, false);
            _permissionOpenProgram = EditorPrefs.GetBool(EditorPrefsTableAI.AIButler_PermissionOpenProgram, false);
            _permissionRunCode = EditorPrefs.GetBool(EditorPrefsTableAI.AIButler_PermissionRunCode, false);
            _permissionReadFile = EditorPrefs.GetBool(EditorPrefsTableAI.AIButler_PermissionReadFile, false);
            _permissionWriteFile = EditorPrefs.GetBool(EditorPrefsTableAI.AIButler_PermissionWriteFile, false);
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

            GUILayout.BeginHorizontal();
            GUILayout.Label("为智能管家开放权限：");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("  访问 Web 网站", GUILayout.Width(100));
            _permissionOpenURL = EditorGUILayout.Toggle(_permissionOpenURL);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("  访问外部程序", GUILayout.Width(100));
            _permissionOpenProgram = EditorGUILayout.Toggle(_permissionOpenProgram);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("  运行代码", GUILayout.Width(100));
            _permissionRunCode = EditorGUILayout.Toggle(_permissionRunCode);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("  读取文件", GUILayout.Width(100));
            _permissionReadFile = EditorGUILayout.Toggle(_permissionReadFile);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("  写入文件", GUILayout.Width(100));
            _permissionWriteFile = EditorGUILayout.Toggle(_permissionWriteFile);
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存", "ButtonLeft"))
            {
                EditorPrefs.SetString(EditorPrefsTableAI.AIButler_Agent, _agent);
                EditorPrefs.SetBool(EditorPrefsTableAI.AIButler_PermissionOpenURL, _permissionOpenURL);
                EditorPrefs.SetBool(EditorPrefsTableAI.AIButler_PermissionOpenProgram, _permissionOpenProgram);
                EditorPrefs.SetBool(EditorPrefsTableAI.AIButler_PermissionRunCode, _permissionRunCode);
                EditorPrefs.SetBool(EditorPrefsTableAI.AIButler_PermissionReadFile, _permissionReadFile);
                EditorPrefs.SetBool(EditorPrefsTableAI.AIButler_PermissionWriteFile, _permissionWriteFile);
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