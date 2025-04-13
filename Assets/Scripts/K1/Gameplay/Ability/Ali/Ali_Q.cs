using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace K1.Gameplay
{
    public class Ali_Q : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
        }

        protected override void ActionCastBegin()
        {
            base.ActionCastBegin();
        }

        protected override void ActionExit()
        {
            base.ActionExit();
            AbiOwner.SetAnimatorBool("Yasuo_Dashing", false);
        }


        protected override void ActionActBegin()
        {
            var projectilePrefab =
                DataVisualAt();

            var spawnLocation = new Vector3(AbiOwner.transform.position.x, AbiOwner.transform.position.y + 0.5f,
                AbiOwner.transform.position.z);
            spawnLocation += TargetDirectionNoY * 0.5f;

            var func = FuncUnit.Spawn(spawnLocation, TargetDirectionNoY);
            func.IsProjectile = true;
            var seq = DOTween.Sequence();
            var projectile = VfxAPI.CreateVisualEffectAtUnit(projectilePrefab, func, TargetDirectionNoY, Vector3.zero);

            var start = func.transform.DOMove(projectile.transform.position + TargetDirectionNoY * 8, 1.0f);
            start.SetEase(Ease.InOutCubic);
            seq.Append(start);
            HashSet<GameUnit> done = new HashSet<GameUnit>();
            seq.AppendCallback(() =>
            {
                TransformTargetProjectile bullet = func.gameObject.AddComponent<TransformTargetProjectile>();
                bullet.mTarget = AbiOwner.transform;
                done.Clear();
                bullet.OnFinish = (p) => { func.Die(); };
                bullet.Speed = 10;
                bullet.Begin();
            });
            func.onLogic = () =>
            {
                CharacterUnitAPI.ForCharacterInSphere(func.WorldPosition, 1.0f,
                    (selection) =>
                    {
                        CharacterUnitAPI.CharacterDamage(AbiOwner, selection, AbiOwner.RealMagicDamage,
                            DamageType.MagicDamage);
                        done.Add(selection);
                    }, (selection) =>
                    {
                        if (selection.IsAlive && selection.IsEnemy(AbiOwner) && !done.Contains(selection))
                            return true;
                        return false;
                    });
            };
            seq.Play();
        }
    }
}