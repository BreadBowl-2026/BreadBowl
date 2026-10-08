using UnityEngine;

public class KneadAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] kneadingSounds;

    [Header("Timing")]
    [SerializeField] private float minimumInterval = 0.8f;
    [SerializeField] private float maximumInterval = 1.2f;

    private float nextSoundTime;

    public void RegisterKnead()
    {
        if (Time.time < nextSoundTime)
            return;

        PlayRandomKneadingSound();

        nextSoundTime = Time.time +
            Random.Range(minimumInterval, maximumInterval);
    }

    private void PlayRandomKneadingSound()
    {
        if (kneadingSounds == null || kneadingSounds.Length == 0)
            return;

        AudioClip clip = kneadingSounds[
            Random.Range(0, kneadingSounds.Length)
        ];

        audioSource.PlayOneShot(clip);
    }
}
