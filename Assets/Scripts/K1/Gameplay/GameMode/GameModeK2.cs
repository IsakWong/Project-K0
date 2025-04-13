using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace K1.Gameplay
{
    public class GameModeK2 : GameMode
    {

        public virtual void OnAwake()
        {
            base.OnAwake();

            KGameCore.Instance.RequireModule<HUDModule>();
            KGameCore.Instance.RequireModule<GameplayModule>();
            KGameCore.Instance.RequireModule<CameraModule>();
            KGameCore.Instance.RequireModule<PerformModule>();
            KGameCore.Instance.RequireModule<PlayerModule>();
        }
        public CharacterUnit Lux;
        private KTimer timer;
        public override void OnModeBegin()
        {
            base.OnModeBegin();
            KGameCore.SystemAt<PlayerModule>().SwitchInputMode("TopdownBattle");
            KGameCore.SystemAt<AudioModule>()
                .AttachAudioListener(KGameCore.SystemAt<CameraModule>().mCameraBrain.gameObject);
            Lux.gameObject.SetActive(true);
            KGameCore.SystemAt<GameplayModule>().BeginGame();
        }
        
        // Update is called once per frame
        void FixedUpdate()
        {

        }
    }

}