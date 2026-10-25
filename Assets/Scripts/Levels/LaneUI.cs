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

        public event System.Action BowlPressed;
        public event System.Action BakePressed;

        private void Awake()
        {
            bowlButton.onClick.AddListener(() => BowlPressed?.Invoke());
            bakeButton.onClick.AddListener(() => BakePressed?.Invoke());
        }

        public void Show(int loafCount, int maxLoaves)
        {
            gameObject.SetActive(true);
            bowlButton.interactable = loafCount > 0;
            bakeButton.interactable = loafCount < maxLoaves;
            loafCountText.text = $"Loaves {loafCount}/{maxLoaves}";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

