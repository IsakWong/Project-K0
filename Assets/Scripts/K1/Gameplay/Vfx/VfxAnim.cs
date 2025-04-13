using UnityEngine;

public class VfxAnim : MonoBehaviour
{
    /// <summary>
    /// LoopDelay后进行Loop的调用
    /// </summary>
    public float mLoopDelay = 0.0f;

    public bool mAutoLoop = true;
    public float mDieDuration = 0.3f;

    /// <summary>
    /// 播放创建动画
    /// </summary>
    public virtual void Spawn()
    {
    }

    /// <summary>
    /// 播放死亡
    /// </summary>
    public virtual void Die()
    {
    }
}