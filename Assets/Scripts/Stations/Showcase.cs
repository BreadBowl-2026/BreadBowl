using UnityEngine;
using UnityEngine.InputSystem;

namespace BreadBowl
{
    public class Showcase : MonoBehaviour
    {
        [SerializeField] private Transform viewer;
        [SerializeField] private float autoSpinSpeed = 30f;
        [SerializeField] private float rotateSpeed = 120f;
        [SerializeField] private float resumeDelay = 2f;

        private InputAction rotateAction;
        private float idleTime;

        private void Awake()
        {
            rotateAction = InputSystem.actions.FindAction("Showcase/Rotate", true);
        }

        private void OnEnable()
        {
            rotateAction.Enable();
            idleTime = resumeDelay;
        }

        private void OnDisable()
        {
            rotateAction.Disable();
        }

        private void Update()
        {
            Vector2 input = rotateAction.ReadValue<Vector2>();

            if (input.sqrMagnitude > 0.01f)
            {
                idleTime = 0f;
                transform.Rotate(Vector3.up, -input.x * rotateSpeed * Time.deltaTime, Space.World);
                transform.Rotate(viewer.right, input.y * rotateSpeed * Time.deltaTime, Space.World);
                return;
            }

            idleTime += Time.deltaTime;

            if (idleTime >= resumeDelay)
            {
                transform.Rotate(Vector3.up, autoSpinSpeed * Time.deltaTime, Space.World);
            }
        }
    }
}

