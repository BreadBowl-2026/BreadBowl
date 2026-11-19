using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BreadBowl.Bowling
{
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private AudioClip pressedSound;

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

            AudioManager.Instance?.PlayContinuousSound(pressedSound);
            IsHeld = true;
            Pressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            AudioManager.Instance?.StopContinuousSound();
            IsHeld = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            AudioManager.Instance?.StopContinuousSound();
            IsHeld = false;
        }
    }
}
