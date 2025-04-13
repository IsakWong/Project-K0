using DG.Tweening;
using UnityEngine;

namespace K1.Gameplay
{
    public static class AbilityAPI
    {
        public static Vfx CreateSqureWarning(Vector3 postion, Vector3 lookDirection, Vector3 halfSize, float time,
            ValueLevel warningLevel = ValueLevel.Level1)
        {
            var hud = KGameCore.SystemAt<HUDModule>();
            var result = VfxAPI.CreateVisualEffectWithLifeTime(
                hud.mWarningCube,
                postion,
                lookDirection,
                time);
            result.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material =
                hud.mWarningMats[(int)(warningLevel) - 1];
            result.UnityDestroyDelay = 0.1f;
            halfSize = halfSize * 2;

            Vector3 v = new Vector3(halfSize.x, result.transform.localScale.y, halfSize.z);
            result.transform.localScale = Vector3.zero;
            result.transform.DOScale(v, 0.2f);
            return result;
        }

        public static Vfx CreateCicleWarning(Vector3 postion, Vector3 halfSize, float time,
            ValueLevel warningLevel = ValueLevel.Level1, Transform parent = null)
        {
            var hud = KGameCore.SystemAt<HUDModule>();
            var result = VfxAPI.CreateVisualEffectWithLifeTime(hud.mWarningCircle, postion,
                Vector3.forward,
                time);
            if (parent)
                result.transform.SetParent(parent, true);
            result.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material =
                hud.mWarningMats[(int)(warningLevel) - 1];
            result.UnityDestroyDelay = 0.1f;
            halfSize = halfSize * 2;
            Vector3 v = new Vector3(halfSize.x, result.transform.localScale.y, halfSize.z);
            result.transform.localScale = Vector3.zero;
            result.transform.DOScale(v, 0.2f);
            return result;
        }
    }
}