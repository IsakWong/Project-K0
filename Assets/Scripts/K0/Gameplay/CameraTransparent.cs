using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace K0.Gameplay
{
    public class CameraTransparent : MonoBehaviour
    {
        public Transform Target;
        private HashSet<MeshRenderer> preCollide = new();
        
        private void FixedUpdate()
        {
            Ray ray = new Ray();
            ray.origin = transform.position;
            ray.direction = (Target.position - transform.position).normalized;
            
            var raycast = Physics.RaycastAll(ray, (transform.position - Target.position).magnitude,
                1 << LayerMask.NameToLayer("Env"), QueryTriggerInteraction.Ignore);
            HashSet<MeshRenderer> collide = new HashSet<MeshRenderer>();
            foreach (var hit in raycast)
            {
                var mesh = hit.collider.gameObject.GetComponent<MeshRenderer>();
                if (!collide.Contains(mesh))
                {
                    collide.Add(mesh);
                    DOTween.To(() => mesh.material.GetFloat("_Alpha"), v =>
                    {
                        mesh.material.SetFloat("_Alpha", v);
                    }, 0, 0.5f).SetEase(Ease.OutSine);
                }
            }

            foreach (var mesh in preCollide)
            {
                if (!collide.Contains(mesh))
                {
                    DOTween.To(() => mesh.material.GetFloat("_Alpha"), v =>
                    {
                        mesh.material.SetFloat("_Alpha", v);
                    }, 1.0f, 0.5f).SetEase(Ease.OutSine);
                }
            }
            preCollide = collide;
        }
    }
}