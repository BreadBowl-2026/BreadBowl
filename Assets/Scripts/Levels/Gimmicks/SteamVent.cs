
using UnityEngine;
using System.Collections;

public class SteamVent : MonoBehaviour
{
    [Header("Steam Settings")]
    public float interval = 3f;
    public float activeDuration = 1f;
    public float launchForce = 12f;

    private bool isActive = false;
    private bool hasLaunched = false;

    [SerializeField] private ParticleSystem steamEffect;

    private void Start()
    {
        StartCoroutine(SteamCycle());
    }

    private IEnumerator SteamCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            isActive = true;
            hasLaunched = false;

            steamEffect.Play();

            yield return new WaitForSeconds(activeDuration);

            isActive = false;
            steamEffect.Stop();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActive || hasLaunched)
            return;

        Rigidbody breadRb = other.attachedRigidbody;
        if (breadRb != null)
        {
            breadRb.AddForce(
                Vector3.up * launchForce,
                ForceMode.Impulse
            );
            hasLaunched = true;
        }
    }
}
