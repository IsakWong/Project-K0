using System;
using DG.Tweening;
using UnityEngine;

namespace K0.Env
{
    public class EnvMove : MonoBehaviour
    {
        
        public Vector3 MoveDelta = Vector3.up;
        public Vector3 MoveDirection = Vector3.forward;
        public bool IsLocal = false;
        public bool Rollback = false;
        public int Loops;
        public float MoveTime = 5.0f;
        public Ease EaseType = Ease.InOutQuart;
        
        private Sequence sequence;

        private void Start()
        {
            sequence = sequence = DOTween.Sequence();
            var FinalPosition = transform.position;
            var StartPosition = transform.position;
            if(IsLocal)
                FinalPosition = FinalPosition + transform.TransformPoint(MoveDelta); 
            else
                FinalPosition = FinalPosition + MoveDelta;
            sequence.Append(transform.DOMove(FinalPosition, MoveTime).SetEase(EaseType));
            if(Rollback)
                sequence.Append(transform.DOMove(StartPosition, MoveTime).SetEase(EaseType));
            sequence.SetLoops(Loops, LoopType.Restart);
            sequence.Play();
        }
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            
        }
#endif
    }
}