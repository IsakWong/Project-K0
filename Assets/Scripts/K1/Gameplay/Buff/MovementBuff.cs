using UnityEngine;

namespace K1.Gameplay
{
    //位移Buff
    public class MovementBuff : Buffbase
    {
        [SerializeField] public Vector3 Direction;
        [SerializeField] public float Speed;
        [SerializeField] public float MinSpeed = 0;
        [SerializeField] public float MaxSpeed = 0;
        [SerializeField] public float Acceleration = 0.0f;

        public ValueLevel MoveLevel = ValueLevel.Level1;

        public override void BuffAdd()
        {
            base.BuffAdd();
            BuffOwner.EnableCollision(false);
        }

        public override void BuffEnd()
        {
            base.BuffEnd();
            BuffOwner.EnableCollision(true);
        }

        public override bool CheckBuffAddable(CharacterUnit source, CharacterUnit target)
        {
            if (source == target)
                return true;
            if (target.GetBuff<EndureBuff>() != null)
            {
                if (target.GetBuff<EndureBuff>().EndureLevel >= MoveLevel)
                    return false;
            }

            return true;
        }

        public MovementBuff SetDirection(Vector3 value)
        {
            Direction = value;
            return this;
        }

        public MovementBuff SetMoveSpeed(float value)
        {
            Speed = value;
            return this;
        }

        public MovementBuff SetAcceleration(float value)
        {
            Acceleration = value;
            return this;
        }

        public override void OnLogic()
        {
            BuffOwner.EnableCollision(false);
            BuffOwner.ForceVelocity = Direction * Speed;
            Speed += Acceleration * KTime.scaleDeltaTime;
            if (MaxSpeed > 0 && Speed > MaxSpeed)
                Speed = MaxSpeed;
            if (MinSpeed > 0 && Speed < MinSpeed)
                Speed = MinSpeed;
        }
    }
}