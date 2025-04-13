using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSmashController : MonoBehaviour
{
    // Start is called before the first frame update
    public float MoveSpeed = 10.0f;
    public float OriginMoveSpeed = 10.0f;
    public Animator animator;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        animator.speed = OriginMoveSpeed / MoveSpeed;
    }
}