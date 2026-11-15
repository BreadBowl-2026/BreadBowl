using BreadBowl.Bowling;
using BreadBowl.Levels;
using UnityEngine;

/// <summary>
/// Attach to the moving target's trigger collider.
/// MovingPlatform controls movement; this script ONLY awards two physical loaves.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class DoughMultiplierGate : LevelGoal
{
    [Header("Bonus dough placement")]
    [SerializeField, Min(0.1f)] private float spawnAhead = 0.65f;
    [SerializeField, Min(0.05f)] private float sideSpacing = 0.36f;
    [SerializeField, Min(0f)] private float outwardSpeed = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    private Loaf currentLoaf;
    private bool awardedThisRoll;
    private float spawnHeight;
    private readonly GameObject[] spawnedDough = new GameObject[2];
    private readonly Rigidbody[] spawnedBodies = new Rigidbody[2];

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    public override void OnRollStarted(Loaf loaf)
    {
        ClearBonusDough();
        currentLoaf = loaf;
        awardedThisRoll = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryMultiply(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryMultiply(other);
    }

    private void TryMultiply(Collider other)
    {
        if (awardedThisRoll || currentLoaf == null)
            return;

        Loaf touchedLoaf = other.GetComponentInParent<Loaf>();
        if (touchedLoaf == null && other.attachedRigidbody != null)
            touchedLoaf = other.attachedRigidbody.GetComponentInParent<Loaf>();

        // Only the dough launched this roll can earn the bonus.
        // Cloned dough cannot cause the gate to multiply repeatedly.
        if (touchedLoaf != currentLoaf || currentLoaf.Body == null)
            return;

        awardedThisRoll = true;
        Rigidbody originalBody = currentLoaf.Body;
        Vector3 velocity = originalBody.linearVelocity;
        Vector3 forward = Vector3.ProjectOnPlane(velocity, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        forward.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        spawnHeight = originalBody.position.y;
        int created = 0;

        for (int i = 0; i < 2; i++)
        {
            float side = i == 0 ? -1f : 1f;
            Vector3 spawnPosition = currentLoaf.transform.position
                + forward * spawnAhead
                + right * side * sideSpacing;

            Loaf copy = Instantiate(currentLoaf, spawnPosition, currentLoaf.transform.rotation);
            copy.gameObject.name = "Bonus Dough " + (i + 1);
            Rigidbody copyBody = copy.Body;
            if (copyBody == null)
            {
                Debug.LogError("[Bonus Gate] Bonus dough has no Rigidbody.", copy);
                Destroy(copy.gameObject);
                continue;
            }

            spawnedDough[i] = copy.gameObject;
            spawnedBodies[i] = copyBody;
            copyBody.isKinematic = false;
            copyBody.detectCollisions = true;
            copyBody.linearVelocity = velocity + right * side * outwardSpeed;
            copyBody.angularVelocity = originalBody.angularVelocity;
            copyBody.WakeUp();
            created++;
        }

        if (showDebugMessages)
            Debug.Log("[Bonus Gate] Spawned " + created + " bonus dough balls.", this);
    }


    public bool HasMovingBonusDough(float speedThreshold)
    {
        float speedSqr = speedThreshold * speedThreshold;
        foreach (Rigidbody body in spawnedBodies)
        {
            if (body == null || body.position.y < spawnHeight - 3f)
                continue;
            if (body.linearVelocity.sqrMagnitude >= speedSqr)
                return true;
        }
        return false;
    }

    public override bool IsComplete(Loaf loaf) => true;

    public override void ResetForNextRoll()
    {
        ClearBonusDough();
        currentLoaf = null;
        awardedThisRoll = false;
    }

    private void ClearBonusDough()
    {
        for (int i = 0; i < spawnedDough.Length; i++)
        {
            if (spawnedDough[i] != null)
                Destroy(spawnedDough[i]);
            spawnedDough[i] = null;
            spawnedBodies[i] = null;
        }
    }
}
