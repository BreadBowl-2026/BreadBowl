using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DoughSoundPlayer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] kneadingSounds;

    [Header("Timing")]
    [SerializeField] private float soundDuration = 0.5f;
    [SerializeField] private float soundGracePeriod = 0.1f;
    private float nextSoundTime;
    private float lastSoundTime;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.1f;
    [SerializeField] private float fadeOutDuration = 0.1f;
    [SerializeField] private float targetVolume = 1f;
    private Coroutine fadeCoroutine;

    private InputAction rotateCCWAction;
    private InputAction rotateCWAction;
    private InputAction palmAction;
    private InputAction foldGrabAction;
    private InputAction pressRow1Action;
    private InputAction pressRow2Action;
    private InputAction pressRow3Action;
    private InputAction pressRow4Action;


    private void Awake()
    {
        rotateCCWAction = InputSystem.actions.FindAction("Knead/RotateCCW", true);
        rotateCWAction = InputSystem.actions.FindAction("Knead/RotateCW", true);
        palmAction = InputSystem.actions.FindAction("Knead/Palm", true);
        foldGrabAction = InputSystem.actions.FindAction("Knead/FoldGrab", true);
        pressRow1Action = InputSystem.actions.FindAction("Knead/PressRow1", true);
        pressRow2Action = InputSystem.actions.FindAction("Knead/PressRow2", true);
        pressRow3Action = InputSystem.actions.FindAction("Knead/PressRow3", true);
        pressRow4Action = InputSystem.actions.FindAction("Knead/PressRow4", true);
    }

    private void Update()
    {
        if (IsKneading())
        {
            lastSoundTime = Time.time;
            RegisterKnead();
        }
        // not kneading = stop sound
        else if (Time.time - lastSoundTime > soundGracePeriod)
        {
            FadeOut();
        }
    }

    private bool IsKneading()
    {
        if (palmAction.IsPressed() || 
            pressRow1Action.IsPressed() || pressRow2Action.IsPressed() || 
            pressRow3Action.IsPressed() || pressRow4Action.IsPressed()
            )
            return true;
        return false;
    }

    private void RegisterKnead()
    {
        // can it play a new sound yet?
        if (Time.time < nextSoundTime) 
            return;
        
        // is sfx and kneading muted
        if (AudioManager.Instance == null || AudioManager.Instance.IsDoughMuted()
            )
            return;

        PlayRandomKneadingSound();

        // wait period the next sound can play
        nextSoundTime = Time.time + soundDuration;
    }

    private void PlayRandomKneadingSound()
    {
        if (kneadingSounds == null || kneadingSounds.Length == 0 ||
            audioSource == null)
            return;

        AudioClip clip = kneadingSounds[
            Random.Range(0, kneadingSounds.Length)
        ];

        //// Cancel an existing fade.
        //if (fadeCoroutine != null)
        //    StopCoroutine(fadeCoroutine);

        audioSource.clip = clip;
        audioSource.volume = 0f;
        audioSource.Play();

        // fade in
        fadeCoroutine = StartCoroutine(
            FadeIn(targetVolume, fadeInDuration) // fade in 
        );
    }

    // ----------
    // Fade in
    // ----------
    private IEnumerator FadeIn(float target, float duration)
    {
        float startingVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            audioSource.volume = Mathf.Lerp(
                startingVolume,
                target,
                t
            );

            yield return null;
        }

        audioSource.volume = target;

        fadeCoroutine = null;
    }

    // ----------
    // Fade out
    // ----------
    private void FadeOut()
    {
        if (audioSource == null || !audioSource.isPlaying)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(
            FadeOutCoroutine()
        );
    }
    private IEnumerator FadeOutCoroutine()
    {
        float startingVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / fadeOutDuration;

            audioSource.volume = Mathf.Lerp(
                startingVolume,
                0f,
                t
            );

            yield return null;
        }

        audioSource.volume = 0f; // set to 0 for fade in
        audioSource.Stop();

        fadeCoroutine = null;
    }

    

    //public void StopKneadingSound()
    //{
    //    if (audioSource != null && audioSource.isPlaying)
    //        audioSource.Stop();
    //}
}
