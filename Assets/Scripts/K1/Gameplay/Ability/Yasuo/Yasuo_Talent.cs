using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Talent : TalentNode
    {
        public VariantRef<GameObject> DeflectVFX = new VariantRef<GameObject>();

        public override void OnTalentEnable()
        {
            base.OnTalentEnable();
            mOwner.CharEvent.OnHit += (source, param) =>
            {
                // KGameCore.Instance.Timers.AddTimer(0.05f, () =>
                // {
                //     var obj = VfxAPI.CreateVisualEffect(DeflectVFX.As(),
                //         mOwner.GetSocketWorldPosition(BuiltinCharacterSocket.Weapon), mOwner.transform.forward);
                // }).Start();
                // var obj2 = VfxAPI.CreateVisualEffect(Config.Datas["DeflectText"].Get<GameObject>(),
                //     mOwner.GetSocketWorldPosition(BuiltinCharacterSocket.Weapon), mOwner.transform.forward);
                //obj2.transform.Translate(0, 0.5f, 0);
            };
        }

        public override void OnTalentDisable()
        {
            base.OnTalentDisable();
        }
    }
}