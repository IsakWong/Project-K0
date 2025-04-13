namespace K1.Gameplay
{
    public class Yasuo_Rage : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            OnActionCastEnd += () =>
            {
                VfxAPI.CreateVisualEffect(DataVisualAt(), AbiOwner.transform.position, AbiOwner.transform.forward);
                AddTimer(0.2f, () =>
                {
                    OverlapSphereEnemy<CharacterUnit>(AbiOwner.WorldPosition, DataBoxAreaAt().z, out var result);
                    foreach (var selection in result)
                    {
                        DamageParam param = new DamageParam()
                        {
                            DamageValue = DataMultipleAt() * 10.0f,
                            DamageType = DamageType.PhysicalDamage,
                            Source = AbiOwner,
                            ValueLevel = ValueLevel.Level2,
                        };
                        selection.TryTakeDamage(param);
                        float distance = GameUnitAPI.DistanceBetweenGameUnit(AbiOwner, selection);
                        MovementBuff buff = CharacterUnitAPI.CreateMovementBuff()
                            .SetLifetime(0.1f + 0.2f * (1 - distance / 5.0f)) as MovementBuff;
                        buff.AddTo(AbiOwner, selection);
                        buff.SetDirection(GameUnitAPI.DirectionBetweenUnit(AbiOwner, selection))
                            .SetMoveSpeed(15)
                            .SetAcceleration(-10);
                    }
                }, 6);
            };
        }
    }
}