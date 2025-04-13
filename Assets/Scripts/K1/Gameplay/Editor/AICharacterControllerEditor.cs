using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace K1.Editor
{
    [CustomEditor(typeof(AICharacterController), true)]
    public class AICharacterControllerEditor : UnityEditor.Editor
    {
        private AICharacterController controller;

        TreeViewState m_TreeViewState;

        //The TreeView is not serializable, so it should be reconstructed from the tree data.
        BehaviourTreeView m_SimpleTreeView;

        void OnEnable()
        {
            controller = serializedObject.targetObject as AICharacterController;
            // Check whether there is already a serialized view state (state 
            // that survived assembly reloading)
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();
            m_SimpleTreeView = new BehaviourTreeView(m_TreeViewState);
        }

        private bool gotoActive = false;

        public void OnInspectorTreeView()
        {
            if (m_SimpleTreeView == null || controller == null)
                return;
            if (controller.CurrentTree != null)
            {
                m_SimpleTreeView.SetBehaviourTree(controller.CurrentTree);
            }
            else
            {
                m_SimpleTreeView.SetBehaviourTree(null);
            }

            // if (controller.CurrentTree != null && controller.CurrentTree.ActiveTasks != null)
            // {
            //     var task = controller.CurrentTree.ActiveTasks[0];
            //     m_SimpleTreeView.GotoItem(task);
            // }

            Rect treeViewRect = GUILayoutUtility.GetRect(0, 1000, 400, 400);
            m_SimpleTreeView.OnGUI(treeViewRect);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            gotoActive = EditorGUILayout.Toggle("Goto", gotoActive);
            OnInspectorTreeView();
            GUILayout.EndVertical();
        }
    }
}