using UnityEngine;
using UnityEngine.UI;

namespace BreadBowl
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private SoundMenu soundMenu;

        private void Awake()
        {
            settingsButton.onClick.AddListener(OpenSettings);
            quitButton.onClick.AddListener(QuitGame);
        }

        private void Start()
        {
            settingsPanel.SetActive(false);
        }

        public void OpenSettings()
        {
            settingsPanel.SetActive(true);
            soundMenu.OpenMenu();
        }

        public void CloseSettings()
        {
            soundMenu.CloseMenu();
            settingsPanel.SetActive(false);
        }

        public void QuitGame()
        {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        }
    }
}