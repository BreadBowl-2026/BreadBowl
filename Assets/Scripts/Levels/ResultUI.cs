using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BreadBowl.Levels
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private Button continueButton;

        public event System.Action ContinuePressed;

        private void Awake()
        {
            continueButton.onClick.AddListener(() => ContinuePressed?.Invoke());
        }

        public void Show(string title, LevelResult result)
        {
            gameObject.SetActive(true);

            int minutes = Mathf.FloorToInt(result.Time / 60f);
            int seconds = Mathf.FloorToInt(result.Time % 60f);

            titleText.text = title;
            statsText.text = $"Loaves bowled: {result.LoavesBowled}\nTime: {minutes:00}:{seconds:00}";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
