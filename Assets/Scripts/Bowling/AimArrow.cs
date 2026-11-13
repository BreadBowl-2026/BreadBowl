using UnityEngine;

namespace BreadBowl.Bowling
{
    // Lays a flat arrow on the lane in front of the ball, pointing where it will launch
    [RequireComponent(typeof(LineRenderer))]
    public class AimArrow : MonoBehaviour
    {
        [Tooltip("Leave empty to use the BreadLauncher on a parent object")]
        [SerializeField] private BreadLauncher launcher;

        [Header("Shape (in multiples of the ball's radius)")]
        [SerializeField] private float gap = 0.3f;
        [SerializeField] private float length = 5f;
        [SerializeField] private float shaftWidth = 0.4f;
        [SerializeField] private float headWidth = 1.2f;
        [Range(0.05f, 0.95f)]
        [SerializeField] private float headLength = 0.25f;
        [SerializeField] private float lift = 0.05f;

        // Tiny step along the line so the shaft widens into the head with a hard edge
        private const float HeadStep = 0.001f;

        private LineRenderer line;
        private Rigidbody measuredBall;
        private float radius;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.alignment = LineAlignment.TransformZ;
            line.positionCount = 4;
            line.numCapVertices = 0;
            line.numCornerVertices = 0;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.widthCurve = BuildWidthCurve();

            if (launcher == null)
            {
                launcher = GetComponentInParent<BreadLauncher>();
            }

            if (launcher == null)
            {
                Debug.LogError("AimArrow: no BreadLauncher assigned", this);
            }
        }

        private void LateUpdate()
        {
            bool aiming = launcher != null && launcher.Ball != null && !launcher.HasLaunched;
            line.enabled = aiming;

            if (!aiming)
            {
                return;
            }

            if (measuredBall != launcher.Ball)
            {
                measuredBall = launcher.Ball;
                radius = MeasureRadius(measuredBall);
            }

            Vector3 normal = launcher.GroundNormal;
            Vector3 direction = launcher.LaunchDirection;
            float arrowLength = radius * length;
            float headStart = 1f - headLength;

            Vector3 start = launcher.AimOrigin
                + normal * (radius * lift)
                + direction * (radius * (1f + gap));

            line.widthMultiplier = radius;
            line.SetPosition(0, start);
            line.SetPosition(1, start + direction * (arrowLength * headStart));
            line.SetPosition(2, start + direction * (arrowLength * (headStart + HeadStep)));
            line.SetPosition(3, start + direction * arrowLength);

            // TransformZ alignment faces the line along our forward, so point it up off the lane
            transform.rotation = Quaternion.LookRotation(normal, direction);
        }

        private AnimationCurve BuildWidthCurve()
        {
            float headStart = 1f - headLength;
            return new AnimationCurve(
                new Keyframe(0f, shaftWidth),
                new Keyframe(headStart, shaftWidth),
                new Keyframe(headStart + HeadStep, headWidth),
                new Keyframe(1f, 0f)
            );
        }

        private static float MeasureRadius(Rigidbody body)
        {
            Collider shape = body.GetComponentInChildren<Collider>();
            if (shape == null)
            {
                return 0.5f;
            }

            // Widest half-size across the lane, so the arrow clears the ball when it's turned
            Vector3 extents = shape.bounds.extents;
            return Mathf.Max(extents.x, extents.z);
        }
    }
}
