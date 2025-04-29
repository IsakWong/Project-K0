using UnityEngine;

namespace K0.Gameplay
{
    public class ProjectConstantK0
    {
        public static int kLayerPlayer = LayerMask.NameToLayer("Player");
        public static int kLayerGround = LayerMask.NameToLayer("Ground");
        public static int kLayerInteractable = LayerMask.NameToLayer("Interactable");
        public static int kLayerFluid = LayerMask.NameToLayer("Fluid");
        public static int kLayerFluidInteractable = LayerMask.NameToLayer("FluidInteractable");
    }
}