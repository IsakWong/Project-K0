using UnityEngine;

namespace K1.Gameplay
{
    public class Lux_Light_Binding : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
        }

        protected override void ActionActBegin()
        {
            var postion = AbiOwner.GetSocketWorldPosition(BuiltinCharacterSocket.RightHand);
            var funcUnit = FuncUnit.Spawn(postion, TargetDirectionNoY);
            funcUnit.transform.rotation = Quaternion.LookRotation(TargetDirectionNoY);
            VfxAPI.CreateVisualEffectAtUnit(DataVisualAt(), funcUnit, TargetDirectionNoY, Vector3.zero);
            var projectileComp = funcUnit.AddUnitComponent<ProjectileDistance>();

            funcUnit.IsProjectile = true;

            projectileComp.Direction = TargetDirectionNoY;
            projectileComp.FinishOnArrived = true;
            projectileComp.FinishOnCollide = false;
            projectileComp.MaxDistance = 15;
            projectileComp.Speed = 10;
            projectileComp.CollideLayerMask = GameUnitAPI.GetCharacterLayerMask();
            projectileComp.OnCollideUnit += delegate(GameUnit unit)
            {
                CharacterUnit selection = unit as CharacterUnit;
                if (CharacterUnitAPI.GenericEnemyCondition(AbiOwner, selection))
                {
                    DamageParam param = new DamageParam()
                    {
                        DamageType = DamageType.MagicDamage,
                        Source = AbiOwner,
                        DamageValue = DataMultipleAt() * AbiOwner.RealMagicDamage,
                    };
                    if (selection.TryTakeDamage(param))
                    {
                        var slowBuff = CharacterUnitAPI.CreatePropertyModifyBuff();
                        slowBuff.SetLifetime(3);
                        slowBuff.mModifierType = CharacterUnit.CharacterProperty.WalkSpeed;
                        slowBuff.AddTo(AbiOwner, selection);
                    }
                }
            };
            projectileComp.Begin();
        }
    }
}