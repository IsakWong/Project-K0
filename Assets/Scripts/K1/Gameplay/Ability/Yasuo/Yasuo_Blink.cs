using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Blink : ActionAbility
    {
        public bool ImmediateCast = false;
        public VariantRef<GameObject> OwnerVfx = new();

        public override void Init()
        {
            base.Init();
            HighPiority = true;
            AutoVFX = true;
            CastType = ActionCastType.Location;
            OnAbiBegin += (abi) =>
            {
                if (ImmediateCast)
                {
                    ActionIdx = 1;
                    AbiOwner.CreateSocketVisual(OwnerVfx.As(), BuiltinCharacterSocket.Origin.ToString(),
                        Vector3.up * 2.0f,
                        Vector3.one, 0.5f);
                    ImmediateCast = false;
                }
                else
                {
                    ActionIdx = 0;
                    AbiOwner.CreateSocketVisual(OwnerVfx.As(), BuiltinCharacterSocket.Origin.ToString(),
                        Vector3.up * 2.0f,
                        Vector3.one, 2.0f);
                }
            };
            OnActionActingBegin += () => { AbiOwner.transform.position = TargetLocation; };
        }
    }
}