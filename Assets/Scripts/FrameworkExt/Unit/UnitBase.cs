using System;
using System.Collections.Generic;
using cakeslice;
using K1.Gameplay;
using UnityEngine;

public interface IGameplayLifeCycle
{
    void Spawn();
    void Die();
}

[DisallowMultipleComponent]
public class UnitBase : MonoBehaviour, IGameplayLifeCycle
{
    public List<GameObject> mOutlineGameobjects;


    public bool EnableOnLogic = false;
    public bool SpawnOnStart = true;

    [NonSerialized] public Action onLogic;
    [NonSerialized] public Action OnUnitDie;
    [NonSerialized] public Action OnUnitSpawn;


    #region Interface

    public Vector3 WorldForward
    {
        get => transform.forward;
    }

    public Vector3 WorldPosition
    {
        get => transform.position;
        set => transform.position = value;
    }

    protected GameplayModule gameplay;

    public GameplayModule gameplaySys
    {
        get
        {
            if (gameplay) return gameplay;
            gameplay = KGameCore.Instance.RequireModule<GameplayModule>();
            return gameplay;
        }
    }

    public T AddUnitComponent<T>() where T : UnitComponent
    {
        T t = gameObject.AddComponent<T>();
        onLogic += t.Logic;
        return t;
    }

    public float UnityDestroyDelay = 3.0f;

    private bool _spawned = false;

    public virtual void Spawn()
    {
        if (_spawned)
            return;
        SetUnitActive(true);
        OnSpawn();
        OnUnitSpawn?.Invoke();
        _spawned = true;
        
        KGameCore.SystemAt<UnitModule>().RegisterGameUnit(this, true);
    }

    protected List<UnitBase> subUnits = new();

    public virtual void Die()
    {
        Debug.Assert(_isAlived);
        SetUnitActive(false);
        KGameCore.SystemAt<UnitModule>().RegisterGameUnit(this, false);
        
        TimerManager.StopAllTimer();
        

        var temp = new GameUnit[subUnits.Count];
        subUnits.CopyTo(temp);
        foreach (var unit in temp)
        {
            unit.Die();
        }

        subUnits.Clear();
        if (UnityDestroyDelay > 0)
        {
            Destroy(gameObject, UnityDestroyDelay);
        }

        if (UnityDestroyDelay == 0)
            Destroy(gameObject);
        OnUnitDie?.Invoke();
        _isAlived = false;
    }

    #region Socket挂接点相关接口

    public virtual Vector3 GetSocketWorldPosition(string name)
    {
        return WorldPosition;
    }

    public virtual Transform GetSocketTransform(string name)
    {
        return transform;
    }
    
    public virtual Vfx CreateSocketVisual(GameObject visualPrefab,
        string socket, Vector3 offset, Vector3 scale,
        float lifeTime = -1f)
    {
        var transform = GetSocketTransform(socket);
        var result = Instantiate(visualPrefab, transform.transform.position + offset, transform.rotation);
        var visual = result.GetComponent<Vfx>();
        if (visual == null)
        {
            Debug.LogWarning("特效没有VisualEffectBase组件！");
            visual = result.AddComponent<Vfx>();
        }

        if (lifeTime != 0)
            visual.mLifeTime = lifeTime;
        result.transform.SetParent(transform, true);

        subUnits.Add(visual);
        visual.OnDie += (v) => { subUnits.Remove(v); };
        return visual;
    }

    public virtual void RemoveSocketVisual(Vfx visual)
    {
        visual.Die();
    }
    #endregion
    
    private List<Outline> _outlines = new List<Outline>();

    public bool IsOutlined()
    {
        return _outlines.Count > 0;
    }

    public void EnableOutline(bool value)
    {
        if (value)
        {
            if (_outlines.Count == 0)
            {
                foreach (var obj in mOutlineGameobjects)
                {
                    _outlines.Add(obj.AddComponent<Outline>());
                }
            }
        }
        else
        {
            if (_outlines.Count != 0)
            {
                foreach (var obj in _outlines)
                {
                    Destroy(obj);
                }

                _outlines.Clear();
            }
        }
    }

    #endregion

    public KTimerManager TimerManager = new();


    // Start is called before the first frame update
    public virtual void OnLogic()
    {
        TimerManager.OnLogic();
        onLogic?.Invoke();
    }

    public KTimer AddTimer(float duration, Action onTimerComplete = null, int loops = 1)
    {
        return TimerManager.AddTimer(duration, onTimerComplete, loops);
    }

    private bool _isAlived = true;

    public bool IsAlive
    {
        get { return _isAlived; }
    }

    public virtual void OnSpawn()
    {
    }

    protected void OnDisable()
    {
        SetUnitActive(false);
    }

    protected void OnEnable()
    {
        SetUnitActive(true);
    }

    protected void Awake()
    {
        if (SpawnOnStart)
            Spawn();
    }

    private bool _active = false;

    public void SetUnitActive(bool active)
    {
        if (_active == active)
            return;

        if (active)
        {
            if (!_active)
                OnUnitActive();
            _active = true;
        }
        else
        {
            if (_active)
                OnUnitInactive();
            _active = false;
        }
    }

    public virtual void OnUnitInactive()
    {
    }


    public virtual void OnUnitActive()
    {
    }
}