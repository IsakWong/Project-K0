namespace K0
{
    public class K0GameMode : GameMode
    {
        public ControllerBase LocalPlayerController;
        public override void OnAwake()
        {
            base.OnAwake();
            LocalPlayerController.enabled = false;
        }

        public void StartK0Game()
        {
            LocalPlayerController.enabled = true;
            // Initialize the game mode
            KGameCore.Instance.RequireModule<AudioModule>();
            KGameCore.Instance.RequireModule<UIModule>();

        }
    }
}