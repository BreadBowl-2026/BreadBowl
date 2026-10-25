using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BreadBowl.Levels
{
    public class LaneUI : MonoBehaviour
    {
        [SerializeField] private Button bowlButton;
        [SerializeField] private Button bakeButton;
        [SerializeField] private TMP_Text loafCountText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button menuButton;

        public event System.Action MenuPressed;
        public event System.Action BowlPressed;
        public event System.Action BakePressed;

        private void Awake()
        {
            bowlButton.onClick.AddListener(() => BowlPressed?.Invoke());
            bakeButton.onClick.AddListener(() => BakePressed?.Invoke());
            menuButton.onClick.AddListener(() => MenuPressed?.Invoke());
        }

        public void Show(int loafCount, int maxLoaves, string message)
        {
            gameObject.SetActive(true);
            bowlButton.interactable = loafCount > 0;
            bakeButton.interactable = loafCount < maxLoaves;
            loafCountText.text = $"Loaves {loafCount}/{maxLoaves}";
            messageText.text = message;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

