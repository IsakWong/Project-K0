using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using K1.Gameplay;
using K1.Gameplay.AI;
using UnityEngine;
using UnityEngine.Serialization;


public class ScopeSequence : IDisposable
{
    public ScopeSequence(AICharacterBehaviorTreeBuilder builder, string name = "Sequence",
        Action start = null, Action exit = null)
    {
        Builder = builder;
        Builder.SequenceCallback(name, start, exit);
    }

    public AICharacterBehaviorTreeBuilder Builder;

    public void Dispose()
    {
        Builder.End();
    }
}

public class ScopeParallel : IDisposable
{
    public ScopeParallel(AICharacterBehaviorTreeBuilder builder, string name = "Parallel")
    {
        Builder = builder;
        Builder.Parallel(name);
    }

    public AICharacterBehaviorTreeBuilder Builder;

    public void Dispose()
    {
        Builder.End();
    }
}


public class ScopeSelector : IDisposable
{
    public ScopeSelector(AICharacterBehaviorTreeBuilder builder, string name = "Selector")
    {
        Builder = builder;
        Builder.Selector(name);
    }

    public AICharacterBehaviorTreeBuilder Builder;

    public void Dispose()
    {
        Builder.End();
    }
}

public class ScopeRandomSelector : IDisposable
{
    public ScopeRandomSelector(AICharacterBehaviorTreeBuilder builder, string name = "RandomSelector")
    {
        Builder = builder;
        Builder.SelectorRandom(name);
    }

    public AICharacterBehaviorTreeBuilder Builder;

    public void Dispose()
    {
        Builder.End();
    }
}

public class AIController : ControllerBase
{
    public BehaviorTree _currentTree;

    public BehaviorTree CurrentTree
    {
        get { return _currentTree; }
        set { _currentTree = value; }
    }

    protected float _lastTick = 0.0f;
    public float TickInterval = 0.05f;

    // Update is called once per frame
    public override void OnLogic()
    {
        base.OnLogic();
        if (_lastTick <= 0 && CurrentTree != null)
        {
            CurrentTree.Tick();
            _lastTick = TickInterval;
        }
        else
        {
            _lastTick -= KTime.scaleDeltaTime;
        }
    }
}