namespace Gameplay.AI
{
    public class YasuoBossController  : AICharacterController
    {
        
        protected void Awake()
        {
            base.Awake();
            //ControlCharacter.CharEvent.OnTakeDamage += (hitted, hitter, param) => { };
            SwitchBehaviour(DefaultPattern);
        }

        public void InitBehaviorBattle()
        {
            using (new ScopeSequence(CurrentBuilder))
            {
                CurrentBuilder.DetectEnemy();
                CurrentBuilder.Walk(() =>
                {
                    CurrentBuilder.Walk_TargetLocation = CurrentBuilder.TargetUnitLocation;
                });
                CurrentBuilder.WaitTime(1.0f);
            }
        }
    }
}