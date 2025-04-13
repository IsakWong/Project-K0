using DamageNumbersPro;
using UnityEngine;

namespace K1.Gameplay
{
    // 拉克丝弹反
    public class Lux_Shield : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            HighPiority = true;
            CastType = ActionCastType.NoTarget;
        }

        public override bool EndWhenKeyRelease()
        {
            return true;
        }

        public override void ForceEnd()
        {
            if (IsActing())
            {
                AbiOwner.BreakAction(AbiOwner);
            }
        }

        protected override void AbiBegin()
        {
            base.AbiBegin();
            defenceBuff = null;
        }

        protected override void ActionExit()
        {
            base.ActionExit();
            if (defenceBuff != null)
            {
                defenceBuff.BuffOwner.RemoveBuff(defenceBuff);
                defenceBuff = null;
            }

            if (endureBuff != null)
            {
                endureBuff.BuffOwner.RemoveBuff(endureBuff);
                endureBuff = null;
            }
        }


        protected override void ActionCastBegin()
        {
            base.ActionCastBegin();
        }

        private Buffbase defenceBuff;
        private EndureBuff endureBuff;

        protected override void ActionActBegin()
        {
            base.ActionActBegin();
            var buff = DataBuffAt().CreateBuff() as LuxShieldBuff;
            defenceBuff = buff;
            buff.SetLifetime(9999f)
                .AddTo(AbiOwner, AbiOwner);
            endureBuff = GameplayConfig.Instance().DefaultEndureBuff.CreateBuff() as EndureBuff;
            endureBuff.EndureLevel = ValueLevel.Level1;
            endureBuff.SetLifetime(9999f);
            endureBuff.AddTo(AbiOwner, AbiOwner);
        }
    }

    public class LuxShieldBuff : DefensiveBuff
    {
        public VariantRef<GameObject> CollideVisualPrefab = new VariantRef<GameObject>();
        public VariantRef<AudioClip> DefenceAudio = new VariantRef<AudioClip>();

        public override void Init()
        {
            base.Init();
            DefenceLevel = ValueLevel.Level2;
        }

        private float _defenced = 0;

        public override void BuffAdd()
        {
            base.BuffAdd();
            _defenced = 0;
            BuffOwner.SetAnimatorBool("Lux_Is_Defense", true);
        }

        public ObjectVarint<GameObject> ExplodeVFX = new();
        public TVariant<float> ExplodeRange = new();
        public ObjectVarint<GameObject> ExplodeHUD = new();

        public override void BuffEnd()
        {
            if (_defenced > 0)
            {
                foreach (var vfx in mBuffVisuals)
                {
                    vfx.UnityDestroyDelay = 0.05f;
                }
            }

            base.BuffEnd();
        }

        public override void BuffBreak()
        {
            base.BuffBreak();
            if (_defenced > 0)
            {
                if (ExplodeVFX.As())
                {
                    VfxAPI.CreateVisualEffect(ExplodeVFX, BuffOwner.WorldPosition, BuffOwner.transform.forward);
                    GameUnitAPI.GetGameUnitInSphere<CharacterUnit>(BuffOwner.WorldPosition, ExplodeRange,
                        GameUnitAPI.GetCharacterLayerMask(), out var ret);
                    foreach (var selection in ret)
                    {
                        if (!selection.IsEnemy(BuffOwner))
                            continue;
                        DamageParam param = new DamageParam()
                        {
                            Source = BuffOwner,
                            DamageType = DamageType.MagicDamage,
                            DamageValue = _defenced
                        };
                        selection.TryTakeDamage(param);
                        var movementBuff = CharacterUnitAPI.CreateMovementBuff() as MovementBuff;
                        float moveSpeed = 20.0f;
                        movementBuff.SetAcceleration(-8.0f);
                        movementBuff.SetDirection(BuffOwner.DirectionToTarget(selection))
                            .SetMoveSpeed(moveSpeed)
                            .SetLifetime(0.3f)
                            .AddTo(BuffOwner, selection);
                    }

                    BuffOwner.AddTimer(0.1f, () =>
                    {
                        KGameCore.SystemAt<HUDModule>().SpawnHud(HUD.As().GetComponent<DamageNumber>(),
                            BuffOwner.WorldPosition, "光能溢出!");
                    }, 1);
                }
            }

            BuffOwner.SetAnimatorBool("Lux_Is_Defense", false);
        }

        public VariantRef<GameObject> HUD = new();
        public VariantRef<GameObject> DeflectText = new();
        public VariantRef<GameObject> DeflectVFX = new();
        public VariantRef<AudioClip> DeflectAudio = new();
        KTimer hitTimer;

        public override void OnDefence(DamageParam param)
        {
            base.OnDefence(param);
            var target = param.Source;
            var pos = BuffOwner.WorldPosition +
                      (param.Source.WorldPosition - BuffOwner.WorldPosition).normalized * 1.0f;
            // var talent = BuffOwner.GetTalent<LuxTalent_SaySorry>();
            // if (talent != null && param.DamageLevel > DamageLevel.LowLow)
            // {
            //     var abi = BuffOwner.GetAbility<Lux_Attack>();
            //     abi.HighAttack(pos, (param.DamageSource.WorldPosition - BuffOwner.WorldPosition).normalized);
            //     param.DamageSource.Stun(BuffOwner, 3f);                
            // }

            BuffOwner.RecoverMana(5.0f);
            _defenced = 10.0f;
            if (param.ValueLevel <= ValueLevel.Level1)
                param.DamageValue = 0;
            if (GameUnitAPI.DistanceBetweenGameUnit(target, BuffOwner) < 4.0f)
            {
                if (hitTimer == null)
                {
                    //KGameCore.SystemAt<GameplayModule>().PushTimeScale(0.3f, 0.2f);

                    HitParam hitParam = new HitParam(BuffOwner)
                    {
                        Source = BuffOwner,
                        HitTime = 0.6f,
                        PlayAnimation = true,
                        ValueLevel = ValueLevel.Level2,
                        HitAnimTrigger = "Deflect"
                    };

                    target.TryGetHitted(hitParam);

                    hitTimer = target.AddTimer(0.2f, () => { hitTimer = null; }, 1);

                    if (HUD.varaint.Get<GameObject>())
                    {
                        GameObject hud = HUD;
                        var damageHUD = hud.GetComponent<DamageNumber>();
                        KGameCore.SystemAt<HUDModule>().SpawnHud(damageHUD, BuffOwner.WorldPosition, "充能!");
                    }

                    if (DeflectAudio.As())
                    {
                        KGameCore.SystemAt<AudioModule>().PlayAudioAtPosition(DeflectAudio.As(), target.WorldPosition);
                    }

                    KGameCore.Instance.Timers.AddTimer(0.15f,
                        () =>
                        {
                            target.CreateSocketVisual(DeflectVFX.As(), BuiltinCharacterSocket.WeaponDeflect.ToString(),
                                Vector3.zero, Vector3.one);
                        }).Start();

                    target.LookAt(BuffOwner.WorldPosition);
                    var buff = GameplayConfig.Instance().DefaultMovement.CreateBuff() as MovementBuff;
                    buff.SetDirection((target.WorldPosition - BuffOwner.WorldPosition).normalized)
                        .SetMoveSpeed(10)
                        .SetAcceleration(-10.0f)
                        .SetLifetime(0.2f)
                        .AddTo(target, target);
                    KGameCore.SystemAt<CameraModule>().ShakeCamera(0.5f);
                }
            }
            else
            {
                GameObject hud = HUD;
                var damageHUD = hud.GetComponent<DamageNumber>();
                KGameCore.SystemAt<HUDModule>().SpawnHud(damageHUD, BuffOwner.WorldPosition, "格挡!");
                KGameCore.SystemAt<CameraModule>().ShakeCamera(0.25f);
            }


            pos.y += 0.5f;
            var obj = VfxAPI.CreateVisualEffect(CollideVisualPrefab, pos, BuffOwner.DirectionBetweenUnit(target));
        }
    }
}