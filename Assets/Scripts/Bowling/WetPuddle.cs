using System.Collections.Generic;
using BreadBowl.Bowling;
using UnityEngine;

public class WetPuddle : MonoBehaviour
{
    [SerializeField] private float sidewaysAcceleration = 2f;

    private readonly HashSet<Rigidbody> pushedThisStep = new();

    private void FixedUpdate() => pushedThisStep.Clear();

    private void OnTriggerStay(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;

        if (body == null || body.isKinematic || body.linearVelocity.x < 0.1f ||
            body.GetComponentInParent<Loaf>() == null || !pushedThisStep.Add(body))
            return;

        body.AddForce(Vector3.back * sidewaysAcceleration, ForceMode.Acceleration);
    }
}