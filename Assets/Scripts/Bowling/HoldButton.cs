using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BreadBowl.Bowling
{
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Selectable selectable;

        public bool IsHeld { get; private set; }

        public event Action Pressed;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
        }

        private void OnDisable()
        {
            IsHeld = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (selectable != null && !selectable.IsInteractable())
            {
                return;
            }

            IsHeld = true;
            Pressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsHeld = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsHeld = false;
        }
    }
}
