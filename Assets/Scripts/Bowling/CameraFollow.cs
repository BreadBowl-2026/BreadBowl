using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 10f;

    private Vector3 offset;

    private void Start()
    {
        if (target != null)
        {
            SetTarget(target);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }
        
        // Follow position only, ignore the ball's rotation
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}