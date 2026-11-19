using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BreadBowl.Levels
{
    /// <summary>
    /// The level complete screen on ResultCanvas in the Level prefab.
    /// LevelController calls Show, which builds one ResultRow per line in
    /// LevelResult.Stats inside the scrolling stats container.
    /// To add lines for a level, don't edit this class. Implement
    /// IResultReporter instead.
    /// </summary>
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private RectTransform statsContainer;
        [SerializeField] private ResultRow rowPrefab;
        [SerializeField] private Button continueButton;

        public event System.Action ContinuePressed;

        private void Awake()
        {
            continueButton.onClick.AddListener(() => ContinuePressed?.Invoke());
        }

        public void Show(string title, LevelResult result)
        {
            gameObject.SetActive(true);
            titleText.text = title;

            foreach (Transform child in statsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (ResultStat stat in result.Stats)
            {
                Instantiate(rowPrefab, statsContainer).Show(stat);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
