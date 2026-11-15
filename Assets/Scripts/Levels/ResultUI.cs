using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BreadBowl.Levels
{
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
