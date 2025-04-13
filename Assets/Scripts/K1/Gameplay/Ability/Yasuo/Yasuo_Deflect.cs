using UnityEngine;

namespace K1.Gameplay
{
    public class YasuoDeflect : ActionAbility
    {
        public override void Init()
        {
            CastType = ActionCastType.Location;
            OnActionActingBegin += () =>
            {
                KGameCore.SystemAt<CameraModule>().ShakeCamera(0.2f);
                KGameCore.Instance.Timers.AddTimer(Random.Range(0.05f, 0.15f), () =>
                {
                    var obj = VfxAPI.CreateVisualEffect(DataVisualAt(),
                        AbiOwner.GetSocketWorldPosition(BuiltinCharacterSocket.Weapon), AbiOwner.transform.forward);
                }).Start();
            };
            base.Init();
        }
    }
}