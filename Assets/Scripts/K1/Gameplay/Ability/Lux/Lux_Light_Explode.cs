using DG.Tweening;
using UnityEngine;

namespace K1.Gameplay
{
    /*
     拉克丝施展荧光闪烁魔咒，发射一个光球到天空中，每隔1秒，光球会对附近的敌人造成魔法伤害，并附加一个感光标记。

     */
    // 拉克丝弹反
    public class Lux_Light_Explode : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
            OnAbiBegin += (abi) => { };
        }

        public VariantRef<GameObject> ChargeVFX = new();

        protected override void ActionCastBegin()
        {
            base.ActionCastBegin();
            var endure = GameplayConfig.Instance().DefaultEndureBuff.CreateBuff();
            endure.SetLifetime(DataCastPointAt());
            endure.AddTo(AbiOwner, AbiOwner);
            KGameCore.SystemAt<CameraModule>().FieldView(100, 2.0f, 0, 0.1f);

            bool breaked = false;
            var targetPosition = AbiOwner.WorldPosition + TargetDirectionNoY * 4.5f + Vector3.up;
            targetPosition = Utility.DetectGround(targetPosition, GameUnitAPI.GetGroundMask());
            var vfx = VfxAPI.CreateVisualEffect(ChargeVFX, targetPosition, Vector3.forward);
            soloActionResult.OnActingBreak += (p) =>
            {
                breaked = true;
                vfx.Die();
                vfx = null;
            };
            AddTimer(0.3f, () =>
            {
                if (!vfx || breaked)
                    return;
                OverlapSphereEnemy<CharacterUnit>(targetPosition, 5, out var ret);
                foreach (var selection in ret)
                {
                    DamageParam param = new DamageParam()
                    {
                        Source = AbiOwner,
                        DamageType = DamageType.MagicDamage,
                        DamageValue = DataMultipleAt() * AbiOwner.RealMagicDamage * 0.3f,
                        ValueLevel = ValueLevel.LevelMax
                    };
                    selection.TryTakeDamage(param);
                    selection.PlayHitAnim("Hit");
                    var audio = KGameCore.SystemAt<AudioModule>()
                        .PlayAudioAtPosition(ActionConfig.Audios[ActAbiDataKey.Key0].RandomAccess(),
                            selection.WorldPosition);
                }
            }, 5).Start();
            AddTimer(2.0f, () =>
            {
                if (!vfx || breaked)
                    return;

                KGameCore.SystemAt<CameraModule>().ShakeCamera(0.5f, KGameCore.SystemAt<CameraModule>().mHighShake);
                OverlapSphereEnemy<CharacterUnit>(targetPosition, 5, out var ret);
                foreach (var selection in ret)
                {
                    DamageParam param = new DamageParam()
                    {
                        Source = AbiOwner,
                        DamageType = DamageType.MagicDamage,
                        DamageValue = DataMultipleAt() * AbiOwner.RealMagicDamage,
                        ValueLevel = ValueLevel.LevelMax
                    };
                    selection.TryTakeDamage(param);
                    var stun = GameplayConfig.Instance().CreateStunBuff();
                    stun.StunLevel = ValueLevel.LevelMax;
                    stun.SetLifetime(2.0f);
                    stun.AddTo(AbiOwner, selection);

                    var movementBuff = CharacterUnitAPI.CreateMovementBuff() as MovementBuff;
                    float moveSpeed = 20 * (selection.WorldPosition - targetPosition).magnitude / 3.0f;
                    ;
                    movementBuff.SetAcceleration(-8.0f);
                    movementBuff.SetDirection(TargetDirectionNoY)
                        .SetMoveSpeed(moveSpeed)
                        .SetLifetime(0.4f)
                        .AddTo(AbiOwner, selection);
                }
            }).Start();
        }
    }
}