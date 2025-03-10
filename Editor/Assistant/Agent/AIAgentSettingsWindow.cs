using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    internal sealed class AIAgentSettingsWindow : HTFEditorWindow
    {
        public static void OpenWindow(AssistantWindow assistantWindow)
        {
            AIAgentSettingsWindow window = GetWindow<AIAgentSettingsWindow>();
            window.titleContent.text = "AIAgent Settings";
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

            _agent = EditorPrefs.GetString(EditorPrefsTableAI.AIAgent_Type, "<None>");
            _permissionOpenURL = EditorPrefs.GetBool(EditorPrefsTableAI.AIAgent_PermissionOpenURL, false);
            _permissionOpenProgram = EditorPrefs.GetBool(EditorPrefsTableAI.AIAgent_PermissionOpenProgram, false);
            _permissionRunCode = EditorPrefs.GetBool(EditorPrefsTableAI.AIAgent_PermissionRunCode, false);
            _permissionReadFile = EditorPrefs.GetBool(EditorPrefsTableAI.AIAgent_PermissionReadFile, false);
            _permissionWriteFile = EditorPrefs.GetBool(EditorPrefsTableAI.AIAgent_PermissionWriteFile, false);
        }
        protected override void OnBodyGUI()
        {
            base.OnBodyGUI();

            GUILayout.BeginHorizontal();
            GUILayout.Label("智能体类型", GUILayout.Width(120));
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
                    return type.IsSubclassOf(typeof(AIAgent)) && !type.IsAbstract;
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
            GUILayout.Label("智能体权限：");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("    访问 Web 网站", GUILayout.Width(120));
            _permissionOpenURL = EditorGUILayout.Toggle(_permissionOpenURL);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("    访问外部程序", GUILayout.Width(120));
            _permissionOpenProgram = EditorGUILayout.Toggle(_permissionOpenProgram);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("    运行代码", GUILayout.Width(120));
            _permissionRunCode = EditorGUILayout.Toggle(_permissionRunCode);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("    读取文件", GUILayout.Width(120));
            _permissionReadFile = EditorGUILayout.Toggle(_permissionReadFile);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("    写入文件", GUILayout.Width(120));
            _permissionWriteFile = EditorGUILayout.Toggle(_permissionWriteFile);
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存", "ButtonLeft"))
            {
                EditorPrefs.SetString(EditorPrefsTableAI.AIAgent_Type, _agent);
                EditorPrefs.SetBool(EditorPrefsTableAI.AIAgent_PermissionOpenURL, _permissionOpenURL);
                EditorPrefs.SetBool(EditorPrefsTableAI.AIAgent_PermissionOpenProgram, _permissionOpenProgram);
                EditorPrefs.SetBool(EditorPrefsTableAI.AIAgent_PermissionRunCode, _permissionRunCode);
                EditorPrefs.SetBool(EditorPrefsTableAI.AIAgent_PermissionReadFile, _permissionReadFile);
                EditorPrefs.SetBool(EditorPrefsTableAI.AIAgent_PermissionWriteFile, _permissionWriteFile);
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