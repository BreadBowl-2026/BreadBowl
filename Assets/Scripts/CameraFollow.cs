using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 10f;

    private Vector3 offset;

    private void Start()
    {
        // Remember the starting distance between camera and ball
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        // Follow position only, ignore the ball's rotation
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}