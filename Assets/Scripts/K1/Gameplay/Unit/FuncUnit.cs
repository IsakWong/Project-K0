using System;
using UnityEngine;

namespace K1.Gameplay
{
    public class FuncUnit : GameUnit
    {
        public float _lifeTime = -1.0f;

        public float Lifetime
        {
            get => _lifeTime;
            set
            {
                _remainTime = value;
                _lifeTime = value;
            }
        }

        public FuncUnit()
        {
        }

        private float _remainTime = 0.0f;

        protected new void Awake()
        {
            var layer = LayerMask.NameToLayer("FuncUnit");
            gameObject.layer = layer;
            EnableOnLogic = true;
        }

        public override void Die()
        {
            base.Die();
        }

        public bool IsProjectile = false;

        public override void OnLogic()
        {
            base.OnLogic();
            if (_remainTime > 0)
            {
                _remainTime -= KTime.scaleDeltaTime;
                if (_remainTime <= 0)
                    Die();
            }
        }

        public override void OnUnitInactive()
        {
            base.OnUnitInactive();
        }

        public override void OnUnitActive()
        {
            base.OnUnitActive();
            if (Lifetime > 0)
                _remainTime = Lifetime;
        }

        public static FuncUnit Spawn(Vector3 position, Vector3 direction, GameObject attach = null)
        {
            if (attach is null)
                attach = new GameObject();
            attach.layer = LayerMask.NameToLayer("FuncUnit");
            FuncUnit func = attach.GetComponent<FuncUnit>();
            if (func is null)
                func = attach.AddComponent<FuncUnit>();
            func.transform.position = position;
            func.transform.rotation = Quaternion.LookRotation(direction);
            func.Spawn();
            return func;
        }
    }
}