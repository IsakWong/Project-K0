using UnityEngine;

public class HUDBase : MonoBehaviour
{
    public Transform m2DCanvas;
    public Transform mTargetTransform;
    public Vector3 mTargetLocation = Vector3.zero;

    public Vector3 mTargetOffset = Vector3.zero;
    public bool mFaceCamera = false;

    public void Awake()
    {
    }

    public void Start()
    {
    }

    public void OnEnable()
    {
    }

    public void OnDisable()
    {
    }

    public void FixedUpdate()
    {
        if (mFaceCamera)
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }

        Vector3 targetLocation = mTargetLocation;
        if (mTargetTransform != null)
        {
            targetLocation = mTargetTransform.position;
        }

        targetLocation += mTargetOffset;
        if (m2DCanvas)
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, targetLocation);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m2DCanvas.transform as RectTransform, screenPoint,
                null, out Vector2 localPosition);
            RectTransform rect = transform as RectTransform;
            rect.anchoredPosition = localPosition;
        }
        else
            transform.position = targetLocation;
    }
}