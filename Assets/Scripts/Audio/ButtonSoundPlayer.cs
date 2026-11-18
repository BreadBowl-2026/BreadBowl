using UnityEngine;

public class ButtonSoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip buttonSound;

    public void PlayButtonSound()
    {
        if (buttonSound == null)
        {
            Debug.Log("No button sound assigned.");
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.Log("AudioManager instance not found.");
            return;
        }

        AudioManager.Instance.PlaySFX(buttonSound);
    }
}
