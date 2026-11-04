using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] private RectTransform gridContent;
    [SerializeField] private LevelTile levelTilePrefab;
    [SerializeField] private int totalLevels = 13;

    [SerializeField] private GameObject emptySelectionMessage;
    [SerializeField] private GameObject detailsContent;
    [SerializeField] private TMP_Text detailsTitle;
    [SerializeField] private TMP_Text detailsDescription;
    [SerializeField] private TMP_Text levelCountLabel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button backButton;

    [SerializeField] private string sceneNamePrefix = "Level_";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [SerializeField, TextArea(2, 4)] private string[] descriptions;

    private readonly List<LevelTile> tiles = new List<LevelTile>();
    private int selected;

    private void Start()
    {
        detailsContent.SetActive(false);
        emptySelectionMessage.SetActive(true);
        startButton.onClick.AddListener(() => SceneManager.LoadScene(SceneName(selected)));
        backButton.onClick.AddListener(() => SceneManager.LoadScene(mainMenuSceneName));

        int available = 0;
        for (int n = 1; n <= totalLevels; n++)
        {
            bool playable = Application.CanStreamedLevelBeLoaded(SceneName(n));
            if (playable) available++;

            LevelTile tile = Instantiate(levelTilePrefab, gridContent);
            tile.Initialize(n, playable, Select);
            tiles.Add(tile);
        }

        levelCountLabel.text = available + " AVAILABLE  /  " + totalLevels + " TOTAL";
    }

    private string SceneName(int n) => sceneNamePrefix + n.ToString("00");

    private void Select(int n)
    {
        selected = n;
        emptySelectionMessage.SetActive(false);
        detailsContent.SetActive(true);

        detailsTitle.text = "LEVEL " + n.ToString("00");
        detailsDescription.text = n <= descriptions.Length && !string.IsNullOrEmpty(descriptions[n - 1])
            ? descriptions[n - 1]
            : "Your next baking challenge awaits.";

        for (int i = 0; i < tiles.Count; i++) tiles[i].SetSelected(i + 1 == n);
    }
}