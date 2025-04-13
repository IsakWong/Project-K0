using Framework.Foundation;
using UnityEngine;
using Random = System.Random;

namespace K1.Gameplay
{
    public class LuxTalent : TalentNode
    {
        public override void OnTalentEnable()
        {
            mOwner.CharEvent.OnManaChange += (unit =>
            {
                var lightObj = unit.GetSocketTransform(BuiltinCharacterSocket.Weapon).transform.Find("Light");
                if (lightObj != null)
                {
                    lightObj.gameObject.GetComponent<Light>().intensity = 0.2f + 0.5f * mOwner.ManaPercent;
                }
            });
        }
    }

    // 安全套
    public class LuxTalent_SaySorry : TalentNode
    {
    }

    //阴暗沉重的秘密
    public class LuxTalent_Dark : TalentNode
    {
    }

    public class LuxTalent_Purple : TalentNode
    {
        public override void OnTalentEnable()
        {
            base.OnTalentEnable();
            KGameCore.SystemAt<GameplayModule>().AnyEvent.OnGetStuned += OnStuned;
        }

        private void OnStuned(CharacterUnit stuned, CharacterUnit source, float time)
        {
            if (source != mOwner)
                return;

            var pos = MathUtility.RandomPointInCircle(source.WorldPosition, new Vector3(2, 0, 2));
            pos.y += 1.5f;
            var func = FuncUnit.Spawn(source.WorldPosition + pos, Vector3.one);
            var vfx = VfxAPI.CreateVisualEffectAtUnit(Config.Data["VFX"].Get<GameObject>(), func,
                Vector3.one, Vector3.zero);
            var distance = func.gameObject.AddComponent<TransformTargetProjectile>();
            distance.IgnoreY = false;
            distance.mTarget = stuned.transform;
            distance.Mode = MoveMode.Bezier;

            distance.Acceleration = 8.0f;
            distance.Direction = -Vector3.up;

            distance.OnArrive += delegate(ProjectileBase project)
            {
                var result = GameUnitAPI.OverlapGameUnitInSphere(stuned.WorldPosition, 0.5f, GameUnitAPI.GetUnitMask(),
                    (unit) =>
                    {
                        CharacterUnit character = unit as CharacterUnit;
                        if (CharacterUnitAPI.IsEnemy(mOwner, character))
                        {
                            CharacterUnitAPI.CharacterDamage(mOwner, character, 1.0f);
                        }
                    });
                vfx.Die();
            };
            mOwner.TimerManager.AddTimer(1.0f, () =>
            {
                func.onLogic += distance.Logic;
                distance.Begin();
            }).Start();
        }
    }

    //给我道歉
    public class LuxTalent_Similar : TalentNode
    {
    }

    //杀人书
    public class LuxTalent_KillBook : TalentNode
    {
    }
}