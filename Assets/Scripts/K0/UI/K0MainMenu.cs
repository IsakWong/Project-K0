using K1.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace K0
{
    public class K0MainMenu : UIPanel
    {
        public CinemachineCamera HelpCamera;
        public CinemachineCamera StartCamera;
        
        public void OnStartClick()
        {
            var k0GameMode = KGameCore.Instance.CurrentGameMode as K0GameMode;
            k0GameMode.StartK0Game();
            KGameCore.SystemAt<CameraModule>().PushCamera(StartCamera, true, -1.0f);
        }

        public void OnBackClick()
        {
        
        }
        public void OnExitClick()
        {
            Application.Quit();
        }

        public void OnHelpClick()
        {
            KGameCore.SystemAt<CameraModule>().PushCamera(HelpCamera, true, -1.0f);
        }
    }
}