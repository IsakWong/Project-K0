using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AYellowpaper.SerializedCollections;
using K1.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public enum ActAbiDataKey
{
    Key0,
    Key1,
    Key2,
    Key3,
    Key4,
    Key5,
    Key6,
    Key7,
    Key8,
}

#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.Field)]
#endif
public class RenameAttribute : PropertyAttribute
{
    /// <summary> 枚举名称 </summary>
    public string name = "";

    /// <summary> 文本颜色 </summary>
    public string htmlColor = "#000000";

    /// <summary> 重命名属性 </summary>
    /// <param name="name">新名称</param>
    public RenameAttribute(string name)
    {
        this.name = name;
    }

    /// <summary> 重命名属性 </summary>
    /// <param name="name">新名称</param>
    /// <param name="htmlColor">文本颜色 例如："#FFFFFF" 或 "black"</param>
    public RenameAttribute(string name, string htmlColor)
    {
        this.name = name;
        this.htmlColor = htmlColor;
    }
}

[Serializable]
public class AnimatorParamConfig
{
    public string VarName;
    public Variant VarValue;


    public static string kDeflectTrigger = "Deflect";
    public static string kStunTrigger = "Stun";
    public static string kHitTrigger = "Hit";
    public static string kIdleTrigger = "Idle";
    public static string kDieTrigger = "Die";
    public static string kSpawnTrigger = "Spawn";
    public static string kWalkTrigger = "Walk";
    public static string kWalkSpeed = "WalkSpeed";
    public static string kLeftWalkTrigger = "LeftWalk";
    public static string kRightWalkTrigger = "RightWalk";
}

[Serializable]
public class ActionCastConfig
{
    public float CastPoint = 0.5f;
    public float CastingTime = 0.0f;
    public float CastBackswingTime = 0.0f;
    public ValueLevel CastEndureLevel = ValueLevel.LevelNone;
    public ValueLevel ActingEndureLevel = ValueLevel.LevelNone;
}

[Serializable]
public class CharacterAudioPlayConfig
{
    public AudioClip Audio;
    public CharacterAudioChannel Channel;
}

[CreateAssetMenu(fileName = "_AbiConfig", menuName = "主动技能配置")]
public class ActionAbilityConfig : AbilityConfig
{
    #region VFX

    [FormerlySerializedAs("OwnerLocationVFX")]
    public Vfx CastOwnerLocationVFX;

    public Vfx CastTargetLocationVFX;

    [FormerlySerializedAs("TargetLocationVFX")]
    public Vfx ActingTargetLocationVFX;

    public Vfx ActingOwnerLocationVFX;

    #endregion

    #region Animator

    public List<AnimatorParamConfig> AnimatorParam;
    public List<ActionCastConfig> ActionCastConfig;

    #endregion

    #region Audio

    public List<AudioClip> CastAudio = new List<AudioClip>();

    #endregion

    public bool FacingTargetWhenCast = false;

    [Rename("特效")] [SerializedDictionary("Key", "Value")]
    public SerializedDictionary<ActAbiDataKey, Variant> mDataVisualEffect;


    [FormerlySerializedAs("mDataDamageMultiple")] [Rename("伤害系数")] [SerializedDictionary("Key", "Value")]
    public SerializedDictionary<ActAbiDataKey, float> DamageMultiple = new SerializedDictionary<ActAbiDataKey, float>()
    {
        { ActAbiDataKey.Key0, 1.0f }
    };

    public SerializedDictionary<ActAbiDataKey, BuffConfig> mDataBuff;

    [Rename("伤害区域")] [SerializedDictionary("Key", "Value")]
    public SerializedDictionary<ActAbiDataKey, Vector3> mDataBoxArea =
        new SerializedDictionary<ActAbiDataKey, Vector3>()
        {
            { ActAbiDataKey.Key0, new Vector3(1.0f, 1.0f, 1.0f) }
        };

    [Rename("音效")] [SerializedDictionary("Key", "Value")]
    public SerializedDictionary<ActAbiDataKey, List<AudioClip>> Audios;

    public SerializedDictionary<ActAbiDataKey, GameObject> GameObjectsData;

    public float mCooldownTime = 0.5f;
    public float mIndicatorRadius = 0.5f;
    public bool mHasIndicatorDefault = false;


    public List<float> ManaCost;


    public override AbilityBase CreateAbi()
    {
        Type type = (Type)Prototype;
        if (type == null)
        {
            Debug.LogError(Prototype);
        }

        var abi = CreateInstance();
        abi.Config = this;
        return abi;
    }
}