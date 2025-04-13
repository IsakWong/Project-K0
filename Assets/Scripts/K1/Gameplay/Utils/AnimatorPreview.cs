using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class AnimatorPreview : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private Animator animator;

    // Update is called once per frame
    void Update()
    {
        animator.Update(Time.deltaTime);
    }
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (UnityEngine.Event.current.type == EventType.Repaint)
            SceneView.RepaintAll();
    }
#endif
}