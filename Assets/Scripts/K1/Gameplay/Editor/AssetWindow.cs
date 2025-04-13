using System;
using System.Collections.Generic;
using System.Reflection;
using K1.Gameplay;
using K1.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace K1.Editor
{
    public class AssetWindow : EditorWindow
    {
        private List<ActionAbilityConfig> actionConfigs = new List<ActionAbilityConfig>();
        private List<BuffConfig> buffConfigs = new List<BuffConfig>();
        private List<GameObject> Panels = new List<GameObject>();
        private List<CharacterConfig> charConfigs = new List<CharacterConfig>();
        private List<AnimatorController> animators = new List<AnimatorController>();

        private Dictionary<string, List<Object>> assets = new();
        private List<Tuple<string, string, Type>> filters = new();

        [MenuItem("K1/AssetWindow")]
        public static void ShowMainEditorWindow()
        {
            // This method is called when the user selects the menu item in the Editor
            AssetWindow wnd = GetWindow<AssetWindow>();
            wnd.RefreshAssets();
        }


        public void RefreshAssets()
        {
            filters.Clear();
            filters.Add(new Tuple<string, string, Type>("t:ActionAbilityConfig", "Assets/Gameplay",
                typeof(ActionAbilityConfig)));
            filters.Add(new Tuple<string, string, Type>("Panel", "Assets/Resources/UI", typeof(GameObject)));
            filters.Add(new Tuple<string, string, Type>("t:BuffConfig", "Assets/Gameplay", typeof(BuffConfig)));
            filters.Add(
                new Tuple<string, string, Type>("t:CharacterConfig", "Assets/Gameplay", typeof(CharacterConfig)));
            filters.Add(new Tuple<string, string, Type>("t:AnimatorController", "Assets/Gameplay",
                typeof(AnimatorController)));
            foreach (var filter in filters)
            {
                var paths = AssetDatabase.FindAssets(filter.Item1, new[] { filter.Item2 });
                assets[filter.Item3.Name] = new List<Object>();
                foreach (var path in paths)
                {
                    var obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(path), filter.Item3);
                    if (obj != null)
                        assets[filter.Item3.Name].Add(obj);
                }
                assets[filter.Item3.Name].Sort((a, b) => a.ToString().CompareTo(b.ToString()));
            }
        }

        public void OnSelectionChange()
        {
        }

        private Vector2 scrollPos;
        private Vector2 scrollPos2;

        private int tab;

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button("刷新"))
                RefreshAssets();
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            foreach (var config in assets)
            {
                GUILayout.Label(config.Key);
                foreach (var it in config.Value)
                {
                    if (GUILayout.Button(it.ToString()))
                    {
                        Selection.objects = new Object[] { it };
                    }
                }
            }
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

            foreach (var abi in myObject.Buffs)
            {
                GUILayout.Label(abi.GetType().Name);
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
                            act.BeginAtLocation(myObject.transform.position + myObject.transform.forward * 3.0f);
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
        }

        private void OnGUISelection()
        {
            if (EditorApplication.isPlaying)
            {
                var mod = KGameCore.SystemAt<GameplayModule>();
                foreach (var character in mod.Characters)
                {
                    OnInspectorCharacterGUI(character);
                }
            }
        }
    }
}