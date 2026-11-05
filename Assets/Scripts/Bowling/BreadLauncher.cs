using UnityEngine;
using UnityEngine.InputSystem;

namespace BreadBowl.Bowling
{
    // Runs before CameraFollow so the camera offset is taken after the ball lands on the lane
    [DefaultExecutionOrder(-100)]
    public class BreadLauncher : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Drag the ball/bread this launcher controls here (needs a Rigidbody)")]
        [SerializeField] private Rigidbody ball;

        [Header("Lane Movement")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float maxLateralOffset = 8f;
        [SerializeField] private Vector3 laneForward = Vector3.forward;

        [Header("Rotation")]
        [SerializeField] private float yawSpeed = 90f;
        [SerializeField] private float tiltSpeed = 90f;

        [Header("Power Gauge")]
        [Range(0f, 1f)]
        [SerializeField] private float minPower = 0.1f;
        [Range(0f, 1f)]
        [SerializeField] private float maxPower = 1f;
        [SerializeField] private float gaugeSpeed = 0.9f;

        [Header("Launch")]
        [SerializeField] private float maxLaunchSpeed = 20f;
        [SerializeField] private bool alignLaunchToSlope = true;

        [Header("Ground Check")]
        [SerializeField] private bool snapToGround = true;
        [SerializeField] private float snapLift = 1f;
        [SerializeField] private float snapMaxDistance = 10f;

        [Header("Testing")]
        [SerializeField] private Key resetKey = Key.R;

        private Transform ballTransform;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Quaternion aimRotation;
        private float lateralOffset;
        private float gaugeTime;

        public float Power { get; private set; }
        public bool HasLaunched { get; private set; }

        private Vector3 LaneForward => Vector3.ProjectOnPlane(laneForward, Vector3.up).normalized;
        private Vector3 LaneRight => Vector3.Cross(Vector3.up, LaneForward);

        public event System.Action Launched;

        private void Start()
        {
            Power = minPower;

            if (ball != null)
            {
                HoldBall();
            }
        }

        private void Update()
        {
            if (resetKey != Key.None && Keyboard.current != null && Keyboard.current[resetKey].wasPressedThisFrame)
            {
                ResetLaunch();
            }

            if (HasLaunched)
            {
                return;
            }

            // Gauge bounces between min and max power until the player launches
            gaugeTime += Time.deltaTime * gaugeSpeed;
            float range = maxPower - minPower;
            Power = range > 0f ? minPower + Mathf.PingPong(gaugeTime, range) : minPower;
        }

        public void Move(float direction)
        {
            if (!CanAim())
            {
                return;
            }

            lateralOffset = Mathf.Clamp(
                lateralOffset + direction * moveSpeed * Time.deltaTime,
                -maxLateralOffset,
                maxLateralOffset
            );
            ApplyPose();
        }

        public void Rotate(float yawDirection, float tiltDirection)
        {
            if (!CanAim())
            {
                return;
            }

            // Yaw spins around world up; tilt rolls around the lane's sideways axis,
            // so positive tilt rolls the top of the ball away from the camera
            aimRotation = Quaternion.AngleAxis(yawDirection * yawSpeed * Time.deltaTime, Vector3.up)
                * Quaternion.AngleAxis(tiltDirection * tiltSpeed * Time.deltaTime, LaneRight)
                * aimRotation;
            ApplyPose();
        }

        public void Launch()
        {
            if (!CanAim())
            {
                return;
            }

            HasLaunched = true;

            // Always launch straight down the lane, following the slope if there is one
            Vector3 direction = LaneForward;
            if (alignLaunchToSlope && SweepToGround(out RaycastHit hit))
            {
                direction = Vector3.ProjectOnPlane(direction, hit.normal).normalized;
            }

            ball.isKinematic = false;
            ball.linearVelocity = direction * (maxLaunchSpeed * Power);
            ball.angularVelocity = Vector3.zero;

            Launched?.Invoke();
        }

        public void ResetLaunch()
        {
            if (ballTransform == null)
            {
                return;
            }

            if (!ball.isKinematic)
            {
                ball.linearVelocity = Vector3.zero;
                ball.angularVelocity = Vector3.zero;
                ball.isKinematic = true;
            }

            HasLaunched = false;
            lateralOffset = 0f;
            aimRotation = startRotation;
            gaugeTime = 0f;
            Power = minPower;
            ApplyPose();
        }

        public void SetBall(Rigidbody newBall, LaunchPoint launchPoint)
        {
            ball = newBall;
            laneForward = launchPoint.transform.forward;
            ball.transform.SetPositionAndRotation(launchPoint.transform.position, launchPoint.transform.rotation);

            HasLaunched = false;
            lateralOffset = 0f;
            gaugeTime = 0f;
            Power = minPower;

            HoldBall();
        }

        private bool CanAim()
        {
            return ballTransform != null && !HasLaunched;
        }

        private void ApplyPose()
        {
            ballTransform.SetPositionAndRotation(startPosition + LaneRight * lateralOffset, aimRotation);
            SnapToGround();
        }

        private void SnapToGround()
        {
            if (snapToGround && SweepToGround(out RaycastHit hit))
            {
                ballTransform.position += Vector3.down * (hit.distance - snapLift);
            }
        }

        private bool SweepToGround(out RaycastHit hit)
        {
            // Start the sweep a little higher so a tilted ball already poking into the lane still finds it
            ballTransform.position += Vector3.up * snapLift;
            Physics.SyncTransforms();
            bool found = ball.SweepTest(Vector3.down, out hit, snapLift + snapMaxDistance, QueryTriggerInteraction.Ignore);
            ballTransform.position -= Vector3.up * snapLift;
            return found;
        }

        private void HoldBall()
        {
            ballTransform = ball.transform;
            ball.isKinematic = true;
            startRotation = ballTransform.rotation;
            aimRotation = startRotation;
            SnapToGround();
            startPosition = ballTransform.position;
        }
    }
}
