using UnityEngine;
using UnityEngine.InputSystem;

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
        private bool foldReady = true;

        private void Awake()
        {
            dough = GetComponent<KneadableDough>();
            rotateCCWAction = InputSystem.actions.FindAction("Knead/RotateCCW", true);
            rotateCWAction = InputSystem.actions.FindAction("Knead/RotateCW", true);
            palmAction = InputSystem.actions.FindAction("Knead/Palm", true);
            foldGrabAction = InputSystem.actions.FindAction("Knead/FoldGrab", true);
        }

        private void OnEnable()
        {
            rotateCCWAction.Enable();
            rotateCWAction.Enable();
            palmAction.Enable();
            foldGrabAction.Enable(); 
        }

        private void OnDisable()
        {
            rotateCCWAction.Disable();
            rotateCWAction.Disable();
            palmAction.Disable();
            foldGrabAction.Disable(); 
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

