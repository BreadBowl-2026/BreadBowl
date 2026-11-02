
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace BreadBowl
{
    public class BreadBowlPauseMenu : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private GameObject pausePanel;
        private SoundMenu soundMenu;
        private bool isPaused;

        public bool IsPaused => isPaused;

        private void Awake()
        {
            pausePanel = transform.Find("PausePanel").gameObject;
            soundMenu = GetComponentInChildren<SoundMenu>(true);

            Bind("PauseButton", TogglePause);
            Bind("PausePanel/Window/ResumeButton", Resume);
            Bind("PausePanel/Window/SoundButton", OpenSound);
            Bind("PausePanel/Window/MainMenuButton", MainMenu);
            Bind("PausePanel/Window/QuitButton", Quit);

            if (soundMenu != null) soundMenu.CloseMenu();
            pausePanel.SetActive(false);
        }

        private void Bind(string path, UnityEngine.Events.UnityAction action)
        {
            transform.Find(path).GetComponent<Button>()
                .onClick.AddListener(action);
        }

        private void Update()
        {
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
                HandleEscape();
        }

        private void HandleEscape()
        {
            if (isPaused && soundMenu != null &&
                soundMenu.gameObject.activeSelf)
                soundMenu.CloseMenu();
            else
                TogglePause();
        }

        public void TogglePause()
        {
            if (isPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            isPaused = true;
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            if (soundMenu != null) soundMenu.CloseMenu();

            isPaused = false;
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void OpenSound()
        {
            if (soundMenu != null)
                soundMenu.OpenMenu();
            else
                Debug.LogWarning("SoundMenu not found!");
        }

        public void MainMenu()
        {
            Resume();
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void Quit()
        {
            Resume();

            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #else
                        Application.Quit();
            #endif
        }

        private void OnDestroy()
        {
            if (isPaused) Time.timeScale = 1f;
        }
    }
}
