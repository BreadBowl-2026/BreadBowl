using UnityEngine;

public class ButtonSoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    public void PlayButtonSound()
    {
        if (audioSource == null || buttonSound == null)
            return;

        if (AudioManager.Instance != null &&
            AudioManager.Instance.IsSFXMuted())
        {
            return;
        }

        audioSource.PlayOneShot(buttonSound);
    }
}
