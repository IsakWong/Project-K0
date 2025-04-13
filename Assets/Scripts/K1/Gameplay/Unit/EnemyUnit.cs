namespace K1.Gameplay
{
    public class EnemyUnit : CharacterUnit
    {
        public new void Init()
        {
            base.OnSpawn();
            mPlayerID = 2;
        }

        // Update is called once per frame
        protected void FixedUpdate()
        {
            // mAIControllerFsm.OnLogic();
        }
    }
}