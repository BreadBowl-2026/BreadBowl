using BreadBowl.Levels;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreadBowl
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private string mainMenuScene = "MainMenu";

        public static GameController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void LoadLevel(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(mainMenuScene);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LevelController level = FindFirstObjectByType<LevelController>();

            if (level != null)
            {
                level.Finished += HandleLevelFinished;
            }
        }

        private void HandleLevelFinished(LevelResult result)
        {
            Debug.Log($"Level finished: {result.Outcome}, {result.LoavesBowled} loaves, {result.Time:F0}s", this);
            LoadMainMenu();
        }
    }
}


