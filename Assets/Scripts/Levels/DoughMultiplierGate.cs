using System.Collections.Generic;
using BreadBowl.Bowling;
using BreadBowl.Levels;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DoughMultiplierGate : LevelGoal
{
    private const int BonusLoafCount = 2;

    [Header("Bonus dough placement")]
   
    [SerializeField, Min(0.1f)] private float spawnAhead = 0.65f;
    [SerializeField, Min(0.05f)] private float sideSpacing = 0.36f;
    [SerializeField, Min(0f)] private float outwardSpeed = 0.3f;

    private readonly List<Loaf> bonusLoaves = new List<Loaf>(BonusLoafCount);
    private Loaf pendingLoaf;
    private float spawnHeight;

    private void Awake() => GetComponent<BoxCollider>().isTrigger = true;
    public override void OnRollStarted(Loaf loaf) => ResetBonus(loaf);
    public override void ResetForNextRoll() => ResetBonus(null);
    public override bool IsComplete(Loaf loaf) => true;
    private void OnTriggerEnter(Collider other) => TryMultiply(other);
    private void OnTriggerStay(Collider other) => TryMultiply(other);
    private void TryMultiply(Collider other)
    {
        Loaf loaf = pendingLoaf;
        if (loaf == null || other.GetComponentInParent<Loaf>() != loaf)
            return;

        Rigidbody originalBody = loaf.Body;
        if (originalBody == null)
            return;

        pendingLoaf = null;
        spawnHeight = originalBody.position.y;

        Vector3 velocity = originalBody.linearVelocity;
        Vector3 forward = Vector3.ProjectOnPlane(velocity, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        forward.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 spawnCenter = loaf.transform.position + forward * spawnAhead;

        for (int i = 0; i < BonusLoafCount; i++)
        {
            Vector3 side = i == 0 ? -right : right;
            Vector3 spawnPosition = spawnCenter + side * sideSpacing;
            Loaf copy = Instantiate(loaf, spawnPosition, loaf.transform.rotation);
            copy.gameObject.name = $"Bonus Dough {i + 1}";

            Rigidbody body = copy.Body;
            if (body == null)
            {
                Debug.LogError("[Bonus Gate] Bonus dough has no Rigidbody.", copy);
                Destroy(copy.gameObject);
                continue;
            }

            bonusLoaves.Add(copy);
            body.isKinematic = false;
            body.detectCollisions = true;
            body.linearVelocity = velocity + side * outwardSpeed;
            body.angularVelocity = originalBody.angularVelocity;
            body.WakeUp();
        }
    }

    public bool HasMovingBonusDough(float speedThreshold)
    {
        float speedSquared = speedThreshold * speedThreshold;
        foreach (Loaf loaf in bonusLoaves)
        {
            if (loaf == null)
                continue;

            Rigidbody body = loaf.Body;
            if (body == null || body.position.y < spawnHeight - 3f)
                continue;

            if (body.linearVelocity.sqrMagnitude >= speedSquared)
                return true;
        }

        return false;
    }

    private void ResetBonus(Loaf nextLoaf)
    {
        foreach (Loaf loaf in bonusLoaves)
        {
            if (loaf != null)
                Destroy(loaf.gameObject);
        }

        bonusLoaves.Clear();
        pendingLoaf = nextLoaf;
    }
}
