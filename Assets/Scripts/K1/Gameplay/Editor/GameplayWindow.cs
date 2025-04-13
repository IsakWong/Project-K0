using System;
using System.Collections.Generic;
using System.Reflection;
using K1.Gameplay;
using K1.UI;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace K1.Editor
{
    public class GameplayWindow : EditorWindow
    {
        [MenuItem("K1/MainWindow")]
        public static void ShowMainEditorWindow()
        {
            // This method is called when the user selects the menu item in the Editor
            GameplayWindow wnd = GetWindow<GameplayWindow>();
        }

        public void OnSelectionChange()
        {
        }

        [SerializeField] TreeViewState m_TreeViewState;

        //The TreeView is not serializable, so it should be reconstructed from the tree data.
        BehaviourTreeView m_SimpleTreeView;

        private void OnEnable()
        {
            // Check whether there is already a serialized view state (state 
            // that survived assembly reloading)
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();
            m_SimpleTreeView = new BehaviourTreeView(m_TreeViewState);
        }

        private Vector2 scrollPos;
        private Vector2 scrollPos2;


        private void OnGUI()
        {
            GUILayout.BeginVertical();
            scrollPos2 = GUILayout.BeginScrollView(scrollPos2, false, true);
            OnGUISelection();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void OnInspectorCharacterGUI(CharacterUnit target)
        {
            CharacterUnit myObject = (CharacterUnit)target;
            if (myObject.FSM != null)
            {
                if (GUILayout.Button(myObject.FSM.ActiveState.ToString()))
                {
                    Selection.activeGameObject = myObject.gameObject;
                }
            }

            var controller = target.gameObject.GetComponent<AIYasuoBossController>();
            if (controller != null)
            {
                var type = typeof(AIYasuoBossController);
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var m in methods)
                {
                    if (m.Name.StartsWith("Init") || m.Name.StartsWith("Pattern"))
                    {
                        if (GUILayout.Button(m.Name))
                        {
                            controller.SwitchBehaviour(m.Name);
                        }
                    }
                }

                if (m_SimpleTreeView != null)
                {
                    if (controller.CurrentTree != null)
                    {
                        m_SimpleTreeView.SetBehaviourTree(controller.CurrentTree);
                    }
                    else
                    {
                        m_SimpleTreeView.SetBehaviourTree(null);
                    }

                    GUILayout.BeginVertical();
                    Rect treeViewRect = GUILayoutUtility.GetRect(0, 1000, 400, 1000);
                    m_SimpleTreeView.OnGUI(treeViewRect);
                    GUILayout.EndVertical();
                }
            }

            foreach (var abi in myObject.Abilities)
            {
                var act = abi as ActionAbility;
                if (act == null)
                    continue;
                GUILayout.BeginHorizontal();

                GUILayout.Label(abi.ToString());
                if (GUILayout.Button("Cast"))
                {
                    switch (act.CastType)
                    {
                        case ActionAbility.ActionCastType.Location:
                            act.BeginAtLocation(myObject.transform.position +
                                                myObject.transform.forward * 5.0f);
                            break;
                        case ActionAbility.ActionCastType.NoTarget:
                            act.BeginNoTarget();
                            break;
                        default:
                            break;
                    }
                }

                if (act.FSM != null)
                    GUILayout.Label(act.FSM.ActiveStateName.ToString());

                GUILayout.EndHorizontal();
            }

            // Add custom controls
            foreach (var abi in myObject.Buffs)
            {
                GUILayout.Label(abi.GetType().Name);
            }
        }

        private int tab;

        private void OnGUISelection()
        {
            if (EditorApplication.isPlaying)
            {
                var gameplay = KGameCore.SystemAt<GameplayModule>();
                var names = new string[gameplay.Characters.Count];
                int idx = 0;
                foreach (var character in gameplay.Characters)
                {
                    names[idx] = character.name;
                    idx += 1;
                }

                tab = GUILayout.Toolbar(tab, names);
                if (tab < gameplay.Characters.Count)
                    OnInspectorCharacterGUI(gameplay.Characters[tab]);
            }
            else
            {
                var selection = Selection.activeGameObject;
                if (selection)
                {
                    var unit = selection.GetComponent<CharacterUnit>();
                    if (unit != null)
                    {
                        OnInspectorCharacterGUI(unit);
                    }    
                }
            }
        }
    }
}