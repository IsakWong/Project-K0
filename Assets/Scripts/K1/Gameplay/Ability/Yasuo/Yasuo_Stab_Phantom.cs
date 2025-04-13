using System.Collections.Generic;
using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Stab_Phantom : ActionAbility
    {
        public VariantRef<GameObject> BladeVFX = new(null);
        public VariantRef<float> Range = new(3);
        private bool acting = true;

        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
            OnActionActingBreak += () => { acting = false; };
            OnActionActingEnd += () => { acting = false; };
            OnActionActingBegin += () =>
            {
                acting = true;
                AddTimer(0.2f, () =>
                {
                    if (acting)
                    {
                        var originPos = TargetLocation;
                        originPos.y += Random.Range(0, 3);
                        var pos = MathUtility.RandomPointInCircle(originPos, new Vector3(3, 0, 1));
                        VfxAPI.CreateVisualEffect(BladeVFX.As(), pos, (TargetLocation - pos).normalized);
                        OverlapSphereEnemy<CharacterUnit>(pos,
                            Range, out var result);
                        foreach (var selection in result)
                        {
                            DamageParam param = new DamageParam()
                            {
                                DamageType = DamageType.MagicDamage,
                                Source = AbiOwner,
                                DamageValue = DataMultipleAt() * AbiOwner.RealMagicDamage,
                                ValueLevel = ValueLevel.Level2,
                                HitValue = 90.0f
                            };
                            if (selection.TryTakeDamage(param))
                            {
                            }
                        }
                    }
                }, 5);
            };
        }
    }
}