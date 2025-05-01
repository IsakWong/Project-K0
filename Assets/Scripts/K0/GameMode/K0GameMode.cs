using System;
using DG.Tweening;
using Obi;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace K0
{
    public class K0GameMode : GameMode
    {
        public ControllerBase LocalPlayerController;
        public Action OnSeedGrowEvent;
        public int GrowSeedCount;
        public int MaxSeedCount;
        public AudioClip WinSFX;
        public AudioClip LoseSFX;
        public AudioClip OpeningSFX;
        public AudioSource BGM;
        public override void OnAwake()
        {
            base.OnAwake();
            LocalPlayerController = KGameCore.SystemAt<PlayerModule>().LocalPlayerController; 
            StartK0Game();
            
//            LocalPlayerController.enabled = false;
            
        }
    
        PlayerControllerK0 Controller
        {
            get
            {
                if (LocalPlayerController is PlayerControllerK0 controller)
                {
                    return controller;
                }
                return null;
            }
        }
        
        public void GrowSeed()
        {
            OnSeedGrowEvent?.Invoke();
            
        }
        public ObiSolver Solver;

        public AssetReference LoadingLevel;

        public void Lose()
        {
            Controller.Slime.SlimeRigidBody.isKinematic = false;
            KGameCore.SystemAt<AudioModule>().PlayAudio(LoseSFX);
            KGameCore.SystemAt<PlayerModule>().SwitchInputMode("UI");
            BGM.gameObject.SetActive(false);
        }
        
        public void Win(AssetReference NextLevel)
        {
            KGameCore.SystemAt<AudioModule>().PlayAudio(WinSFX);
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
            // Initialize the game mode
            KGameCore.Instance.RequireModule<AudioModule>();
            KGameCore.Instance.RequireModule<UIModule>();
            
            KGameCore.SystemAt<AudioModule>().PlayAudio(OpeningSFX);
            var seq = DOTween.Sequence();
            seq.InsertCallback(OpeningSFX.length, () =>
            {
                BGM.gameObject.SetActive(true);
            });
            

        }
        
    }
}