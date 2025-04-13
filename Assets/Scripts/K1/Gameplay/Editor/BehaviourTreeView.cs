using System;
using System.Collections.Generic;
using System.Linq;
using CleverCrow.Fluid.BTs.TaskParents;
using CleverCrow.Fluid.BTs.TaskParents.Composites;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Tasks.Actions;
using CleverCrow.Fluid.BTs.Trees;
using K1.Gameplay;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[Serializable]
public class BehaviorTreeNodeItem : TreeViewItem
{
    public ITask Task;
    public string customData;
}

public class BehaviourTreeView : TreeView
{
    public BehaviourTreeView(TreeViewState treeViewState)
        : base(treeViewState)
    {
        Reload();
    }

    public void GotoItem(ITask task)
    {
        var item = _items[task];
        if (item != null)
        {
            FrameItem(item.id);
        }
    }

    private Dictionary<ITask, BehaviorTreeNodeItem> _items = new();

// 在自定义 TreeView 类中添加
    private int _targetItemId = -1;

    BehaviorTree behaviourTree;

    public void SetBehaviourTree(BehaviorTree behaviourTree)
    {
        if (this.behaviourTree == behaviourTree)
            return;
        this.behaviourTree = behaviourTree;
        Reload();
    }

    int id = 0;

    public void BuildTree(ref List<TreeViewItem> items, ITask task, int depth)
    {
        var item = new BehaviorTreeNodeItem() { id = id, depth = depth, displayName = task.Name, Task = task };
        _items[task] = item;
        items.Add(item);
        id++;
        var parent = task as ITaskParent;
        if (parent != null)
        {
            foreach (var VARIABLE in parent.Children)
            {
                BuildTree(ref items, VARIABLE, depth + 1);
            }
        }
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        var item = (BehaviorTreeNodeItem)args.item;
        ITask task = item.Task;
        BehaviorTree tree = task.ParentTree as BehaviorTree;
        var composite = task as CompositeBase;
        var generic = task as ActionBase;
        var coloredStyle = new GUIStyle(EditorStyles.label);

        if (tree.ActiveTasks.Contains(task))
        {
            coloredStyle.normal.textColor = Color.red;
        }
        
        Rect dataRect = new Rect(args.rowRect);
        dataRect.x += GetContentIndent(item);
        EditorGUI.LabelField(dataRect, item.displayName, coloredStyle);
    }

    protected override TreeViewItem BuildRoot()
    {
        // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
        // are created from data. Here we create a fixed set of items. In a real world example,
        // a data model should be passed into the TreeView and the items created from the model.    
        var allItems = new List<TreeViewItem>();
        _items.Clear();
        TreeViewItem root = new TreeViewItem { id = id, depth = -1, displayName = "Root" };
        if (behaviourTree == null || behaviourTree.Root == null)
        {
            SetupParentsAndChildrenFromDepths(root, allItems);
            // Return root of the tree
            return root;
        }

        foreach (var VARIABLE in behaviourTree.Root.Children)
        {
            BuildTree(ref allItems, VARIABLE, 0);
        }

        // Utility method that initializes the TreeViewItem.children and .parent for all items.
        SetupParentsAndChildrenFromDepths(root, allItems);
        // Return root of the tree
        return root;
    }
}