using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace BreadBowl.Bowling
{
    public class LaunchControlsUI : MonoBehaviour
    {
        [Tooltip("Leave empty to use the BreadLauncher on this same object")]
        [SerializeField] private BreadLauncher launcher;

        [Header("Lane Movement")]
        [SerializeField] private HoldButton moveLeftButton;
        [SerializeField] private HoldButton moveRightButton;

        [Header("Rotation")]
        [SerializeField] private HoldButton rotateUpButton;
        [SerializeField] private HoldButton rotateDownButton;
        [SerializeField] private HoldButton rotateLeftButton;
        [SerializeField] private HoldButton rotateRightButton;

        [Header("Launch")]
        [SerializeField] private HoldButton launchButton;
        [SerializeField] private RectTransform powerFill;
        [SerializeField] private CanvasGroup controlsGroup;

        private void Awake()
        {
            if (launcher == null)
            {
                launcher = GetComponent<BreadLauncher>();
            }

            if (launcher == null)
            {
                Debug.LogError("LaunchControlsUI: no BreadLauncher assigned", this);
            }

            // Buttons need an EventSystem, so add one if this level doesn't have it yet
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }
        }

        private void OnEnable()
        {
            launchButton.Pressed += OnLaunchPressed;
        }

        private void OnDisable()
        {
            launchButton.Pressed -= OnLaunchPressed;
        }

        private void Update()
        {
            if (launcher == null)
            {
                return;
            }

            if (!launcher.HasLaunched)
            {
                float move = Axis(moveLeftButton, moveRightButton);
                if (move != 0f)
                {
                    launcher.Move(move);
                }

                float yaw = Axis(rotateLeftButton, rotateRightButton);
                float tilt = Axis(rotateDownButton, rotateUpButton);
                if (yaw != 0f || tilt != 0f)
                {
                    launcher.Rotate(yaw, tilt);
                }
            }

            powerFill.anchorMax = new Vector2(launcher.Power, powerFill.anchorMax.y);
            controlsGroup.interactable = !launcher.HasLaunched;
        }

        private void OnLaunchPressed()
        {
            if (launcher != null)
            {
                launcher.Launch();
            }
        }

        private static float Axis(HoldButton negative, HoldButton positive)
        {
            return (positive.IsHeld ? 1f : 0f) - (negative.IsHeld ? 1f : 0f);
        }
    }
}
