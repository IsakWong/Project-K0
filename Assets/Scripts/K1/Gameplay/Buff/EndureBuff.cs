namespace K1.Gameplay
{
    //霸体 Buff
    public class EndureBuff : Buffbase
    {
        public ValueLevel EndureLevel = ValueLevel.LevelMax;

        public override void BuffAdd()
        {
            base.BuffAdd();
            if (EndureLevel > ValueLevel.Level1)
                BuffOwner.EnableOutline(true);
        }

        public virtual void OnEndure()
        {
        }

        public override void BuffEnd()
        {
            base.BuffEnd();
            if (EndureLevel > ValueLevel.Level1)
                BuffOwner.EnableOutline(false);
        }
    }
}