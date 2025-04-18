using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace K0.Gameplay
{
    public class WindZone : MonoBehaviour
    {
        public Vector3 WindDirection = Vector3.forward;
        public Ray Start;
        public Ray End;

        private Dictionary<InteractableItem, Sequence> interactableItems = new ();
        private void OnCollisionEnter(Collider other)
        {

            var item = other.gameObject.GetComponent<InteractableItem>();
            if (item && !interactableItems.ContainsKey(item))
            {
                var seq = DOTween.Sequence();
                interactableItems[item] = seq;
            }
        }

        private void OnCollisionExit(Collision other)
        {
            var item = other.gameObject.GetComponent<InteractableItem>();
            if (item && interactableItems.ContainsKey(item))
            {
                interactableItems.Remove(item);
            }
        }
    }
}