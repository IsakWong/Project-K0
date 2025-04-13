using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class Lux_Light_Strike : ActionAbility
    {
        public VariantRef<GameObject> ExplodeVfx = new VariantRef<GameObject>();

        public override void Init()
        {
            base.Init();
            OnActionActingBegin += () =>
            {
                CharacterUnit target = GetLockEnemy(8.0f);
                if (target)
                    targetLocation = target.WorldPosition;
                else
                    targetLocation = AbiOwner.WorldPosition + AbiOwner.transform.forward * 5.0f;
                Trigger();
            };
        }

        private Vector3 targetLocation;

        public VariantRef<float> StunTime = new VariantRef<float>(2.0f);

        public void Trigger()
        {
            var position = targetLocation;
            position.y += Random.Range(4.5f, 6.5f);
            var funcUnit = FuncUnit.Spawn(position, Vector3.forward);
            var vfx = VfxAPI.CreateVisualEffectAtUnit(DataVisualAt(), funcUnit, Vector3.zero, Vector3.zero);
            funcUnit.Lifetime = vfx.mLifeTime;
            KGameCore.SystemAt<CameraModule>().FieldView(90, DataCastPointAt(), DataActingTimeAt(), 0.2f);
            funcUnit.onLogic += () => { funcUnit.transform.LookAt(targetLocation); };
            AddTimer(0.9f, () =>
            {
                VfxAPI.CreateVisualEffect(ExplodeVfx.As(), targetLocation, Vector3.forward);
                KGameCore.SystemAt<CameraModule>().ShakeCamera(0.5f, KGameCore.SystemAt<CameraModule>().mHighShake);
                OverlapSphereEnemy<CharacterUnit>(targetLocation, DataBoxAreaAt().z, out var ret);
                foreach (var selection in ret)
                {
                    DamageParam param = new DamageParam()
                    {
                        DamageType = DamageType.MagicDamage,
                        DamageValue = DataMultipleAt(ActAbiDataKey.Key0) * AbiOwner.RealMagicDamage,
                        Source = AbiOwner,
                        ValueLevel = ValueLevel.Level2,
                    };
                    var stun = GameplayConfig.Instance().CreateStunBuff();
                    stun.StunLevel = ValueLevel.LevelMax;
                    stun.SetLifetime(StunTime);
                    stun.AddTo(AbiOwner, selection);
                    selection.TryTakeDamage(param);
                }
            }).Start();
        }
    }
}