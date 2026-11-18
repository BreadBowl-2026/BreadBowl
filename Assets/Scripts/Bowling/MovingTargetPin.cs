using System.Collections;
using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovingTargetPin : MonoBehaviour
    {
        [Header("Movement (world Z is sideways in this lane)")]
        [SerializeField, Min(0f)] private float moveDistance = 0.25f;
        [SerializeField, Min(0f)] private float moveSpeed = 1.2f;
        [SerializeField] private float phaseOffset;

        [Header("Knockdown")]
        [SerializeField, Min(0f)] private float hitImpulse = 0.4f;
        [SerializeField, Min(0f)] private float tipTorque = 1f;
        [SerializeField, Min(0f)] private float freezeDelay = 1f;

        public bool IsDown { get; private set; }
        public bool IsSettled { get; private set; }

        private Rigidbody body;
        private Vector3 startPosition;
        private Quaternion startRotation;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            startPosition = body.position;
            startRotation = body.rotation;
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            if (IsDown) return;
            float offset = Mathf.Sin(Time.fixedTime * moveSpeed + phaseOffset) * moveDistance;
            body.MovePosition(startPosition + Vector3.forward * offset);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsDown) return;

            Loaf loaf = collision.collider.GetComponentInParent<Loaf>();
            MovingTargetPin other = collision.collider.GetComponentInParent<MovingTargetPin>();
            bool hitByDough = loaf != null;
            bool hitByFallingPin = other != null && other != this && other.IsDown && !other.IsSettled;
            if (!hitByDough && !hitByFallingPin) return;

            IsDown = true;
            body.isKinematic = false;
            body.useGravity = true;
            body.linearDamping = 3f;

            Vector3 push = Vector3.ProjectOnPlane(body.position - collision.transform.position, Vector3.up);
            if (push.sqrMagnitude < 0.001f)
                push = Vector3.ProjectOnPlane(collision.relativeVelocity, Vector3.up);
            if (push.sqrMagnitude < 0.001f) push = Vector3.forward;
            push.Normalize();

            body.AddForce(push * hitImpulse, ForceMode.Impulse);
            body.AddTorque(Vector3.Cross(Vector3.up, push) * tipTorque, ForceMode.Impulse);
            StartCoroutine(FreezeAfterFall());
        }

        private IEnumerator FreezeAfterFall()
        {
            yield return new WaitForSeconds(freezeDelay);
            if (!IsDown || body.isKinematic) yield break;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            body.useGravity = false;
            IsSettled = true;
        }

        public void ResetForNewAttempt()
        {
            StopAllCoroutines();
            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            body.isKinematic = true;
            body.useGravity = false;
            body.position = startPosition;
            body.rotation = startRotation;
            IsDown = false;
            IsSettled = false;
        }
    }
}
