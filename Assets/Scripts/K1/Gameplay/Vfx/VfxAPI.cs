using UnityEngine;

namespace K1.Gameplay
{
    public static class VfxAPI
    {
        private static Transform _effectRoot;

        public static Transform mRootEffect
        {
            get
            {
                if (_effectRoot == null)
                {
                    var obj = new GameObject();
                    obj.name = "[RootEffect]";
                    _effectRoot = obj.GetComponent<Transform>();
                }

                return _effectRoot;
            }
        }

        public static Vfx CreateVisualEffect<T>(GameObject gameobject, Vector3 position, Vector3 direction)
            where T : Vfx
        {
            var result = gameobject.GetComponent<T>();
            if (result == null)
                result = gameobject.AddComponent<T>();
            return CreateVisualEffectWithLifeTime(gameobject, position, direction);
        }

        public static Vfx CreateVisualEffectAtUnit(GameObject source, GameUnit target, Vector3 direction,
            Vector3 offset)
        {
            var result = GameObject.Instantiate(source, target.transform, false);
            var effectBase = result.GetComponent<Vfx>();
            if (effectBase == null)
                effectBase = result.AddComponent<Vfx>();
            result.transform.SetParent(target.transform, false);
            if (direction == Vector3.zero)
                result.transform.localRotation = Quaternion.identity;
            else
                result.transform.localRotation = Quaternion.LookRotation(direction);
            result.transform.localPosition = offset;
            return effectBase;
        }

        public static Vfx CreateVisualEffect(GameObject source, Vector3 position, Vector3 direction)
        {
            var result = GameObject.Instantiate(source);
            var effectBase = result.GetComponent<Vfx>();
            if (null == effectBase)
                effectBase = result.AddComponent<Vfx>();
            result.transform.SetParent(mRootEffect, true);
            result.transform.position = position;
            result.transform.rotation = Quaternion.LookRotation(direction);
            return effectBase;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static Vfx CreateVisualEffectWithLifeTime(GameObject source, Vector3 position, Vector3 direction,
            float lifeTime = -1f)
        {
            var result = GameObject.Instantiate(source);
            var effectBase = result.GetComponent<Vfx>();
            if (null == effectBase)
                effectBase = result.AddComponent<Vfx>();
            effectBase.mLifeTime = lifeTime;
            result.transform.SetParent(mRootEffect, true);
            result.transform.position = position;
            result.transform.rotation = Quaternion.LookRotation(direction);
            return effectBase;
        }

        public static Vfx CreateVisualEffect(GameObject source, Vector3 position)
        {
            var result = GameObject.Instantiate(source.gameObject);
            result.transform.position = position;
            result.transform.SetParent(mRootEffect);
            var effectBase = result.GetComponent<Vfx>();
            if (null == effectBase)
                effectBase = result.AddComponent<Vfx>();
            return effectBase;
        }
    }
}