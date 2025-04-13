namespace K1.Gameplay
{
    //防御Buff
    public class DefensiveBuff : Buffbase
    {
        public ValueLevel DefenceLevel = ValueLevel.Level2;

        public virtual void OnDefence(DamageParam damage)
        {
        }
    }
}