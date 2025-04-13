using System;
using System.Collections.Generic;
using K1.Gameplay;
using UnityEngine;
using UnityEngine.Serialization;


public enum MoveMode
{
    Linear,
    Bezier,
}

public class ProjectileBase : UnitComponent
{
    public MoveMode Mode = MoveMode.Linear;

    public float Speed = 10;
    public float Acceleration = 0;
    public float MaxSpeed = 0;
    public float MinSpeed = 0;

    public float MaxLifetime = -1.0f;

    public Vector3 Direction = Vector3.zero;

    public bool IgnoreY = true;

    public GameObject[] ExcludeObjects;

    public bool DestroyOnFinish = true;
    public bool FinishOnCollide = false;

    #region 碰撞相关

    public Action<ProjectileBase> OnArrive;
    public Action<ProjectileBase> OnForceDie;
    public Action<ProjectileBase> OnFinish;
    public Action<ProjectileBase> OnLifetimeOver;

    #endregion

    #region 碰撞相关

    public bool mCollideEnable = true;

    [FormerlySerializedAs("mCollideRange")]
    public float CollideRange = 1;

    [FormerlySerializedAs("mCollideLayerMask")]
    public LayerMask CollideLayerMask;

    public Func<GameUnit, bool> mCollideCondition;

    #endregion

    public Action<GameObject> OnCollideGameObject;

    public Action<GameUnit> OnCollideUnit;

    // Start is called before the first frame update
    public HashSet<GameUnit> CollideUnits
    {
        get { return CollidedUnits; }
    }

    protected HashSet<GameUnit> CollidedUnits;

    protected float distance = 0;
    protected float liftime = 0;
    protected bool isPlaying = false;
    protected bool isFinish = false;
    protected Vector3 startLocation;

    public bool AutoBegin = false;

    private void Start()
    {
        if (AutoBegin)
            Begin();
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }


    public override void Begin()
    {
        CollidedUnits = new HashSet<GameUnit>();
        startLocation = transform.position;
        Direction.Normalize();
        lifeover = false;
        isPlaying = true;
    }

    protected virtual void CaculateSpeed()
    {
        Speed += Acceleration * KTime.scaleDeltaTime;
        if (MaxSpeed > 0 && Speed > MaxSpeed)
            Speed = MaxSpeed;
        if (MinSpeed > 0 && Speed < MinSpeed)
            Speed = MinSpeed;
    }

    private bool lifeover;

    protected virtual void CheckLifetimeOver()
    {
        if (!isFinish)
        {
            if (MaxLifetime > 0)
            {
                isFinish = liftime >= MaxLifetime;
                lifeover = isFinish;
            }
        }
    }

    protected virtual bool CheckArrived()
    {
        return isFinish;
    }

    protected virtual void CheckFinish()
    {
        if (isFinish && isPlaying)
        {
            isPlaying = false;
            if (lifeover)
            {
                OnLifetimeOver?.Invoke(this);
            }

            if (DestroyOnFinish)
            {
                Die();
            }

            OnFinish?.Invoke(this);
            Destroy(this);
        }
    }

    protected virtual void Arrived()
    {
    }

    public override void Die()
    {
        if (isPlaying)
        {
            OnForceDie?.Invoke(this);
            OnFinish?.Invoke(this);
        }

        Destroy(this);
    }

    protected void Overlap()
    {
        if (isPlaying)
        {
            liftime += KTime.scaleDeltaTime;
            var colliders = Physics.OverlapSphere(transform.position, CollideRange, CollideLayerMask);
            foreach (var collider in colliders)
            {
                OnCollideGameObject?.Invoke(collider.gameObject);
                var unit = collider.GetComponent<GameUnit>();
                if (unit)
                {
                    if (CollidedUnits.Contains(unit))
                        return;
                    if (mCollideCondition != null)
                    {
                        if (!mCollideCondition.Invoke(unit))
                            return;
                    }

                    CollidedUnits.Add(unit);
                    OnCollideUnit?.Invoke(unit);
                }
            }
        }
    }
}