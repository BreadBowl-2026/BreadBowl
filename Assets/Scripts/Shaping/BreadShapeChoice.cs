using UnityEngine;
using UnityEngine.EventSystems;

namespace BreadBowl
{
    public class BreadShapeChoice : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private BreadShape shape;
        [SerializeField] private float liftHeight = 0.1f;

        private Vector3 restPosition;

        public BreadShape Shape => shape;

        public event System.Action<BreadShapeChoice> Clicked;

        private void Awake()
        {
            restPosition = transform.localPosition;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }

        public void SetSelected(bool selected)
        {
            transform.localPosition = selected 
                ? restPosition + Vector3.up * liftHeight 
                : restPosition;
        }
    }
}
