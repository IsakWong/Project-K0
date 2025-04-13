using DG.Tweening;
using UnityEngine;

namespace K1.Gameplay
{
    public class Lux_R1 : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
        }

        private KTimer timer;

        protected void CreateLighting()
        {
            float lastTime = 0.0f;
            float freq = 0.1f;
            timer = AddTimer(freq, () =>
            {
                lastTime += freq;
                int loop = (int)lastTime;
                int max = (int)Mathf.Max(3.5f - loop, 0);
                if (Random.Range(0, max) == 0)
                {
                    float r = 3.5f;
                    float x = Random.Range(-r, r);
                    float z = Random.Range(-r, r);
                    float y = 7.0f;
                    Vector3 source = AbiOwner.WorldPosition + new Vector3(x, y, z);
                    Vector3 target = AbiOwner.WorldPosition + new Vector3(x, AbiOwner.WorldPosition.y, z);
                    var prefab = VfxAPI.CreateVisualEffect(DataVisualAt(ActAbiDataKey.Key1),
                        source, (target - source).normalized);
                    CharacterUnitAPI.ForCharacterInSphere(AbiOwner.transform.position,
                        4.0f,
                        (selection) =>
                        {
                            MovementBuff buff =
                                GameplayConfig.Instance().DefaultMovement.CreateBuff() as MovementBuff;
                            buff.SetDirection(AbiOwner.DirectionBetweenUnit(selection))
                                .SetMoveSpeed(0.5f)
                                .SetLifetime(0.1f)
                                .AddTo(AbiOwner, selection);

                            GameplayConfig.Instance().CreateStunBuff()
                                .SetLifetime(0.5f)
                                .AddTo(AbiOwner, selection);

                            CharacterUnitAPI.CharacterDamage(AbiOwner, selection, AbiOwner.RealMagicDamage);
                        });
                }
            }, -1);
            timer.Start();
        }

        protected override void ActionCastBegin()
        {
            base.ActionCastBegin();
            var buff = CharacterUnitAPI.CreateEndureBuff();
            buff.SetLifetime(6.0f);
            buff.AddTo(AbiOwner, AbiOwner);
            UIManager.Instance.GetUI<UIMainPanel>().mCharacterUltPanel.ShowPanel(0.1f, 1.0f);
            var seq = MakeSequence();
            KGameCore.SystemAt<CameraModule>().EnvDirectionalLight.DOIntensity(0f, 0.2f);
            seq.AppendInterval(1.3f);
            seq.AppendCallback(() =>
            {
                var camModule = KGameCore.SystemAt<CameraModule>();
                ApplyAnimatorParam();
            });
            seq.AppendInterval(0.80f);
            seq.AppendCallback(() =>
            {
                VfxAPI.CreateVisualEffect(DataVisualAt(ActAbiDataKey.Key2),
                    AbiOwner.WorldPosition, Vector3.forward);
            });
            seq.AppendInterval(0.50f);
            seq.AppendCallback(() => { KGameCore.SystemAt<CameraModule>().EnvDirectionalLight.DOIntensity(3.0f, 0.2f); }
            );
            seq.AppendInterval(0.55f);
            seq.AppendCallback(() =>
                {
                    KGameCore.SystemAt<CameraModule>().EnvDirectionalLight.DOIntensity(1.0f, 0.5f);
                    KGameCore.SystemAt<CameraModule>().ShakeCamera(5.0f);
                    KGameCore.SystemAt<CameraModule>().EnvDirectionalLight.DOIntensity(1.0f, 0.3f);
                    VfxAPI.CreateVisualEffect(DataVisualAt(ActAbiDataKey.Key0),
                        AbiOwner.WorldPosition, Vector3.forward);
                    CreateLighting();
                }
            );
            seq.AppendInterval(5.0f);
            seq.AppendCallback(() => { timer.Stop(); });
        }

        protected override void ActionActBegin()
        {
        }
    }
}