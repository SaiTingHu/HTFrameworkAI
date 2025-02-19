using UnityEditor;
using UnityEngine;

namespace HT.Framework.AI
{
    internal sealed class AssistantPromptWordWindow : HTFEditorWindow
    {
        public static void OpenWindow(AssistantWindow assistantWindow, ChatSession chatSession)
        {
            AssistantPromptWordWindow window = GetWindow<AssistantPromptWordWindow>();
            window.titleContent.text = "Assistant PromptWords";
            window._assistantWindow = assistantWindow;
            window._chatSession = chatSession;
            window._content = chatSession.PromptWords.Content;
            window.Show();
        }

        private AssistantWindow _assistantWindow;
        private ChatSession _chatSession;
        private string _content;
        private GUIContent _deleteGC;
        private Vector2 _scroll;

        protected override void OnEnable()
        {
            base.OnEnable();

            _deleteGC = new GUIContent();
            _deleteGC.image = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
            _deleteGC.tooltip = "清空提示词";
        }
        protected override void OnTitleGUI()
        {
            base.OnTitleGUI();

            GUILayout.Label("【智能体提示词】" + _chatSession.Name);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_deleteGC, EditorStyles.iconButton))
            {
                _content = null;
            }
        }
        protected override void OnBodyGUI()
        {
            base.OnBodyGUI();

            _scroll = GUILayout.BeginScrollView(_scroll);
            _content = EditorGUILayout.TextArea(_content);
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存", "ButtonLeft"))
            {
                _chatSession.SetPromptWords(_content);
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