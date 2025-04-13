using System.Collections.Generic;
using DG.Tweening;
using DTT.AreaOfEffectRegions;
using UnityEngine;

namespace K1.Gameplay
{
    public class Indicator : MonoBehaviour
    {
        public CircleRegion mCircleRegion;
        public Transform mRoot;
        public bool mHighlightSelection = true;
        public HashSet<GameUnit> mSelections = new HashSet<GameUnit>();

        public void ShowIndicator()
        {
            gameObject.SetActive(true);
        }

        public void ShowAndDestroy(float time)
        {
            Sequence sequence = DOTween.Sequence();
            if (mRoot)
                mRoot.localScale = Vector3.zero;
            var scale = mRoot.DOScale(Vector3.one * 0.3f, 0.1f);
            sequence.Append(scale);
            sequence.AppendInterval(time);
            var scale2 = mRoot.DOScale(Vector3.zero, 0.1f);
            sequence.Append(scale2);
            sequence.AppendCallback(() => { Destroy(gameObject); });
        }

        public void HideIndicator()
        {
            foreach (var selection in mSelections)
            {
                selection.EnableOutline(false);
            }

            mSelections.Clear();
            gameObject.SetActive(false);
        }

        public void FixedUpdate()
        {
            if (!mHighlightSelection)
                return;

            HashSet<GameUnit> selections = new HashSet<GameUnit>();
            GameUnitAPI.OverlapGameUnitInSphere(transform.position, transform.localScale.x,
                GameUnitAPI.GetCharacterLayerMask(),
                (GameUnit unit) =>
                {
                    if (!selections.Contains(unit))
                    {
                        selections.Add(unit);
                        unit.EnableOutline(true);
                    }
                });
            foreach (var unit in mSelections)
            {
                if (!selections.Contains(unit))
                    unit.EnableOutline(false);
            }

            mSelections = selections;
        }
    }
}