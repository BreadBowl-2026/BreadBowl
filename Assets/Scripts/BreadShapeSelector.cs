using UnityEngine;
using UnityEngine.InputSystem;

public class BreadShapeSelector : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    private void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            SelectShape();
        }
    }

    private void SelectShape()
    {
        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            BreadShapeChoice choice =
                hit.collider.GetComponent<BreadShapeChoice>();

            if (choice != null)
            {
                choice.Select();
            }
        }
    }
}
