using System;
using K1.Gameplay;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
///
/// 距离投射物
/// 
/// </summary>
public class ProjectileDistance : ProjectileBase
{
    public float Height = 0;
    public float MaxDistance = 10.0f;
    public bool DetectGround = false;
    public bool FinishOnArrived = false;
    public Vector3 TargetPosition = Vector3.zero;
    public Action<ProjectileDistance> OnMaxDistance;
    // Start is called before the first frame update

    public override void Begin()
    {
        if (Direction == Vector3.zero)
            Direction = TargetPosition - transform.position;
        base.Begin();
        maxDistance = false;
    }

    bool maxDistance = false;

    bool CheckDistance()
    {
        distance += Speed * KTime.scaleDeltaTime;
        if (!isFinish && MaxDistance > 0)
        {
            isFinish = distance >= MaxDistance;
            maxDistance = isFinish;
        }

        return isFinish;
    }

    public void ForceFinish()
    {
        isFinish = true;
    }

    protected override bool CheckArrived()
    {
        var delta = TargetPosition - transform.position;
        if (IgnoreY)
            delta.y = 0;

        if (FinishOnArrived)
        {
            if (!isFinish)
                isFinish = delta.magnitude < Speed * KTime.scaleDeltaTime * 1.1f;
        }

        return isFinish;
    }

    protected override void CheckFinish()
    {
        base.CheckFinish();
        if (isFinish && isPlaying)
        {
            if (maxDistance)
            {
                OnMaxDistance?.Invoke(this);
            }
            else
            {
                OnArrive?.Invoke(this);
            }
        }
    }

    public override void Logic()
    {
        if (!isPlaying)
            return;


        Overlap();

        if (CollidedUnits.Count > 0 && FinishOnCollide)
            isFinish = true;

        CheckDistance();

        CheckLifetimeOver();

        CheckArrived();

        CheckFinish();

        if (!isFinish)
        {
            var position = transform.position;
            var currenLocation = position;
            var newLocation = position + Direction * (KTime.scaleDeltaTime * Speed);
            if (IgnoreY)
                newLocation.y = transform.position.y;
            if (DetectGround)
            {
                Ray ray = new Ray(newLocation + Vector3.up * 0.5f, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 10, GameUnitAPI.GetGroundMask()))
                {
                    newLocation = hit.point;
                }
            }

            transform.position = newLocation;
        }

        CaculateSpeed();
    }
}