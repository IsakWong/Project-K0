using System;
using System.Reflection;
using K1.Gameplay;
using K1.Gameplay.AI;
using TMPro;
using UnityEditor;
using UnityEngine;

public enum AIBossPhase
{
    Normal,
    Rage0,
    Rage1,
    Rage2,
}

public enum AIBossState
{
    None,
    Idle = 1,

    Patrol = Idle + 100,

    Battle = Patrol + 100,
    Battle1,

    Confrontation = Battle + 300,
}

public enum AIBossDistance
{
    Unknown,
    In0_5,
    In5_10,
    In10_20,
    Greater20,
};

public class AICharacterController : AIController
{
    public TextMeshProUGUI DebugText;
    public CharacterUnit ControlCharacter;

    // Start is called before the first frame update
    public Vector3 AwakeLocation;
    public SerializedBT BT;
    protected string CurrentPattern = "";
    public string DefaultPattern = "InitBehaviorBattle";


    public AIBossState BossState = AIBossState.Idle;
    public AIBossPhase BossPhase = AIBossPhase.Normal;
    public AIBossDistance AIBossDistance = AIBossDistance.Unknown;


    public Action OnTargetStunned;

    public AICharacterBehaviorTreeBuilder NewTreeBuilder()
    {
        var builder = new AICharacterBehaviorTreeBuilder(gameObject);
        builder.TickDelta = TickInterval;
        builder.OwnerController = this;
        return builder;
    }

    protected virtual void OnTargetCasting(CharacterUnit target, ActionAbility abi)
    {
    }

    public void OnTargetChange(CharacterUnit old, CharacterUnit value)
    {
        if (old != null)
            old.CharEvent.OnAbilityCasting -= OnTargetCasting;
        if (value != null)
            value.CharEvent.OnAbilityCasting += OnTargetCasting;
    }

    public void SwitchBehaviour(string behaviour)
    {
        var builder = NewTreeBuilder();
        CurrentBuilder = builder;

        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (var m in methods)
        {
            if (m.Name == behaviour)
            {
                builder.Sequence();
                builder.DetectEnemy();
                m.Invoke(this, null);
                builder.End();
                break;
            }
        }

        SwitchTreeBehaviour(CurrentBuilder);
    }

    public void SwitchTreeBehaviour(AICharacterBehaviorTreeBuilder Tree)
    {
        CurrentBuilder = Tree;
        CurrentTree = Tree.Build();
    }

    private AICharacterBehaviorTreeBuilder _builder;

    public AICharacterBehaviorTreeBlackboard CurrentBlackBoard = new AICharacterBehaviorTreeBlackboard();
    public AICharacterBehaviorTreeBuilder CurrentBuilder
    {
        get { return _builder; }
        set { _builder = value; }
    }

    protected int HitCount = 0;

    protected int StunnedCount = 0;
    protected void Awake()
    {
        if (ControlCharacter == null)
            ControlCharacter = GetComponent<CharacterUnit>();
        AwakeLocation = ControlCharacter.transform.position;


        ControlCharacter.CharEvent.OnTakeDamage += (unit, unit2, param) => { HitCount += 1; };
        ControlCharacter.CharEvent.OnGetStuned += (unit, unit2, param) => { StunnedCount += 1; };
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (CurrentBuilder != null && CurrentBuilder.TargetUnit)
        {
            Gizmos.color = Color.blue;

            Handles.Label(CurrentBuilder.Walk_TargetLocation, "WalkTargetLocation");
            Gizmos.DrawSphere(CurrentBuilder.Walk_TargetLocation, 0.4f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(CurrentBuilder.CustomCastLocation, 0.4f);
            Handles.Label(CurrentBuilder.CustomCastLocation, "CastLocation");

            Handles.ArrowHandleCap(
                0,
                CurrentBuilder.ControlCharacter.WorldPosition,
                Quaternion.LookRotation(CurrentBuilder.DirectionToTarget),
                2.0f,
                EventType.Repaint
            );
            var pos = CurrentBuilder.ControlCharacter.WorldPosition;
            pos.y += 2.0f;
            Handles.Label(pos,
                $"P: {CurrentPattern} D:{CurrentBuilder.DistanceToTarget}, A:{CurrentBuilder.TargetAngle}");
        }
    }
#endif
    public override void OnLogic()
    {
        base.OnLogic();

        if (CurrentBuilder.TargetUnit != null)
        {
            if (CurrentBuilder.DistanceToTarget < 5)
            {
                AIBossDistance = AIBossDistance.In0_5;
            }
            else if (CurrentBuilder.DistanceToTarget < 10.0f)
            {
                AIBossDistance = AIBossDistance.In5_10;
            }
            else if (CurrentBuilder.DistanceToTarget < 20.0f)
            {
                AIBossDistance = AIBossDistance.In10_20;
            }
        }
    }
}