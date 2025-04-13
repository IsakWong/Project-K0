using System.Collections.Generic;
using System.Linq.Expressions;
using DG.Tweening;
using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class Lux_Dash_Buff : Buffbase
    {
    }

    public class Lux_Dash : ActionAbility
    {
        private KTimer timer = null;
        public bool Dashed = false;

        public override Vector3 CorrectTargetLocation(Vector3 walkDirection, Quaternion cameraDirection)
        {
            base.CorrectTargetLocation(walkDirection, cameraDirection);
            Vector3 newPos;
            if (walkDirection.z.Equals(1))
                walkDirection.z = 0;
            if (walkDirection == Vector3.zero)
                walkDirection = -Vector3.forward;

            newPos = AbiOwner.WorldPosition + cameraDirection * walkDirection * 2.5f;
            newPos = Utility.DetectGround(newPos, GameUnitAPI.GetGroundMask());
            return newPos;
        }

        public VariantRef<BuffConfig> LuxMovementBuff = new VariantRef<BuffConfig>();

        public override void Init()
        {
            base.Init();
            HighPiority = true;
            CastType = ActionCastType.Location;
            NeedFaceTarget = false;
            OnAbiBegin += ability =>
            {
                var invincibleBuff = DataBuffAt(ActAbiDataKey.Key1).CreateBuff() as InvincibleBuff;
                invincibleBuff.SetLifetime(DataActingTimeAt());
                invincibleBuff.AddTo(AbiOwner, AbiOwner);
            };
            OnActionActingBegin += () =>
            {
                var luxDash = LuxMovementBuff.As().CreateBuff() as MovementBuff;

                luxDash.SetMoveSpeed(MoveSpeed);
                luxDash.SetDirection(TargetDirectionNoY);
                luxDash.SetAcceleration(Acceleration);
                luxDash.SetLifetime(DataActingTimeAt())
                    .AddTo(AbiOwner, AbiOwner);
            };
        }

        public VariantRef<float> MoveSpeed = new VariantRef<float>();
        public VariantRef<float> Distance = new VariantRef<float>();
        public VariantRef<float> Acceleration = new VariantRef<float>();

        protected override void AbiBegin()
        {
            base.AbiBegin();
        }

        protected override void ActionActBegin()
        {
            base.ActionActBegin();
            KGameCore.SystemAt<CameraModule>().PostProcess(0.1f, 0.2f);
        }
    }

    class LuxMovement : MovementBuff
    {
    }

    class LuxDashInvicible : InvincibleBuff
    {
        private bool _slowed = false;
        GameplayModule.TimeScaleTask task = null;

        public override void OnInvicibleEffect(CharacterUnit damageSource)
        {
            base.OnInvicibleEffect(damageSource);
            if (!_slowed)
            {
                source = damageSource;
                var offset = Camera.main.transform.forward;
                offset.y = 0;
                task = KGameCore.SystemAt<GameplayModule>().PushTimeScale(0.2f, 0.7f);
                KGameCore.SystemAt<AudioModule>().PlayAudio(BuffOwner.GetAbility<Lux_Dash>().DataAudioAt());
                KGameCore.SystemAt<HUDModule>().EndureTextPrefab
                    .SpawnText(BuffOwner.WorldPosition + offset * 1.5f, "完美闪避!");
                _slowed = true;
                var buf = new Lux_Dash_Buff();
                buf.SetLifetime(0.7f);
                buf.AddTo(BuffOwner, BuffOwner);
            }
        }

        private CharacterUnit source;

        public override void BuffEnd()
        {
            base.BuffEnd();
            if (_slowed)
            {
                var direction = Camera.main.transform.forward;
                for (int i = 0; i < Random.Range(2, 4); i++)
                {
                    float angle = Random.Range(60, 140);
                    var dir1 = MathUtility.RotateDirectionY(direction, -angle + angle * 2 * i);
                    var pos1 = BuffOwner.WorldPosition + dir1 * 0.4f;

                    pos1.y += Random.Range(1.0f, 2.0f);

                    var func = FuncUnit.Spawn(pos1, Vector3.forward);
                    func.CreateSocketVisual(BuffConfig.Datas["Projectile"], "", Vector3.zero, Vector3.one);
                    var distance = func.AddUnitComponent<TransformTargetProjectile>();
                    //distance.Direction = GameUnitAPI.DirectionBetweenUnit(func, source);
                    distance.Mode = MoveMode.Bezier;
                    distance.mTarget = source.transform;
                    distance.Angle = -90 + 180 * i;
                    distance.MaxSpeed = 40.0f;
                    distance.Acceleration = 20.0f;
                    distance.CollideRange = 0.5f;
                    distance.Speed = 20.0f;
                    distance.ControlDistance = 0.3f;
                    distance.Begin();
                    distance.CollideLayerMask = GameUnitAPI.GetCharacterLayer() | GameUnitAPI.GetEnvLayerMask();
                    distance.OnCollideUnit += (selection) =>
                    {
                        if (selection.GetComponent<GameUnit>() == BuffOwner)
                            return;
                        func.Die();
                    };
                    distance.OnFinish += (u) =>
                    {
                        DamageParam param = new DamageParam()
                        {
                            DamageType = DamageType.MagicDamage,
                            Source = BuffOwner,
                            DamageValue = BuffOwner.RealMagicDamage * 0.4f,
                            ValueLevel = ValueLevel.Level1,
                        };
                        source.TryTakeDamage(param);
                        HitParam hitParam = new HitParam(BuffOwner)
                        {
                            PlayAnimation = true,
                            HitTime = 0.2f,
                            ValueLevel = ValueLevel.Level0
                        };
                        source.TryGetHitted(hitParam);
                        func.Die();
                    };
                }
            }

            if (_slowed && task != null)
            {
                //task.duration = -1.0f;
            }
        }
    }
}