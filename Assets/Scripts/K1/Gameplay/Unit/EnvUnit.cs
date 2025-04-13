using UnityEngine;
using UnityEngine.Serialization;

namespace K1.Gameplay
{
    public class EnvUnit : GameUnit
    {
        [FormerlySerializedAs("mPickable")] public bool Pickable = false;
        [FormerlySerializedAs("mHasRigibody")] public bool HasRigibody = false;

        protected new void Start()
        {
            if (HasRigibody)
            {
                var rigibody = gameObject.GetComponent<Rigidbody>();
            }

            gameObject.layer = GameUnitAPI.GetEnvLayer();
        }
    }
}