using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace K1.Gameplay
{
    public class GameModeK1 : GameMode
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

        // Update is called once per frame
        void Update()
        {

        }
    }

}