using EasyCharacterMovement;
using UnityEngine;
using UnityEngine.AI;

namespace K1.Gameplay
{
    public class WalkCharacterState : CharacterState
    {
        private Vector3 TargetLocation;
        private Transform CharacterTransform;
        public Vector3 MoveTargetDirection;
        public Vector3 FaceTargetDirection;
        public bool SeparateMove = false;
        public bool DirectMove = false;

        public WalkCharacterState()
        {
            Name = "移动";
        }

        private string _prevAnim = "";

        public override void OnEnter()
        {
            //base.OnEnter();
            Vector3 movementDirection = Character.transform.forward;
            Vector3 faceDirection = Character.transform.forward;
            string walkAnim = AnimatorParamConfig.kWalkTrigger;

            if (DirectMove)
            {
                movementDirection = MoveTargetDirection;
                faceDirection = FaceTargetDirection;
            }
            else
            {
                if (path.corners.Length > 1)
                {
                    TargetLocation = path.corners[1];
                }
                else
                {
                    TargetLocation = Character.WalkTargetPosition;
                }

                movementDirection = TargetLocation - Character.transform.position;
                faceDirection = movementDirection;
            }

            faceDirection.Normalize();
            ;
            movementDirection.Normalize();

            Quaternion delta = Quaternion.FromToRotation(faceDirection, movementDirection);
            var angle = delta.eulerAngles.y;
            if (angle > 1 && angle <= 180)
            {
                walkAnim = AnimatorParamConfig.kLeftWalkTrigger;
            }
            else if (angle >= 180 && angle < 360)
            {
                walkAnim = AnimatorParamConfig.kRightWalkTrigger;
            }

            _prevAnim = walkAnim;
            CharacterTransform = Character.transform;
            Character.SetAnimatorFloat(AnimatorParamConfig.kWalkSpeed, Character.WalkSpeed);
            Character.SetAnimatorTrigger(walkAnim);
        }

        public void RotateTowards(Vector3 worldDirection, float maxDegreesDelta, bool updateYawOnly = true)
        {
            Vector3 characterUp = CharacterTransform.transform.up;

            if (updateYawOnly)
                worldDirection = worldDirection.projectedOnPlane(characterUp);

            if (worldDirection == Vector3.zero)
                return;
            Quaternion targetRotation = Quaternion.LookRotation(worldDirection, characterUp);
            Character.transform.localRotation =
                Quaternion.RotateTowards(Character.transform.localRotation, targetRotation, maxDegreesDelta);
        }

        private NavMeshPath path = new NavMeshPath();
        public int NaviIndex = 0;

        public override void OnLogic()
        {
            base.OnLogic();
            Vector3 movementDirection = Character.transform.forward;
            Vector3 faceDirection = Character.transform.forward;
            Vector3 velocity;
            bool isArrived = false;
            if (DirectMove)
            {
                movementDirection = MoveTargetDirection;
                faceDirection = FaceTargetDirection;
            }
            else
            {
                if (path.corners.Length > 1)
                {
                    TargetLocation = path.corners[1];
                }
                else
                {
                    TargetLocation = Character.WalkTargetPosition;
                }

                movementDirection = TargetLocation - Character.transform.position;
                faceDirection = movementDirection;
            }

            faceDirection.Normalize();
            ;
            movementDirection.Normalize();

            velocity = movementDirection * Character.WalkSpeed;

            RotateTowards(faceDirection, 540 * Time.deltaTime, true);
            Character.mMoveVelocity = velocity;

            string walkAnim = _prevAnim;
            var deltaDir = Quaternion.FromToRotation(faceDirection, movementDirection);
            var angle = deltaDir.eulerAngles.y;
            if (angle > 1 && angle <= 180)
            {
                Character.mMoveVelocity *= 0.4f;
                walkAnim = AnimatorParamConfig.kLeftWalkTrigger;
            }
            else if (angle >= 180 && angle < 360)
            {
                Character.mMoveVelocity *= 0.4f;
                walkAnim = AnimatorParamConfig.kRightWalkTrigger;
            }
            else
            {
                walkAnim = AnimatorParamConfig.kWalkTrigger;
            }

            if (walkAnim != _prevAnim)
            {
                Character.SetAnimatorTrigger(walkAnim);
                _prevAnim = walkAnim;
            }

            var delta = Character.transform.position - Character.WalkTargetPosition;
            delta.y = 0;
            isArrived = delta.magnitude < (velocity.magnitude * KTime.scaleDeltaTime + 0.05f);
            if (isArrived)
            {
                Character.Idle();
            }
            else
            {
                Character.SetAnimatorFloat(AnimatorParamConfig.kWalkSpeed, Character.WalkSpeed);
            }
        }

        public override void OnExit()
        {
            //base.OnExit();
            Character.SetAnimatorFloat(AnimatorParamConfig.kWalkSpeed, 0.0f);
            //Character.SetAnimatorFloat(AnimatorParamConfig.kLeftWalkTrigger, 0.0f);
            //Character.SetAnimatorFloat(AnimatorParamConfig.kRightWalkTrigger, 0.0f);
        }
    }
}