using UnityEngine;
using System.Collections;

public class CameraWork : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraRig;
    [SerializeField] private Transform shapeView;
    [SerializeField] private Transform bakeView;

    [SerializeField] private float moveSpeed = 0.5f;

    private Coroutine cameraMovement;


    public void MoveCameraSmoothly(Transform targetView)
    {
        if (cameraMovement != null)
        {
            StopCoroutine(cameraMovement);
        }

        cameraMovement = StartCoroutine(MoveCameraCoroutine(targetView));
    }

    private IEnumerator MoveCameraCoroutine(Transform targetView)
    {
        Vector3 startPosition = cameraRig.position;
        Quaternion startRotation = cameraRig.rotation;
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * moveSpeed;
            float t = Mathf.Clamp01(elapsedTime);
            cameraRig.position = Vector3.Lerp(startPosition, targetView.position, t);
            cameraRig.rotation = Quaternion.Slerp(startRotation, targetView.rotation, t);
            yield return null;
        }
        cameraRig.position = targetView.position;
        cameraRig.rotation = targetView.rotation;
    }
}
