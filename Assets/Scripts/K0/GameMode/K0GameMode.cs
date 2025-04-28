using Obi;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace K0
{
    public class K0GameMode : GameMode
    {
        public ControllerBase LocalPlayerController;
        public override void OnAwake()
        {
            base.OnAwake();
//            LocalPlayerController.enabled = false;
            
        }

        public ObiSolver Solver;

        public AssetReference LoadingLevel;
        public void Win(AssetReference NextLevel)
        {
            var activeScene = SceneManager.GetActiveScene();
            var handle = LoadingLevel.LoadSceneAsync(LoadSceneMode.Additive);
            handle.WaitForCompletion();
            var roots = handle.Result.Scene.GetRootGameObjects();
            foreach (var it in roots)
            {
                var loadingScene = it.GetComponent<LoadingScene>();
                if (loadingScene)
                {
                    loadingScene.NextLevel = NextLevel;
                    loadingScene.BeginLoad();
                }
            }
            SceneManager.UnloadSceneAsync(activeScene);
            
            
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