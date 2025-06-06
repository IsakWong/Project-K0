﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi.Samples
{
    [RequireComponent(typeof(ObiSolver))]
    public class ObiParticleCounter : MonoBehaviour
    {

        ObiSolver solver;
        public int counter = 0;
        public Collider2D targetCollider = null;

        ObiNativeContactList frame;
        HashSet<int> particles = new HashSet<int>();

        void Awake()
        {
            solver = GetComponent<Obi.ObiSolver>();
        }

        void OnEnable()
        {
            solver.OnCollision += Solver_OnCollision;
        }

        void OnDisable()
        {
            solver.OnCollision -= Solver_OnCollision;
        }

        void Solver_OnCollision(object sender, ObiNativeContactList e)
        {
            HashSet<int> currentParticles = new HashSet<int>();

            for (int i = 0; i < e.count; ++i)
            {
                if (e[i].distance < 0.001f)
                {

                    /*Component collider;
                    if (ObiCollider2D.idToCollider.TryGetValue(e.contacts.Data[i].other,out collider)){

                        if (collider == targetCollider)
                            currentParticles.Add(e.contacts.Data[i].particle);

                    }*/
                }
            }

            particles.ExceptWith(currentParticles);
            counter += particles.Count;
            particles = currentParticles; Debug.Log(counter);
        }

    }
}
