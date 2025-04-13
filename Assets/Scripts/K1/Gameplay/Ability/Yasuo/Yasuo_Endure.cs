namespace K1.Gameplay
{
    // 霸体Buff
    public class Yasuo_Endure : ActionAbility
    {
        public bool HasVfx = true;

        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.NoTarget;
        }

        protected override void AbiBegin()
        {
            base.AbiBegin();
            mBuff = null;
        }

        private Buffbase mBuff;

        protected override void ActionActBegin()
        {
            base.ActionActBegin();
            mBuff = GameplayConfig.Instance().DefaultEndureBuff.CreateBuff();
            mBuff.HasVFX = HasVfx;
            mBuff.SetLifetime(3)
                .AddTo(AbiOwner, AbiOwner);
        }
    }
}