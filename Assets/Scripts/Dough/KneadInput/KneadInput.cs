using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace BreadBowl.Dough
{
    [RequireComponent(typeof(KneadableDough))]
    public class KneadInput : MonoBehaviour
    {
        private KneadableDough dough;
        private InputAction rotateCCWAction;
        private InputAction rotateCWAction;
        private InputAction palmAction;
        private InputAction foldGrabAction;
        private InputAction[] pressRowActions;
        private bool foldReady = true;
        [SerializeField] private KneadAudio kneadAudio;

        private void Awake()
        {
            dough = GetComponent<KneadableDough>();
            rotateCCWAction = InputSystem.actions.FindAction("Knead/RotateCCW", true);
            rotateCWAction = InputSystem.actions.FindAction("Knead/RotateCW", true);
            palmAction = InputSystem.actions.FindAction("Knead/Palm", true);
            foldGrabAction = InputSystem.actions.FindAction("Knead/FoldGrab", true);
            pressRowActions = new[]
            {
                InputSystem.actions.FindAction("Knead/PressRow1", true),
                InputSystem.actions.FindAction("Knead/PressRow2", true),
                InputSystem.actions.FindAction("Knead/PressRow3", true),
                InputSystem.actions.FindAction("Knead/PressRow4", true)
            };
        }

        private void OnEnable()
        {
            rotateCCWAction.Enable();
            rotateCWAction.Enable();
            palmAction.Enable();
            foldGrabAction.Enable(); 
            foreach (InputAction rowAction in pressRowActions)
            {
                rowAction.Enable();
            }
        }

        private void OnDisable()
        {
            rotateCCWAction.Disable();
            rotateCWAction.Disable();
            palmAction.Disable();
            foldGrabAction.Disable(); 
            foreach (InputAction rowAction in pressRowActions)
            {
                rowAction.Disable();
            }
        }

        private void Update()
        {
            int grabbedKeys = CountPressedControls(foldGrabAction);

            if (grabbedKeys == 0)
            {
                foldReady = true;
            }

            if (palmAction.IsPressed())
            {
                if (grabbedKeys > 0)
                {
                    foldReady = false;
                }

                ApplyPress();

                kneadAudio.RegisterKnead();

                return;
            }

            if (rotateCCWAction.WasPressedThisFrame())
            {
                dough.Rotate(-1);
            }

            if (rotateCWAction.WasPressedThisFrame())
            {
                dough.Rotate(1);
            }

            if (foldReady && grabbedKeys >= dough.Settings.FoldGrabKeyCount)
            {
                foldReady = false;
                Debug.Log("Folding");
                dough.Fold();
            }
        }

        private void ApplyPress()
        {
            KneadSettings settings = dough.Settings;
            float amount = settings.KneadForcePerSecond * Time.deltaTime;

            for (int row = 0; row < pressRowActions.Length; row++)
            {
                ReadOnlyArray<InputControl> keys = pressRowActions[row].controls;
                float v = (row + 0.5f) / pressRowActions.Length;

                for (int i = 0; i < keys.Count; i++)
                {
                    if (keys[i].IsPressed())
                    {
                        float u = (i + 0.5f) / keys.Count;
                        dough.PalmPress(new Vector2(u, v), settings.PressRadius, amount, settings.PressFalloff);
                    }
                }
            }
        }

        private static int CountPressedControls(InputAction action)
        {
            int count = 0;

            foreach (InputControl control in action.controls)
            {
                if (control.IsPressed())
                {
                    count++;
                }
            }

            return count;
        }
    }
}
