using UnityEngine;

namespace K1.Gameplay
{
    public class IdleCommand : Command
    {
        public CharacterUnit Unit;

        public override void Enqueue(CommandQueue queue)
        {
            if (queue.Queue.Count == 0)
                base.Enqueue(queue);
        }

        public override ExecuteResult Execute()
        {
            Unit.Idle();
            return ExecuteResult.Success;
        }
    }

    public class WalkCommand : Command
    {
        public CharacterUnit Unit;
        public Vector3 TargetDirection;
        public Vector3 FaceDirection;

        public override ExecuteResult Execute()
        {
            if (!Unit)
                return ExecuteResult.Fail;
            if (Unit.IsSoloActing)
                return ExecuteResult.Continue;

            Unit.DirectWalk(TargetDirection, FaceDirection);

            return ExecuteResult.Success;
        }
    }

    public class MoveCommand : Command
    {
        public CharacterUnit Unit;
        public Vector3 TargetLocation;

        public override void Enqueue(CommandQueue queue)
        {
            var it = queue.Queue.Last;
            while (it != null)
            {
                var moveCmd = it.Value as MoveCommand;
                it = it.Previous;
                if (moveCmd is not null)
                {
                    queue.Queue.RemoveLast();
                }
            }

            queue.Queue.AddLast(this);
        }

        public override ExecuteResult Execute()
        {
            if (!Unit)
                return ExecuteResult.Success;
            if (Unit.IsSoloActing)
                return ExecuteResult.Continue;
            Unit.WalkTowards(TargetLocation);
            return ExecuteResult.Success;
        }
    }

    public class BeginAbilityCmd : Command
    {
        public CharacterUnit Unit;
        public ActionAbility Abi;

        public Vector3 TargetLocation;
        public GameUnit TargetUnit;

        public override void Enqueue(CommandQueue queue)
        {
            queue.Queue.Clear();
            queue.Queue.AddLast(this);
        }

        public override ExecuteResult Execute()
        {
            if (Abi.HighPiority)
            {
                if (Unit.IsSoloActing)
                {
                    var actionState = Unit.FSM.GetState(CharacterStateID.ActionState) as SoloActionCharacterState;
                    if (actionState.ActiveStateName == ActionSubID.ActionState_BackswingRecovery)
                    {
                        Unit.BreakAction();
                    }
                }
            }

            if (!Abi.CanBegin)
                return ExecuteResult.Continue;
            if (!Abi.CheckTriggerable())
                return ExecuteResult.Continue;

            switch (Abi.CastType)
            {
                case ActionAbility.ActionCastType.Location:
                    Abi.BeginAtLocation(TargetLocation);
                    break;
                case ActionAbility.ActionCastType.GameUnit:
                    Abi.BeginAtTargetUnit(TargetUnit);
                    break;
                case ActionAbility.ActionCastType.NoTarget:
                    Abi.BeginNoTarget();
                    break;
            }

            return ExecuteResult.Success;
        }
    }
}