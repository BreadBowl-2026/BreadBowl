using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float kneadingVolume = 1f;

    [Header("Mute States")]
    private bool masterMuted;
    private bool musicMuted;
    private bool sfxMuted;
    private bool kneadingMuted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        ApplyAllVolumes();
    }

    // -------------------------
    // Master Volume
    // -------------------------
    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        ApplyAllVolumes();
    }
    public void SetMasterMuted(bool muted)
    {
        masterMuted = !muted;
        ApplyAllVolumes();
    }

    // -------------------------
    // Music Volume
    // -------------------------
    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        ApplyAllVolumes();
    }
    public void SetMusicMuted(bool muted)
    {
        musicMuted = !muted;
        ApplyAllVolumes();
    }

    // -------------------------
    // SFX Volume
    // -------------------------
    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        ApplyAllVolumes();
    }
    public void SetSFXMuted(bool muted)
    {
        sfxMuted = !muted;
        ApplyAllVolumes();
    }
    public bool IsSFXMuted()
    {
        return masterMuted || sfxMuted;
    }

    // -------------------------
    // Kneading Volume
    // -------------------------
    public void SetKneadingVolume(float value)
    {
        kneadingVolume = value;
        ApplyAllVolumes();
    }
    public void SetKneadingMuted(bool muted)
    {
        kneadingMuted = !muted;
        ApplyAllVolumes();
    }
    public bool IsKneadingMuted()
    {
        return masterMuted || sfxMuted || kneadingMuted;
    }

    // -------------------------
    // Apply Global Volume
    // -------------------------
    private void ApplyAllVolumes()
    {
        // Master mute affects all audio.
        AudioListener.volume = masterMuted ? 0f : masterVolume;

        if (musicSource != null)
        {
            musicSource.volume = musicMuted ? 0f : musicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxMuted ? 0f : sfxVolume;
        }
    }
}
