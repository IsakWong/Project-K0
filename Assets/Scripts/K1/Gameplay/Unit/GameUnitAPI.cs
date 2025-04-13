using System;
using System.Collections.Generic;
using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public static class GameUnitAPI
    {
        public static Vector3 FindGround(Vector3 v)
        {
            var hitinfos = Physics.RaycastAll(v, Vector3.down, 10, GetGroundMask());
            if (hitinfos == null)
                return v;
            if (hitinfos.Length > 0)
            {
                Vector3 foot = hitinfos[0].point;
                return foot;
            }

            return v;
        }

        public static Vector3 DirectionBetweenUnit(this GameUnit a, GameUnit b, bool ignoreY = true)
        {
            var delta = b.transform.position - a.transform.position;
            if (ignoreY)
                delta.y = 0;
            delta.Normalize();
            return delta;
        }

        public static float DistanceBetweenPosition(Vector3 a, Vector3 b, bool ignoreY = true)
        {
            var delta = a - b;
            if (ignoreY)
            {
                delta.y = 0;
            }

            return delta.magnitude;
        }

        public static float DistanceBetweenGameUnit(GameUnit a, GameUnit b, bool ignoreY = true)
        {
            var delta = a.transform.position - b.transform.position;
            if (ignoreY)
            {
                delta.y = 0;
            }

            return delta.magnitude;
        }

        public static bool OverlapGameUnitInBox<T>(Vector3 center, Vector3 halfSize, Quaternion rotation,
            LayerMask mask,
            Action<T> action,
            Func<T, bool> condition = null, bool debugDraw = true)
        {
            if (debugDraw)
            {
                KGizmos.Instance.DrawGizmos(() =>
                {
                    Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(Vector3.zero, halfSize * 2);
                }, 0.5f);
            }


            var colliders = Physics.OverlapBox(center, halfSize, rotation, mask);
            bool result = false;
            T selection;
            if (colliders is null || colliders.Length == 0)
                return false;
            {
                foreach (var it in colliders)
                {
                    selection = it.gameObject.GetComponent<T>();
                    if (selection == null)
                        continue;

                    if (condition != null)
                    {
                        if (condition.Invoke(selection))
                        {
                            result = true;
                            action.Invoke(selection);
                        }
                    }
                    else
                    {
                        result = true;
                        action.Invoke(selection);
                    }
                }
            }
            return result;
        }

        public static bool OverlapGameUnitInSphere(Vector3 location, float range, LayerMask mask,
            Action<GameUnit> action)
        {
            return OverlapGameUnitInSphere<GameUnit>(location, range, mask, action);
        }

        public static bool GetGameUnitInBox<T>(Vector3 location, Vector3 halfSize, Quaternion direction, LayerMask mask,
            out List<T> result, Func<T, bool> condition = null, bool debugDraw = true)
            where T : GameUnit
        {
            if (debugDraw)
            {
                KGizmos.Instance.DrawGizmos(() =>
                {
                    Gizmos.matrix = Matrix4x4.TRS(location, direction, Vector3.one);
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(Vector3.zero, halfSize * 2);
                }, 0.2f);
            }


            var colliders = Physics.OverlapBox(location, halfSize, direction, mask);
            T selection;
            result = new List<T>();
            if (colliders is null || colliders.Length == 0)
                return false;
            foreach (var it in colliders)
            {
                selection = it.gameObject.GetComponent<T>();
                if (selection == null)
                    continue;
                if (condition?.Invoke(selection) == false)
                    continue;
                result.Add(selection);
            }

            return true;
        }

        public static bool GetGameUnitInSphere<T>(Vector3 location, float range, LayerMask mask, out List<T> result,
            Func<T, bool> condition = null, bool debugDraw = true) where T : GameUnit
        {
            if (debugDraw)
            {
                var color = Color.red;
                color.a = 0.3f;
                KGizmos.Instance.DrawSphere(
                    Vector3.zero,
                    range,
                    duration: 0.2f,
                    matrix: Matrix4x4.TRS(location, Quaternion.identity, Vector3.one), color: color);
            }


            var colliders = Physics.OverlapSphere(location, range, mask);
            T selection;
            result = new List<T>();
            if (colliders is null || colliders.Length == 0)
                return false;
            foreach (var it in colliders)
            {
                selection = it.gameObject.GetComponent<T>();
                if (selection == null)
                    continue;
                if (condition?.Invoke(selection) == false)
                    continue;
                result.Add(selection);
            }

            return true;
        }

        public static bool OverlapGameUnitInSphere<T>(Vector3 location, float range, LayerMask mask,
            Action<T> action,
            Func<T, bool> condition = null)
        {
            KGizmos.Instance.DrawSphere(
                Vector3.zero,
                range,
                duration: 0.2f,
                matrix: Matrix4x4.TRS(location, Quaternion.identity, Vector3.one),
                color: Color.red);

            var colliders = Physics.OverlapSphere(location, range, mask);
            bool result = false;
            T selection;
            if (colliders is null || colliders.Length == 0)
                return false;

            foreach (var it in colliders)
            {
                selection = it.gameObject.GetComponent<T>();
                if (selection == null)
                    continue;
                if (condition == null)
                {
                    result = true;
                    action.Invoke(selection);
                    continue;
                }

                if (condition?.Invoke(selection) == false)
                {
                    result = true;
                    action.Invoke(selection);
                }
            }

            return result;
        }

        public static bool OverlapGameUnitInSector<T>(Vector3 location, Vector3 direction, float range, float angle,
            LayerMask mask,
            Action<T> action,
            Func<T, bool> condition = null)
        {
            KGizmos.Instance.DrawGizmos(
                () =>
                {
                    Gizmos.matrix = Matrix4x4.TRS(location, Quaternion.LookRotation(direction), Vector3.one);
                    Gizmos.color = Color.red;
                    Gizmos.DrawMesh(KGizmos.GenerateSectionMesh(range, angle));
                }, 0.2f);
            var colliders = Physics.OverlapSphere(location, range, mask);
            bool result = false;
            T selection;
            if (colliders is null || colliders.Length == 0)
                return false;
            foreach (var it in colliders)
            {
                selection = it.gameObject.GetComponent<T>();
                if (selection == null)
                    continue;
                if (!Utility.IsInSection(location, it.transform.position, direction, angle, range))
                    continue;
                if (condition == null)
                {
                    result = true;
                    action.Invoke(selection);
                    continue;
                }

                if (condition.Invoke(selection))
                {
                    result = true;
                    action.Invoke(selection);
                }
            }

            return result;
        }

        public static int GetCharacterLayer()
        {
            return LayerMask.NameToLayer("CharacterUnit");
        }

        public static int GetEffectLayer()
        {
            return LayerMask.NameToLayer("Effect");
        }

        public static int GetEnvLayer()
        {
            return LayerMask.NameToLayer("EnvUnit");
        }

        public static int GetGround()
        {
            return LayerMask.NameToLayer("Ground");
        }

        public static int GetGroundMask()
        {
            return 1 << LayerMask.NameToLayer("Ground");
        }

        public static int GetFuncUnitMask()
        {
            return 1 << LayerMask.NameToLayer("FuncUnit");
        }

        public static LayerMask GetUnitMask()
        {
            return 1 << LayerMask.NameToLayer("EnvUnit") | 1 << LayerMask.NameToLayer("CharacterUnit");
        }

        public static LayerMask GetLayerMask<T>()
        {
            if (typeof(T) == typeof(CharacterUnit))
                return 1 << GetCharacterLayer();
            if (typeof(T) == typeof(GameUnit))
                return 1 << LayerMask.NameToLayer("EnvUnit") | 1 << LayerMask.NameToLayer("CharacterUnit");
            return LayerMask.NameToLayer("Default");
        }

        public static LayerMask GetCharacterLayerMask()
        {
            return 1 << GetCharacterLayer();
        }


        public static LayerMask GetEnvLayerMask()
        {
            return 1 << GetEnvLayer();
        }
    }
}