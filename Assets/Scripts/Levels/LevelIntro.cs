using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BreadBowl.Levels
{
    /// <summary>
    /// A short fly-over of the level that plays once when the level loads,
    /// before the player starts baking. Each child IntroShot is one camera
    /// move; shots play in hierarchy order with a hard cut between them.
    /// Duplicate a shot to add one, drag shots in the Hierarchy to reorder.
    /// LevelController finds this in the scene, calls Play from its Start,
    /// and starts the level when Finished fires. Baking more loaves never
    /// replays it. Untick this GameObject to skip the intro while testing.
    /// </summary>
    public class LevelIntro : MonoBehaviour
    {
        [Tooltip("Camera to move. Leave empty to use the main camera.")]
        [SerializeField] private Transform cameraRig;

        [Header("Title")]
        [Tooltip("The title shows this level's name, so it matches Level " +
            "Select. Leave empty for no title.")]
        [SerializeField] private LevelInfo levelInfo;
        [SerializeField] private GameObject titleCanvas;
        [SerializeField] private GameObject titleBand;
        [SerializeField] private TMP_Text titleText;

        [Header("Skip")]
        [Tooltip("Space, Enter or a left click skips the rest of the intro.")]
        [SerializeField] private bool allowSkip = true;
        [SerializeField] private GameObject skipHint;

        private IntroShot[] shots;
        private Coroutine routine;
        private bool played;
        private bool skipping;

        public event System.Action Finished;

        private void Awake()
        {
            if (cameraRig == null)
            {
                cameraRig = Camera.main.transform;
            }

            shots = FindShots();
            titleCanvas.SetActive(false);
        }

        private void Update()
        {
            if (routine != null && allowSkip && !skipping && SkipPressed())
            {
                Skip();
            }
        }

        public void Play()
        {
            if (played)
            {
                return;
            }

            played = true;

            if (shots.Length == 0)
            {
                Finished?.Invoke();
                return;
            }

            ShowOverlay();
            routine = StartCoroutine(PlayShots());
        }

        public void Skip()
        {
            if (routine == null || skipping)
            {
                return;
            }

            skipping = true;
            StopCoroutine(routine);
            routine = StartCoroutine(FinishWhenReleased());
        }

        private IntroShot[] FindShots()
        {
            List<IntroShot> found = new List<IntroShot>();

            foreach (IntroShot shot in GetComponentsInChildren<IntroShot>())
            {
                if (shot.IsSetUp)
                {
                    found.Add(shot);
                }
                else
                {
                    Debug.LogWarning($"{shot.name} needs From and To set, " +
                        "so the intro skips it", shot);
                }
            }

            return found.ToArray();
        }

        private void ShowOverlay()
        {
            bool hasTitle = levelInfo != null;

            if (hasTitle)
            {
                titleText.text = levelInfo.DisplayName;
            }

            titleBand.SetActive(hasTitle);
            skipHint.SetActive(allowSkip);
            titleCanvas.SetActive(hasTitle || allowSkip);
        }

        private IEnumerator PlayShots()
        {
            foreach (IntroShot shot in shots)
            {
                yield return shot.Play(cameraRig);
            }

            End();
        }

        // Space is also the knead Palm key, so wait until the skip press is
        // let go, or the kitchen would start with the dough being pressed.
        private IEnumerator FinishWhenReleased()
        {
            titleCanvas.SetActive(false);

            do
            {
                yield return null;
            }
            while (SkipHeld());

            End();
        }

        private void End()
        {
            routine = null;
            titleCanvas.SetActive(false);
            Finished?.Invoke();
        }

        private static bool SkipPressed()
        {
            // Paused: the pause menu owns input until it closes.
            if (Time.timeScale == 0f)
            {
                return false;
            }

            Keyboard keyboard = Keyboard.current;

            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame ||
                keyboard.enterKey.wasPressedThisFrame ||
                keyboard.numpadEnterKey.wasPressedThisFrame))
            {
                return true;
            }

            Mouse mouse = Mouse.current;

            return mouse != null && mouse.leftButton.wasPressedThisFrame &&
                !IsPointerOverUI();
        }

        private static bool SkipHeld()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            bool keyHeld = keyboard != null && (keyboard.spaceKey.isPressed ||
                keyboard.enterKey.isPressed ||
                keyboard.numpadEnterKey.isPressed);
            bool clickHeld = mouse != null && mouse.leftButton.isPressed;

            return keyHeld || clickHeld;
        }

        // Clicking the pause button must not also skip the intro.
        private static bool IsPointerOverUI()
        {
            return EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject();
        }
    }
}
