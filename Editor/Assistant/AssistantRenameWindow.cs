using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    internal sealed class AssistantRenameWindow : HTFEditorWindow
    {
        public static void OpenWindow(AssistantWindow assistantWindow, ChatSession chatSession)
        {
            AssistantRenameWindow window = GetWindow<AssistantRenameWindow>();
            window.titleContent.text = "Assistant Rename";
            window._assistantWindow = assistantWindow;
            window._chatSession = chatSession;
            window._name = chatSession.Name;
            window.minSize = new Vector2(300, 100);
            window.maxSize = new Vector2(300, 100);
            window.Show();
        }

        private AssistantWindow _assistantWindow;
        private ChatSession _chatSession;
        private string _name;

        protected override void OnTitleGUI()
        {
            base.OnTitleGUI();

            GUILayout.Label("【重命名】" + _chatSession.Name);
            GUILayout.FlexibleSpace();
        }
        protected override void OnBodyGUI()
        {
            base.OnBodyGUI();

            GUILayout.BeginHorizontal();
            _name = EditorGUILayout.TextField(_name);
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存", "ButtonLeft"))
            {
                _chatSession.Name = _name;
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