using UnityEngine;

namespace K1.Gameplay
{
    public class Ali_R : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
        }

        protected override void AbiActing()
        {
            base.AbiActing();
        }

        protected override void ActionActBegin()
        {
            VfxAPI.CreateVisualEffect(DataVisualAt(), AbiOwner.GetSocketWorldPosition(BuiltinCharacterSocket.Body),
                Vector3.forward);

            var incBuff = DataBuffAt(ActAbiDataKey.Key0).CreateBuff() as PropertyModifyBuff;
            incBuff.SetLifetime(1.0f);
            incBuff.mModifierType = CharacterUnit.CharacterProperty.MagicDamage;
            incBuff.AddTo(AbiOwner, AbiOwner);

            var buff = CharacterUnitAPI.CreateMovementBuff();
            buff.SetLifetime(0.3f);
            buff.SetDirection(TargetDirectionNoY)
                .SetMoveSpeed(15)
                .AddTo(AbiOwner, AbiOwner);
        }
    }
}