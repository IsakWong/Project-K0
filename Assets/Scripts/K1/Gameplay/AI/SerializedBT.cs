using System;
using System.Collections.Generic;
using CleverCrow.Fluid.BTs.Trees;
using K1.Gameplay.AI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace K1.Gameplay
{
    public enum NodeType
    {
        CompositeNode,
        LeafNode,
        TreeNode
    }


    [Serializable]
    public class SerializedNode : ISerializationCallbackReceiver
    {
        public string Name = new string("");
        public NodeType Type = NodeType.LeafNode;
        public List<SerializedNode> Children = new List<SerializedNode>();
        public SerializedBT Tree;

        public string GUID
        {
            get
            {
                AssignGuid();
                return _guid;
            }
        }

        [SerializeField] private string _guid = new string("");

        public void OnBeforeSerialize()
        {
            AssignGuid();
        }

        public void OnAfterDeserialize()
        {
        }

        public void AssignGuid()
        {
            if (_guid == "")
            {
                _guid = Guid.NewGuid().ToString();
            }

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.AssignGuid();
                }
            }
        }
    }

    [CreateAssetMenu(fileName = "BehaviourTree", menuName = "BehaviourTree")]
    public class SerializedBT : ScriptableObject
    {
        public SerializeType<K1BehaviorTreeBuilder> TreeBuilder;
        public SerializedNode Root;
        private AICharacterBehaviorTreeBuilder Builder;

        public void Init(SerializedNode node)
        {
            if (node.Type == NodeType.CompositeNode)
            {
                var method = Builder.GetType().GetMethod(node.Name);
                if (method != null)
                {
                    method.Invoke(Builder, null);
                    foreach (var child in node.Children)
                    {
                        Init(child);
                    }

                    Builder.End();
                }
            }
            else if (node.Type == NodeType.LeafNode)
            {
                var method = Builder.GetType().GetMethod(node.Name);
                if (method != null)
                {
                    method.Invoke(Builder, null);
                }
            }
            else if (node.Type == NodeType.TreeNode)
            {
                node.Tree.BuildTree(Builder);
            }

            foreach (var VARIABLE in node.Children)
            {
                Init(VARIABLE);
            }
        }

        public void BuildTree(AICharacterBehaviorTreeBuilder builder)
        {
            Builder = builder;
            Init(Root);
        }
    }
}