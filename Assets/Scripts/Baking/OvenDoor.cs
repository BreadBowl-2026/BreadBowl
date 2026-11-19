
using UnityEngine;

public class OvenDoor : MonoBehaviour
{
    [SerializeField] private float openingAngle = 80f;
    [SerializeField] private float speed = 120f;

    private bool isOpen = true;

    public void SetOpeningAngle(float angle)
    {
        openingAngle = angle;
    }

    void Start()
    {
        transform.localRotation = Quaternion.Euler(
            openingAngle, 0f, 0f
        );
    }

    void Update()
    {
        Quaternion target = Quaternion.Euler(
            isOpen ? openingAngle : 0f, 0f, 0f
        );

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            target,
            speed * Time.deltaTime
        );
    }

    public void OpenDoor()
    {
        isOpen = true;
    }

    public void CloseDoor()
    {
        isOpen = false;
    }
}
