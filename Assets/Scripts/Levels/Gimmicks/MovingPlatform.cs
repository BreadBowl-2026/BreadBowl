using System;
using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    /// <summary>
    /// Moves this object back and forth around where it was placed, starting
    /// when the level loads. Uses a kinematic Rigidbody so loaves riding on it
    /// or bumping into it get carried and pushed by the physics engine.
    /// Pins standing on it can be locked to it until the dough gets close, so
    /// the platform's motion can't knock them over by itself.
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

        [Tooltip("Snap back to the starting spot at the end of every roll. " +
            "Always on when Carried Pins is set.")]
        [SerializeField] private bool resetEachRoll;

        [Header("Carried Pins")]
        [Tooltip("Pins standing on this platform, usually a PinSpawner. They're locked to the " +
            "platform and move with it until the dough gets close, then they're let go as " +
            "normal pins. Leave empty if no pins ride on it.")]
        [SerializeField] private Transform carriedPins;

        [Tooltip("How close the dough has to get to a carried pin before the pins are let go.")]
        [SerializeField][Min(0f)] private float releaseDistance = 2f;

        private Rigidbody body;
        private Vector3 startPosition;
        private float elapsed;
        private LevelController level;
        private Loaf rollingLoaf;

        private Rigidbody[] riders = Array.Empty<Rigidbody>();
        private Vector3[] riderOffsets;
        private Quaternion[] riderRotations;
        private bool holdingRiders;
        private bool regrabRiders;

        private bool ResetsEachRoll => resetEachRoll || carriedPins != null;

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

        private void OnEnable()
        {
            if (!ResetsEachRoll)
            {
                return;
            }

            level = FindFirstObjectByType<LevelController>();

            if (level != null)
            {
                level.RollStarted += HandleRollStarted;
                level.RollEnded += HandleRollEnded;
            }
        }

        private void OnDisable()
        {
            if (level != null)
            {
                level.RollStarted -= HandleRollStarted;
                level.RollEnded -= HandleRollEnded;
                level = null;
            }
        }

        // PinSpawner makes its pins in Awake, so they exist by now.
        private void Start()
        {
            if (carriedPins == null)
            {
                return;
            }

            if (level == null)
            {
                Debug.LogWarning($"{name} has Carried Pins but there's no LevelController in the " +
                    "scene to say when the dough is rolling, so the pins are left loose.", this);
                return;
            }

            riders = carriedPins.GetComponentsInChildren<Rigidbody>();
            riderOffsets = new Vector3[riders.Length];
            riderRotations = new Quaternion[riders.Length];

            for (int i = 0; i < riders.Length; i++)
            {
                Transform pin = riders[i].transform;
                riderOffsets[i] = pin.position - startPosition;
                riderRotations[i] = pin.rotation;

                // Keeps the pins in step with the platform on screen, which is interpolated too.
                riders[i].interpolation = RigidbodyInterpolation.Interpolate;
            }

            HoldRiders();
        }

        private void HandleRollStarted(Loaf loaf)
        {
            rollingLoaf = loaf;
        }

        // RollEnded fires just before the goals put their pins back, so the
        // platform is already at its starting spot when the pins land on it.
        private void HandleRollEnded(Loaf loaf)
        {
            rollingLoaf = null;
            elapsed = 0f;
            body.position = startPosition;
            transform.position = startPosition;

            if (riders.Length == 0)
            {
                return;
            }

            // Let go first so the goal can reset the pins like normal ones,
            // then lock them again next physics step if it put them back.
            if (holdingRiders)
            {
                ReleaseRiders(Vector3.zero);
            }

            regrabRiders = true;
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
            Vector3 target = startPosition + Direction() * offset;
            body.MovePosition(target);

            if (regrabRiders)
            {
                regrabRiders = false;

                // Pins left lying down mean the level was completed, so leave them be.
                if (RidersBackAtStart())
                {
                    HoldRiders();
                }
            }

            if (!holdingRiders)
            {
                return;
            }

            if (DoughIsClose())
            {
                ReleaseRiders(Velocity());
                return;
            }

            for (int i = 0; i < riders.Length; i++)
            {
                riders[i].MovePosition(target + riderOffsets[i]);
            }
        }

        // Called while the platform is at its starting spot, so each pin
        // goes back to exactly where it was placed.
        private void HoldRiders()
        {
            for (int i = 0; i < riders.Length; i++)
            {
                Rigidbody rider = riders[i];
                Vector3 position = startPosition + riderOffsets[i];

                if (!rider.isKinematic)
                {
                    rider.linearVelocity = Vector3.zero;
                    rider.angularVelocity = Vector3.zero;
                }

                rider.isKinematic = true;
                rider.position = position;
                rider.rotation = riderRotations[i];
                rider.transform.SetPositionAndRotation(position, riderRotations[i]);
            }

            holdingRiders = true;
        }

        // The pins keep the platform's speed so they don't slide off it.
        private void ReleaseRiders(Vector3 velocity)
        {
            holdingRiders = false;

            foreach (Rigidbody rider in riders)
            {
                rider.isKinematic = false;
                rider.linearVelocity = velocity;
                rider.angularVelocity = Vector3.zero;
            }
        }

        private bool RidersBackAtStart()
        {
            for (int i = 0; i < riders.Length; i++)
            {
                Transform pin = riders[i].transform;
                Vector3 start = startPosition + riderOffsets[i];

                if (Vector3.Distance(pin.position, start) > 0.05f ||
                    Quaternion.Angle(pin.rotation, riderRotations[i]) > 5f)
                {
                    return false;
                }
            }

            return true;
        }

        private bool DoughIsClose()
        {
            if (rollingLoaf == null)
            {
                return false;
            }

            Vector3 dough = rollingLoaf.Body.position;
            float reach = releaseDistance * releaseDistance;

            foreach (Rigidbody rider in riders)
            {
                if ((rider.position - dough).sqrMagnitude <= reach)
                {
                    return true;
                }
            }

            return false;
        }

        // Rate of change of the sine in FixedUpdate.
        private Vector3 Velocity()
        {
            float speed = Mathf.Cos(elapsed * Mathf.PI / tripTime) * distance * Mathf.PI / tripTime;
            return Direction() * speed;
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

            if (carriedPins != null)
            {
                // How close the dough gets before the pins are let go.
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(carriedPins.position, releaseDistance);
            }
        }
    }
}
