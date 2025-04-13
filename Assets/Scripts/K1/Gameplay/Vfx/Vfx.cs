using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace K1.Gameplay
{
    // 特效挂载位置
    [Serializable]
    public class UnitVfxConfig
    {
        public GameObject mVisualPrefab;
        public BuiltinCharacterSocket mBuiltinSocket;
        public string mCustomSocket;
        public Vector3 mOffset;
        public Quaternion mRotation;
        public Vector3 mScale = Vector3.one;
    }

    [DisallowMultipleComponent]
    public class Vfx : GameUnit
    {
        public float mLifeTime = 1;
        public List<Transform> ParticleParents = new List<Transform>();
        public Action<Vfx> OnDie;
        public Action<Vfx> EventSpawn;
        public static HashSet<Vfx> AllVfx = new HashSet<Vfx>();

        public override void Spawn()
        {
            var anim = GetComponent<VfxAnim>();
            if (anim)
                anim.Spawn();
            OnSpawn();
            EventSpawn?.Invoke(this);
            AllVfx.Add(this);
        }

        private bool _isDead = false;

        public override void Die()
        {
            if (_isDead)
                return;
            var anim = GetComponent<VfxAnim>();
            if (anim)
                anim.Die();
            _isDead = true;
            OnDie?.Invoke(this);
            Destroy(gameObject, UnityDestroyDelay);
            AllVfx.Remove(this);
        }

        protected GameplayModule cacheGameplay;

        protected void Start()
        {
            gameObject.layer = GameUnitAPI.GetEffectLayer();
            if (mLifeTime > 0)
                Invoke("Die", mLifeTime);
        }
    }
}