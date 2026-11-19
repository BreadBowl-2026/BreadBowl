using System.Collections.Generic;
using BreadBowl.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] private RectTransform gridContent;
    [SerializeField] private LevelTile levelTilePrefab;
    [SerializeField] private LevelInfo[] levels;
    [SerializeField] private int totalLevels = 13;

    [SerializeField] private GameObject emptySelectionMessage;
    [SerializeField] private GameObject detailsContent;
    [SerializeField] private TMP_Text detailsTitle;
    [SerializeField] private TMP_Text detailsDescription;
    [SerializeField] private Image detailsScreenshot;
    [SerializeField] private TMP_Text levelCountLabel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button backButton;

    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string fallbackDescription = "Your next baking challenge awaits.";

    private readonly List<LevelTile> tiles = new List<LevelTile>();
    private LevelInfo selected;

    private void Start()
    {
        detailsContent.SetActive(false);
        emptySelectionMessage.SetActive(true);
        startButton.onClick.AddListener(() => SceneManager.LoadScene(selected.SceneName));
        backButton.onClick.AddListener(() => SceneManager.LoadScene(mainMenuSceneName));

        int count = Mathf.Max(totalLevels, levels.Length);
        int available = 0;

        for (int i = 0; i < count; i++)
        {
            LevelInfo info = i < levels.Length
                ? levels[i]
                : null;
            if (info != null && info.IsPlayable)
            {
                available++;
            } 

            LevelTile tile = Instantiate(levelTilePrefab, gridContent);
            tile.Initialize(i, info, Select);
            tiles.Add(tile);
        }

        levelCountLabel.text = available + " AVAILABLE  /  " + totalLevels + " TOTAL";
    }

    private void Select(int index)
    {
        selected = levels[index];
        emptySelectionMessage.SetActive(false);
        detailsContent.SetActive(true);

        detailsTitle.text = selected.DisplayName;
        detailsDescription.text = string.IsNullOrEmpty(selected.Description)
            ? fallbackDescription
            : selected.Description;
        detailsScreenshot.sprite = selected.Screenshot;
        detailsScreenshot.enabled = selected.Screenshot != null;

        for (int i = 0; i < tiles.Count; i++) 
        {
            tiles[i].SetSelected(i == index);
        }
    }
}