
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SlidingRollingPin : MonoBehaviour
{
    [SerializeField] private float moveDistance = 0.5f;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float phaseOffset = 0f;

    private Rigidbody rb;
    private Vector3 startPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        startPosition = rb.position;
    }

    private void FixedUpdate()
    {
        float offset = Mathf.Sin(
            Time.fixedTime * moveSpeed + phaseOffset
        ) * moveDistance;

        rb.MovePosition(
            startPosition + Vector3.forward * offset
        );
    }
}
