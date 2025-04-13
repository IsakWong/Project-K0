using System.Collections.Generic;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_SpeedUp : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
        }

        protected override void AbiBegin()
        {
            base.AbiBegin();
        }

        public VariantRef<BuffConfig> SpeedUpBuff = new VariantRef<BuffConfig>();

        // ReSharper disable Unity.PerformanceAnalysis
        public VariantRef<GameObject> BuffVFX = new();

        protected override void ActionActBegin()
        {
            base.ActionActBegin();
            var effect = VfxAPI.CreateVisualEffect(BuffVFX, AbiOwner.WorldPosition, Vector3.forward);
            var buff = SpeedUpBuff.As().CreateBuff() as PropertyModifyBuff;
            buff.IsFixed.SetVariant(new Variant(true));
            buff.Increment.SetVariant(new Variant(3.0f));
            buff.mModifierType = CharacterUnit.CharacterProperty.WalkSpeed;
            buff.SetLifetime(5.0f).AddTo(AbiOwner, AbiOwner);
        }
    }
}