using System.Collections.Generic;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Healing : ActionAbility
    {
        public VariantRef<GameObject> HealingBuffVFX = new();

        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
            OnActionActingBegin += () =>
            {
                var visualEffect = DataVisualAt();
                var effect = VfxAPI.CreateVisualEffect(HealingBuffVFX, AbiOwner.WorldPosition, Vector3.forward);
                AbiOwner.TakeHealing(10.0f);
            };
        }
    }
}