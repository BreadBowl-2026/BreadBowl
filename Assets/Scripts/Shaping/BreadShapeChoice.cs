using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace BreadBowl
{
    public class BreadShapeChoice : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private BreadShape shape;

        [SerializeField] private GameObject tin;
        [SerializeField] private Light spotLight;
        [SerializeField] private TMP_Text hoverText;

        [Header("Selection Effects")]
        [SerializeField] private float liftHeight = 0.1f;
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color unselectedColor = Color.gray;

        private Vector3 restPosition;

        public BreadShape Shape => shape;

        public event System.Action<BreadShapeChoice> Clicked;


        private void Awake()
        {
            restPosition = tin.transform.localPosition;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }

        public void SetSelected(bool selected)
        {
            // Lift tin
            tin.transform.localPosition = selected 
                ? restPosition + Vector3.up * liftHeight 
                : restPosition;

            // Toggle spotlight
            spotLight.enabled = selected;

            // Change hover text color
            hoverText.color = selected 
                ? selectedColor 
                : unselectedColor;
        }
    }
}
