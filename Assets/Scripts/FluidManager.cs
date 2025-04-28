using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

[RequireComponent(typeof(ObiSolver))]
public class FluidManager : MonoBehaviour
{
    public static FluidManager Instance;
    public GameObject SplashPrefab;
    private List<GameObject> SplashPool = new List<GameObject>();
    public ObiSolver Solver;
    // Start is called before the first frame update
    private void Solver_OnCollision(ObiSolver solver, ObiNativeContactList contacts)
    {
        var world = ObiColliderWorld.GetInstance();
        foreach (var contact in contacts)
        {
            int startA = solver.simplexCounts.GetSimplexStartAndSize(contact.bodyA, out _);
            // retrieve the index of both particles from the simplices array:
            int particleA = solver.simplices[startA];
            

            // retrieve info about both actors involved in the collision:
            var particleInActorA = solver.particleToActor[particleA];
            var layer = LayerMask.NameToLayer("Fluid");
            if(particleInActorA == null || particleInActorA.actor.gameObject.layer != layer)
                continue;
            
            if (contact.distance < 0.01)
            {
                ObiColliderBase col = world.colliderHandles[contact.bodyB].owner;
                if (col.gameObject.layer == LayerMask.NameToLayer("WaterInteractable"))
                {
                    var hole = col.gameObject.GetComponent<WaterReceiver>();
                    hole.ReceiveWater(1.0f);
                   
                }
                
                if (col.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    // do something with the particle, for instance get its position:
                    var position = solver.transform.localToWorldMatrix.MultiplyPoint3x4(contact.pointB);
                    Vector3 pos = position;
                    bool found = false;
                    foreach (var it in SplashPool)
                    {
                        if ((pos - it.transform.position).magnitude < 1.0f)
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        var newSplash = GameObject.Instantiate(SplashPrefab);
                        SplashPool.Add(newSplash);
                        newSplash.transform.position = pos;    
                    }
                    
                }

            }
        }
    }
    void Awake()
    {
        Solver = GetComponent<ObiSolver>();
        Instance = this;
        Solver.OnCollision += Solver_OnCollision;
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
