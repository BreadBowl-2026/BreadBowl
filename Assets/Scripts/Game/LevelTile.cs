using System;
using TMPro;
using BreadBowl.Levels;
using UnityEngine;
using UnityEngine.UI;

public class LevelTile : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text levelLabel;
    [SerializeField] private TMP_Text statusLabel;

    private static readonly Color Normal   = new Color(0.988f, 0.969f, 0.929f); 
    private static readonly Color Selected = new Color(0.894f, 0.714f, 0.475f);
    private static readonly Color Locked   = new Color(0.72f, 0.66f, 0.58f);

    private bool playable;

    public void Initialize(int index, LevelInfo info, Action<int> onClick)
    {
        string number = "Level " + (index + 1).ToString("00");
        playable = info != null && info.IsPlayable;

        levelLabel.text = info != null ? info.DisplayName : number;
        statusLabel.text = playable ? number : "COMING SOON";
        background.color = playable ? Normal : Locked;
        button.interactable = playable;
        button.onClick.AddListener(() => onClick(index));
    }

    public void SetSelected(bool on)
    {
        if (playable) background.color = on ? Selected : Normal;
    }
}