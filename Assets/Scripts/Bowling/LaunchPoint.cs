using UnityEngine;

namespace BreadBowl.Bowling
{
    public class LaunchPoint : MonoBehaviour
    {
        [SerializeField] private Transform cameraView;
        [SerializeField] private float gizmoLength = 3f;

        public Transform CameraView => cameraView;

        private void OnDrawGizmos()
        {
            Vector3 start = transform.position;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 end = start + forward * gizmoLength;
            Vector3 headLeft = Quaternion.AngleAxis(150f, Vector3.up) * forward * 0.75f;
            Vector3 headRight = Quaternion.AngleAxis(-150f, Vector3.up) * forward * 0.75f;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(start, 0.5f);
            Gizmos.DrawLine(start, end);
            Gizmos.DrawLine(end, end + headLeft);
            Gizmos.DrawLine(end, end + headRight);
        }
    }
}

