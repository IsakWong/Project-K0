﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Obi.Samples
{
    [RequireComponent(typeof(ObiSolver))]
    public class CollisionEventHandler : MonoBehaviour
    {

        ObiSolver solver;
        public int contactCount;

        ObiNativeContactList frame;

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
            frame = e;
        }

        void OnDrawGizmos()
        {
            if (solver == null || frame == null) return;

            Gizmos.matrix = solver.transform.localToWorldMatrix;

            contactCount = frame.count;

            for (int i = 0; i < frame.count; ++i)
            {
                var contact = frame[i];

                //if (contact.distance > 0.001f) continue;

                Gizmos.color = (contact.distance <= 0) ? Color.red : Color.green;

                //Gizmos.color = new Color(((i * 100) % 255) / 255.0f, ((i * 50) % 255) / 255.0f, ((i * 20) % 255) / 255.0f);

                Vector3 point = frame[i].pointB;

                Gizmos.DrawSphere(point, 0.01f);

                Gizmos.DrawRay(point, contact.normal * contact.distance);

                Gizmos.color = Color.cyan;
                //Gizmos.DrawRay(point, contact.tangent * contact.tangentImpulse + contact.bitangent * contact.bitangentImpulse);

            }

            /*for (int i = 0; i < frame.count; ++i)
            {
                var contact = frame[i];

                //if (contact.distance > 0.001f) continue;

                Gizmos.color = (contact.distance <= 0) ? Color.red : Color.green;

                //Gizmos.color = new Color(((i * 100) % 255) / 255.0f, ((i * 50) % 255) / 255.0f, ((i * 20) % 255) / 255.0f);

                Vector3 point = Vector3.zero;//frame.contacts.Data[i].point;

                int simplexStart = solver.simplexCounts.GetSimplexStartAndSize(contact.bodyB, out int simplexSize);

                float radius = 0;
                for (int j = 0; j < simplexSize; ++j)
                {
                    point += (Vector3)solver.positions[solver.simplices[simplexStart + j]] * contact.pointB[j];
                    radius += solver.principalRadii[solver.simplices[simplexStart + j]].x * contact.pointB[j];
                }

                Vector3 normal = contact.normal;

                //Gizmos.DrawSphere(point + normal.normalized * frame.contacts[i].distance, 0.01f);

                Gizmos.DrawSphere(point + normal * radius, 0.01f);

                Gizmos.DrawRay(point + normal * radius, normal.normalized * contact.distance);
            }*/
        }

    }
}
