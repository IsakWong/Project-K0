using UnityEngine;

namespace K1.Gameplay
{
    public class Lux_Lucent_Singularity : ActionAbility
    {
        public VariantRef<float> StunTime = new(2.0f);
        public VariantRef<float> OwnerSpeed = new(10);
        public VariantRef<float> OwnerAcceleration = new(-15);
        public VariantRef<float> OwnerLifeTime = new(0.5f);

        public override void Init()
        {
            base.Init();

            CastType = ActionCastType.Location;
            OnActionCastBegin += () =>
            {
                KGameCore.SystemAt<CameraModule>().FieldView(90, DataCastPointAt() - 0.2f, 0.15f, 0.1f);
            };
            Vector3 direction;
            OnActionActingBegin = () =>
            {
                KGameCore.SystemAt<CameraModule>().ShakeCamera(0.2f, KGameCore.SystemAt<CameraModule>().mHighShake);
                direction = TargetDirection;
                var unit = FuncUnit.Spawn(AbiOwner.GetSocketWorldPosition(BuiltinCharacterSocket.RightHand),
                    Vector3.forward);
                var pos = unit.transform.position;
                unit.transform.position = pos;
                unit.CreateSocketVisual(ProjectileVFX,
                    BuiltinCharacterSocket.Origin.ToString(), Vector3.zero, Vector3.one);
                _funcUnit = unit;
                var movementBuff = CharacterUnitAPI.CreateMovementBuff();
                movementBuff.SetDirection(-direction)
                    .SetAcceleration(OwnerAcceleration)
                    .SetMoveSpeed(OwnerSpeed)
                    .SetLifetime(OwnerLifeTime)
                    .AddTo(AbiOwner, AbiOwner);

                var projectComp = unit.AddUnitComponent<ProjectileDistance>();
                projectComp.FinishOnArrived = true;
                projectComp.FinishOnCollide = false;
                projectComp.MaxDistance = 10;
                projectComp.Direction = direction;
                projectComp.IgnoreY = false;
                projectComp.Speed = 5;
                projectComp.CollideRange = DataBoxAreaAt().y;
                projectComp.CollideLayerMask = GameUnitAPI.GetCharacterLayer() | GameUnitAPI.GetEnvLayerMask();
                projectComp.OnCollideGameObject += (selection) =>
                {
                    if (selection.GetComponent<GameUnit>() == AbiOwner)
                        return;
                    projectComp.ForceFinish();
                };
                projectComp.OnFinish += (u) => { unit.Die(); };
                projectComp.Begin();

                unit.onLogic += () =>
                {
                    OverlapSphereEnemy<CharacterUnit>(unit.WorldPosition, DataBoxAreaAt().x, out var ret);
                    foreach (var selection in ret)
                    {
                        selection.transform.position = selection.transform.position +
                                                       selection.DirectionBetweenUnit(unit) * 1.0f *
                                                       KTime.scaleDeltaTime;
                    }
                };
                unit.OnUnitDie += () =>
                {
                    OverlapSphereEnemy<CharacterUnit>(_funcUnit.WorldPosition, DataBoxAreaAt().z, out var ret);
                    foreach (var selection in ret)
                    {
                        if (!CharacterUnitAPI.GenericEnemyCondition(AbiOwner, selection))
                            return;
                        MovementBuff buff = CharacterUnitAPI.CreateMovementBuff();
                        buff.SetDirection((selection.WorldPosition - _funcUnit.WorldPosition).normalized);
                        buff.SetMoveSpeed(8.0f);
                        buff.SetAcceleration(-3.0f);
                        buff.SetLifetime((_funcUnit.WorldPosition - selection.WorldPosition).magnitude / 8.0f);
                        buff.AddTo(AbiOwner, selection);
                        DamageParam param = new DamageParam()
                        {
                            Source = AbiOwner,
                            DamageType = DamageType.MagicDamage,
                            DamageValue = AbiOwner.RealMagicDamage * DataMultipleAt(),
                            ValueLevel = ValueLevel.LevelMax
                        };
                        if (selection.TryTakeDamage(param))
                        {
                            var stunbuff = GameplayConfig.Instance().CreateStunBuff();
                            stunbuff.SetLifetime(StunTime);
                            ;
                            stunbuff.AddTo(AbiOwner, selection);
                        }
                    }
                };
            };
        }

        public VariantRef<GameObject> ProjectileVFX = new();
        protected FuncUnit _funcUnit;

        protected override void ActionCastBegin()
        {
            base.ActionCastBegin();
        }
    }
}