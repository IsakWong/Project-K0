using System;
using Framework.Foundation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace K1.Gameplay
{
    public class AbilityIndicator
    {
        public CharacterUnit Owner;
        public AbilityBase Abi;

        public Vector3 point0;
        public Vector3 controlPoint;
        public Vector3 point1;

        public int PointCounts = 15;
        public float DistancePercent = 0.5f;
        public float MaxDistance = 15.0f;

        public void Begin(InputAction.CallbackContext context)
        {
            KGameCore.SystemAt<HUDModule>().Indicator.SetActive(true);
        }

        public void End(K1PlayerController controller, InputAction.CallbackContext context)
        {
            KGameCore.SystemAt<HUDModule>().Indicator.SetActive(false);
            controller.PushCommand(new BeginAbilityCmd()
            {
                Abi = this.Abi as ActionAbility,
                Unit = Owner,
                TargetLocation = point1
            });
        }

        public void CaculateEndPoint()
        {
            point0 = Owner.WorldPosition;
            var lookDirection = Camera.main.transform.forward;
            controlPoint = Owner.WorldPosition + lookDirection * (DistancePercent * MaxDistance);
            lookDirection.y = 0;
            lookDirection.Normalize();

            point1 = Owner.WorldPosition + lookDirection * (DistancePercent * MaxDistance);
            point1 = Utility.DetectGround(point1, GameUnitAPI.GetGroundMask());
            controlPoint = (point1 + point0) * 0.5f;
            controlPoint.y += MaxDistance * 0.5f;
        }

        public void Logic()
        {
            CaculateEndPoint();
            DistancePercent = Math.Clamp((Camera.main.transform.forward.y + 0.2f) / 0.5f, 0, 1);
            LineRenderer lineRenderer = KGameCore.SystemAt<HUDModule>().IndicatorLine;
            Transform end = KGameCore.SystemAt<HUDModule>().IndicatorCircle;
            end.position = point1;

            lineRenderer.transform.position = Owner.WorldPosition;
            lineRenderer.positionCount = PointCounts;
            // Calculate the positions for the curved line
            int j = 0;
            for (int index = 0; index < lineRenderer.positionCount; index++, j++)
            {
                float t = j / (float)(PointCounts - 1);

                lineRenderer.SetPosition(
                    index,
                    MathUtility.Bezier(
                        t,
                        point0,
                        controlPoint,
                        point1
                    ));
            }
        }
    }
}