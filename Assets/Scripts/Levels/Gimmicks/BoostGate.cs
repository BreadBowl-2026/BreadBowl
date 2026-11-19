using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    [RequireComponent(typeof(BoxCollider))]
    public class BoostGate : MonoBehaviour
    {
        [SerializeField] private float boost = 50f;

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;

            if (body == null || body.GetComponent<Loaf>() == null)
            {
                return;
            }

            Vector3 direction = body.linearVelocity.sqrMagnitude > 0.01f
                ? body.linearVelocity.normalized
                : transform.forward;

            body.AddForce(direction * boost, ForceMode.VelocityChange);
        }
    }
}
