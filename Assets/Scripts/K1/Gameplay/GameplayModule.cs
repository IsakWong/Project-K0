using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using K1.Gameplay;
using UnityEngine.InputSystem;


public class GameplayModule : KModule
{
    public CharacterUnit.BuiltinCharacterEvent AnyEvent;

    public bool AutoBeginGame = false;
    public bool IsBegan = false;

    public List<CharacterUnit> Characters = new();
    public List<ControllerBase> ControllerList = new();


    public List<string> LogList = new List<string>();
    public Action<string> OnNewLog;

    public void Log(string log)
    {
        LogList.Add(log);
        global::KLog.Log($@"[Frame:{LogicFrame}]" + LogList.Last());
        OnNewLog?.Invoke(log);
    }

    public void RegisterController(ControllerBase value, bool enable)
    {
        if (enable)
            ControllerList.Add(value);
        else
            ControllerList.Remove(value);
    }

    void Start()
    {
        KGameCore.Instance.RequireModule<HUDModule>();
        if (AutoBeginGame)
            BeginGame();
    }


    public bool IsInLogic = false;
    public UInt64 LogicFrame = 0;
    private static GameplayModule _cacheModule;

    public static void CheckInLogic()
    {
        if (!_cacheModule)
            _cacheModule = KGameCore.SystemAt<GameplayModule>();
        if (!_cacheModule.IsInLogic)
            Debug.LogError("不在逻辑线程中更新");
    }

    private float _timescale = 1.0f;

    public class TimeScaleTask
    {
        public float oldValue;
        public float newValue;
        public float duration;
    }

    private Stack<TimeScaleTask> timeScaleTasks = new Stack<TimeScaleTask>();

    public TimeScaleTask PushTimeScale(float value, float duration)
    {
        timeScaleTasks.Push(new TimeScaleTask
        {
            oldValue = _timescale,
            newValue = value,
            duration = duration
        });
        return timeScaleTasks.Peek();
    }

    public float TimeScale
    {
        get => _timescale;
        set
        {
            if (Math.Abs(_timescale - value) < 0.001)
                return;
            _timescale = value;
            foreach (var character in Characters)
            {
                character.mAnimator.speed = _timescale;
            }

            foreach (var vfx in Vfx.AllVfx)
            {
                if (vfx.ParticleParents != null)
                {
                    foreach (var particleParent in vfx.ParticleParents)
                    {
                        if (particleParent == null)
                            Debug.LogError(vfx.gameObject.name);
                        var system = particleParent.GetComponentInChildren<ParticleSystem>();
                        if (system)
                        {
                            var main = system.main;
                            main.simulationSpeed = _timescale;
                        }
                    }
                }
            }
        }
    }

    private void _DoTimeScale()
    {
        foreach (var task in timeScaleTasks)
        {
            task.duration -= Time.fixedDeltaTime;
        }

        if (timeScaleTasks.Count > 0)
        {
            var scaleTask = timeScaleTasks.Peek();
            if (scaleTask.duration <= 0)
            {
                timeScaleTasks.Pop();
            }
            else
            {
                TimeScale = scaleTask.newValue;
            }
        }
        else
        {
            TimeScale = 1.0f;
        }

        KTime.scaleDeltaTime = _timescale * Time.fixedDeltaTime;
    }
    
    private void OnFrameBegin()
    {
        IsInLogic = true;
        _DoTimeScale();
        KGameCore.SystemAt<UnitModule>().ManualPreLogic();
    }

    private void OnFrame()
    {
        foreach (var controller in ControllerList)
        {
            controller.OnLogic();
        }
        KGameCore.SystemAt<UnitModule>().ManualLogic();
    }

    private void OnFrameEnd()
    {
        IsInLogic = false;
        LogicFrame++;
    }

    protected void OnLogic()
    {
        OnFrameBegin();
        OnFrame();
        OnFrameEnd();
    }

    protected void FixedUpdate()
    {
        KTime.scaleDeltaTime = Time.fixedDeltaTime;
        if (!IsBegan)
            return;

        OnLogic();
    }

    protected void Update()
    {
        if (!IsBegan)
            return;
        if (Application.isEditor)
        {
            bool mouseOverWindow = Input.mousePosition.x > 0 && Input.mousePosition.x < Screen.width &&
                                   Input.mousePosition.y > 0 && Input.mousePosition.y < Screen.height;

            //check if the cursor is locked but not centred in the game viewport - i.e. like it is every time the game starts
            if (Cursor.lockState == CursorLockMode.Locked && !mouseOverWindow)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            //if the player presses Escape, unlock the cursor
            if (Input.GetKeyDown(KeyCode.Escape) && Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            //if the player releases any mouse button while the cursor is over the game viewport, then lock the cursor again
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (mouseOverWindow &&
                    (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }

    public void BeginGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        IsBegan = true;
    }
}