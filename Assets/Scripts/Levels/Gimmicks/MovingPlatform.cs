using UnityEngine;

namespace BreadBowl.Levels
{
    /// <summary>
    /// Moves this object back and forth around where it was placed, starting
    /// when the level loads. Uses a kinematic Rigidbody so loaves riding on it
    /// or bumping into it get carried and pushed by the physics engine.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MovingPlatform : MonoBehaviour
    {
        [Tooltip("Move along the world X axis.")]
        [SerializeField] private bool leftRight = true;

        [Tooltip("Move along the world Z axis.")]
        [SerializeField] private bool backForth;

        [Tooltip("Move along the world Y axis.")]
        [SerializeField] private bool upDown;

        [Tooltip("How far it moves away from its starting spot in each direction.")]
        [SerializeField][Min(0f)] private float distance = 3f;

        [Tooltip("Seconds to travel from one end to the other.")]
        [SerializeField][Min(0.1f)] private float tripTime = 2f;

        private Rigidbody body;
        private Vector3 startPosition;
        private float elapsed;

        // Runs in the editor when the component is first added, so the
        // Rigidbody shows up already set up correctly in the inspector.
        private void Reset()
        {
            SetUpBody();
        }

        private void Awake()
        {
            SetUpBody();
            startPosition = body.position;

        #if UNITY_EDITOR
            if (gameObject.isStatic)
            {
                Debug.LogWarning($"{name} is marked Static, so it will look frozen even though " +
                    "MovingPlatform is moving it. Untick Static at the top of the Inspector.", this);
            }
        #endif
        }

        private void SetUpBody()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

    #if UNITY_EDITOR
        // Static objects get merged into one frozen mesh when the game starts,
        // so a moving object must never be Static. Untick it automatically.
        private void OnValidate()
        {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // Static flags can't be changed during OnValidate itself, so wait a frame.
                UnityEditor.EditorApplication.delayCall += ClearStatic;
            }
        }

        private void ClearStatic()
        {
            if (this == null || !gameObject.isStatic)
            {
                return;
            }

            UnityEditor.Undo.RecordObject(gameObject, "Untick Static on moving platform");
            UnityEditor.GameObjectUtility.SetStaticEditorFlags(gameObject, 0);
            Debug.Log($"Unticked Static on {name} so MovingPlatform can move it.", this);
        }
    #endif

        private void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime;

            // Sine eases in and out at each end so loaves aren't flung off on the turnaround.
            float offset = Mathf.Sin(elapsed * Mathf.PI / tripTime) * distance;
            body.MovePosition(startPosition + Direction() * offset);
        }

        private Vector3 Direction()
        {
            Vector3 direction = Vector3.zero;

            if (leftRight)
            {
                direction += Vector3.right;
            }

            if (backForth)
            {
                direction += Vector3.forward;
            }

            if (upDown)
            {
                direction += Vector3.up;
            }

            return direction.normalized;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = Application.isPlaying ? startPosition : transform.position;
            Vector3 reach = Direction() * distance;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(center - reach, center + reach);
            Gizmos.DrawWireSphere(center - reach, 0.25f);
            Gizmos.DrawWireSphere(center + reach, 0.25f);
        }
    }
}
