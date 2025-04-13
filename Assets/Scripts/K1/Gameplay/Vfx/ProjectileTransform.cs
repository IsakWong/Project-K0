using System;
using Framework.Foundation;
using UnityEngine;

/// <summary>
///
/// Transform投射物
///
/// 会跟随目标Transform的投射物
/// 
/// </summary>
public class TransformTargetProjectile : ProjectileBase
{
    public Transform mTarget;

    public Vector3 ControlPoint;
    private float _time;

    public float Angle = -90;
    public float ControlDistance = 0.5f;

    public override void Begin()
    {
        base.Begin();
        Direction = mTarget.position - startLocation;
        Direction.Normalize();
        var d = (mTarget.position - startLocation).magnitude * ControlDistance;
        ControlPoint = startLocation + MathUtility.RotateDirectionY(Direction, Angle) * d;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(ControlPoint, 0.5f);
    }

    protected override bool CheckArrived()
    {
        var delta = mTarget.position - transform.position;

        if (IgnoreY)
            delta.y = 0;

        if (!isFinish)
            isFinish = delta.sqrMagnitude < 0.1f;

        return isFinish;
    }

    // Update is called once per frame
    public override void Logic()
    {
        if (!isPlaying)
            return;

        Direction = mTarget.position - transform.position;
        Direction.Normalize();

        Overlap();

        CheckArrived();

        CheckLifetimeOver();


        if (isFinish)
        {
            isPlaying = false;
            OnArrive?.Invoke(this);
            OnFinish?.Invoke(this);
            return;
        }

        Vector3 newLocation;
        if (Mode == MoveMode.Linear)
        {
            newLocation = transform.position + Direction * KTime.scaleDeltaTime * Speed;
        }
        else
        {
            newLocation = MathUtility.Bezier(_time, startLocation, ControlPoint, mTarget.transform.position);
        }

        _time += KTime.scaleDeltaTime;
        if (IgnoreY)
            newLocation.y = transform.position.y;
        transform.position = newLocation;
        transform.rotation = Quaternion.LookRotation(Direction);
    }
}