using System.Collections.Generic;
using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Lightning : ActionAbility
    {
        public VariantRef<float> Delay = new VariantRef<float>(0.5f);
        public VariantRef<float> StunTime = new VariantRef<float>(0.8f);
        public VariantRef<int> Loops = new(10);
        public VariantRef<GameObject> Field_VFX = new();


        private List<Vector3> points = new();
        private KTimer timer;

        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.NoTarget;
            OnActionActingBegin += () =>
            {
                var mode = KGameCore.Instance.CurrentGameMode as GameMode1;
                if (mode)
                    mode.ThunderEffect.PlayThunder();
                var vfxPrefab = Field_VFX;
                timer = AddTimer(1.5f, () =>
                {
                    CharacterUnit target = null;
                    OverlapSphereEnemy<CharacterUnit>(AbiOwner.WorldPosition, 20, out var ret);
                    foreach (var selection in ret)
                    {
                        target = selection;
                        break;
                    }

                    var central = target != null ? target.WorldPosition : AbiOwner.WorldPosition;
                    var length = target != null ? new Vector3(0, 0, 0) : new Vector3(10, 0, 10);
                    var point = MathUtility.RandomPointInCircle(central, length);
                    //WarningCircle(point, DataBoxAreaAt(), 0.5f);
                    VfxAPI.CreateVisualEffect(vfxPrefab, point, Vector3.forward);
                    AddTimer(Delay, () =>
                    {
                        OverlapSphereEnemy<CharacterUnit>(point, DataBoxAreaAt().z, out var result);
                        foreach (var selection in result)
                        {
                            DamageParam param = new DamageParam()
                            {
                                DamageType = DamageType.PhysicalDamage,
                                DamageValue = DataMultipleAt() * AbiOwner.RealMagicDamage,
                                Source = AbiOwner,
                                ValueLevel = ValueLevel.Level1
                            };
                            float maxRange = DataBoxAreaAt().z;
                            float distance = GameUnitAPI.DistanceBetweenPosition(selection.WorldPosition, point);
                            bool hitted = false;

                            if (distance < maxRange * 0.5f)
                            {
                                if (selection.TryTakeDamage(param))
                                {
                                    var stun = GameplayConfig.Instance().CreateStunBuff();
                                    stun.StunLevel = ValueLevel.Level2;
                                    stun.SetLifetime(StunTime);
                                    stun.AddTo(AbiOwner, selection);
                                }
                            }
                            else if (distance < maxRange)
                            {
                                param.DamageValue = param.DamageValue * 0.5f;
                                if (selection.TryTakeDamage(param))
                                {
                                }
                            }
                        }
                    });
                }, Loops);
            };
        }
    }
}