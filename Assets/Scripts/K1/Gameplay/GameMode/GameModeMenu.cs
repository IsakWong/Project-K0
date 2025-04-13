using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace K1.Gameplay
{
    public class GameModeMenu : GameMode
    {
        public AudioClip Bgm;

        public override void OnModeBegin()
        {
            base.OnModeBegin();
            KGameCore.SystemAt<AudioModule>().SwitchBgm(Bgm);

            KGameCore.SystemAt<PlayerModule>().SwitchInputMode("UI");
            UIManager.Instance.GetUI<UIMenuPanel>().ShowPanel(0.5f, -1f);
        }
    }
}