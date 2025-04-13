using DG.Tweening;
using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Double_Stab : ActionAbility
    {
        public VariantRef<GameObject> DashVfx = new();

        public void Stab(Vector3 position, Vector3 direction, float distance)
        {
            var fakeYasuo = FuncUnit.Spawn(AbiOwner.transform.position, direction);
            var vfx = fakeYasuo.CreateSocketVisual(DashVfx, "", Vector3.zero, Vector3.one);
            vfx.transform.localRotation = Quaternion.identity;

            var YasuoModel = ActionConfig.GameObjectsData[ActAbiDataKey.Key0];
            YasuoModel = GameObject.Instantiate(YasuoModel);
            fakeYasuo.Lifetime = 2.0f;
            var fakeYasuoAnimator = YasuoModel.transform.GetChild(0).GetComponent<Animator>();
            YasuoModel.transform.SetParent(fakeYasuo.transform);
            YasuoModel.transform.localPosition = Vector3.zero;
            YasuoModel.transform.localRotation = Quaternion.identity;

            ApplyAnimatorParam(0, fakeYasuoAnimator);
            var projectile = fakeYasuo.AddUnitComponent<ProjectileDistance>();
            projectile.Direction = direction;
            projectile.Speed = MathUtility.CalculateSpeed(5.0f, 0.5f);
            projectile.Acceleration = MathUtility.CaclulateAcc(5.0f, 0.5f);
            projectile.MaxDistance = distance;
            projectile.MaxLifetime = 0.5f;
            projectile.Begin();

            fakeYasuo.transform.rotation = Quaternion.LookRotation(direction);
            var yasuoStab = AbiOwner.GetAbility<Yasuo_Stab>();
            projectile.OnFinish += (proj) =>
            {
                OverlapSphereEnemy<CharacterUnit>(fakeYasuo.WorldPosition, 10, out var ret, false);
                Vector3 stabDirection = TargetDirectionNoY;
                foreach (var selection in ret)
                {
                    stabDirection = (selection.WorldPosition - fakeYasuo.WorldPosition).normalized;
                    break;
                }

                fakeYasuoAnimator.SetTrigger("Yasuo_Stab");
                fakeYasuo.transform.rotation = Quaternion.LookRotation(stabDirection);
                AddTimer(yasuoStab.DataCastPointAt(), () =>
                {
                    yasuoStab.Trigger(fakeYasuo.transform, fakeYasuo.transform.position, stabDirection.normalized);
                    var projectile2 = fakeYasuo.AddUnitComponent<ProjectileDistance>();
                    projectile2.Direction = stabDirection;
                    projectile2.Speed = MathUtility.CalculateSpeed(5.0f, 0.5f);
                    projectile2.Acceleration = MathUtility.CaclulateAcc(5.0f, 0.5f);
                    projectile2.MaxDistance = distance;
                    projectile2.MaxLifetime = 0.5f;
                    projectile2.Begin();
                    AddTimer(1.0f, () => { fakeYasuo.Die(); });
                }).Start();
            };
        }

        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;

            OnActionActingBegin += () =>
            {
                for (int i = 0; i < 2; i++)
                {
                    var left = MathUtility.RotateDirectionY(TargetDirectionNoY, -90.0f + 180 * i);
                    Stab(AbiOwner.WorldPosition, left, 5.0f);
                }
            };
        }
    }
}