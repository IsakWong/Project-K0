using UnityEngine;

namespace K1.Gameplay
{
    public class ShadowMovementBuff : MovementBuff
    {
        public VariantRef<float> DeltaDistance = new VariantRef<float>(0.25f);
        public VariantRef<float> ShadowLifetime = new VariantRef<float>(0.25f);
        public VariantRef<Material> ShadowMaterial = new VariantRef<Material>();
        VfxShadowFollow shadownFollow;

        public override void BuffAdd()
        {
            base.BuffAdd();
            shadownFollow = BuffOwner.gameObject.AddComponent<VfxShadowFollow>();
            shadownFollow.mat = ShadowMaterial.As();
            shadownFollow.DeltaDistance = DeltaDistance;
            shadownFollow.ShadowLifetime = ShadowLifetime;
        }

        public override void BuffEnd()
        {
            base.BuffEnd();
            GameObject.Destroy(shadownFollow);
        }

        public override void OnLogic()
        {
            base.OnLogic();
        }
    }
}