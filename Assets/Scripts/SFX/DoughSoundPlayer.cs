using UnityEngine;
using UnityEngine.InputSystem;

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
            StopKneadingSound();
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

        // randomize a wait period the next sound can play
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

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopKneadingSound()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}
