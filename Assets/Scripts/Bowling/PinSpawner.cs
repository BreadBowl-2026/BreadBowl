using UnityEngine;

namespace BreadBowl.Bowling
{
    // Spawns a rack of pins centred on this object, with their bases on the floor it sits on.
    // Its forward (blue) axis points down the lane, away from the bowler.
    public class PinSpawner : MonoBehaviour
    {
        // Regulation pin spots are 12 in apart and a pin is 4.766 in at its widest
        private const float RegulationSpacing = 12f / 4.766f;

        // Rows of a triangle rack are closer together than the pins in a row
        private const float Row = 0.8660254f;

        [Tooltip("Drag the pin prefab here (needs a Rigidbody)")]
        [SerializeField] private GameObject pinPrefab;

        [Tooltip("1 Last Cup, 2 Flatline, 3 Triangle, 4 Diamond, 5 Crosshair, 6 Triangle, 7 Honeycomb, 8 Marching, 9 Triangle without the centre pin, 10 Triangle")]
        [Range(1, 10)]
        [SerializeField] private int pinCount = 10;

        // Pin spots in pin spacings: x across the lane, y down the lane, front pin first
        private static readonly Vector2[][] Layouts =
        {
            // 1 Last Cup
            new[] { new Vector2(0f, 0f) },

            // 2 Flatline
            new[] { new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f) },

            // 3 Triangle
            new[]
            {
                new Vector2(0f, 0f),
                new Vector2(-0.5f, Row), new Vector2(0.5f, Row)
            },

            // 4 Diamond
            new[]
            {
                new Vector2(0f, 0f),
                new Vector2(-0.5f, Row), new Vector2(0.5f, Row),
                new Vector2(0f, 2f * Row)
            },

            // 5 Crosshair
            new[]
            {
                new Vector2(0f, -1f),
                new Vector2(-1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 1f)
            },

            // 6 Triangle
            new[]
            {
                new Vector2(0f, 0f),
                new Vector2(-0.5f, Row), new Vector2(0.5f, Row),
                new Vector2(-1f, 2f * Row), new Vector2(0f, 2f * Row), new Vector2(1f, 2f * Row)
            },

            // 7 Honeycomb
            new[]
            {
                new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-1f, Row), new Vector2(0f, Row), new Vector2(1f, Row),
                new Vector2(-0.5f, 2f * Row), new Vector2(0.5f, 2f * Row)
            },

            // 8 Marching
            new[]
            {
                new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-0.5f, 2f), new Vector2(0.5f, 2f),
                new Vector2(-0.5f, 3f), new Vector2(0.5f, 3f)
            },

            // 9 Triangle without the centre pin
            new[]
            {
                new Vector2(0f, 0f),
                new Vector2(-0.5f, Row), new Vector2(0.5f, Row),
                new Vector2(-1f, 2f * Row), new Vector2(1f, 2f * Row),
                new Vector2(-1.5f, 3f * Row), new Vector2(-0.5f, 3f * Row), new Vector2(0.5f, 3f * Row), new Vector2(1.5f, 3f * Row)
            },

            // 10 Triangle
            new[]
            {
                new Vector2(0f, 0f),
                new Vector2(-0.5f, Row), new Vector2(0.5f, Row),
                new Vector2(-1f, 2f * Row), new Vector2(0f, 2f * Row), new Vector2(1f, 2f * Row),
                new Vector2(-1.5f, 3f * Row), new Vector2(-0.5f, 3f * Row), new Vector2(0.5f, 3f * Row), new Vector2(1.5f, 3f * Row)
            }
        };

        // Spawned in Awake so StrikeGoal finds the pins as children when it starts
        private void Awake()
        {
            if (pinPrefab == null)
            {
                Debug.LogError("PinSpawner: no pin prefab assigned", this);
                return;
            }

            Vector2[] spots = Layouts[pinCount - 1];
            Quaternion rotation = transform.rotation * pinPrefab.transform.rotation;
            Transform[] pins = new Transform[spots.Length];

            for (int i = 0; i < spots.Length; i++)
            {
                pins[i] = Instantiate(pinPrefab, transform.position, rotation, transform).transform;
                pins[i].name = $"{pinPrefab.name} {i + 1}";
            }

            // Measure a spawned pin so the spacing follows whatever size the prefab is
            Bounds bounds = MeasurePin(pins[0]);
            float width = Mathf.Max(bounds.size.x, bounds.size.z);
            if (bounds.size.y < width)
            {
                Debug.LogWarning($"PinSpawner: {pinPrefab.name} spawns lying down, check the prefab's rotation", this);
            }

            float spacing = width * RegulationSpacing;
            Vector3 pivotToBase = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z) - pins[0].position;

            for (int i = 0; i < spots.Length; i++)
            {
                pins[i].position = SpotPosition(spots, i, spacing) - pivotToBase;
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying || pinPrefab == null)
            {
                return;
            }

            // Preview the rack footprint, using the prefab's size as it would spawn under this object
            Bounds bounds = MeasurePin(pinPrefab.transform);
            float width = Mathf.Max(bounds.size.x, bounds.size.z) * transform.lossyScale.x;
            if (width <= 0f)
            {
                return;
            }

            Vector2[] spots = Layouts[pinCount - 1];
            float spacing = width * RegulationSpacing;

            Gizmos.color = Color.red;
            for (int i = 0; i < spots.Length; i++)
            {
                DrawCircle(SpotPosition(spots, i, spacing), width * 0.5f);
            }
        }

        private Vector3 SpotPosition(Vector2[] spots, int index, float spacing)
        {
            Vector2 spot = (spots[index] - Centre(spots)) * spacing;
            return transform.position + transform.rotation * new Vector3(spot.x, 0f, spot.y);
        }

        private static Vector2 Centre(Vector2[] spots)
        {
            Vector2 sum = Vector2.zero;
            foreach (Vector2 spot in spots)
            {
                sum += spot;
            }

            return sum / spots.Length;
        }

        private static Bounds MeasurePin(Transform pin)
        {
            Renderer[] renderers = pin.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(pin.position, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private void DrawCircle(Vector3 centre, float radius)
        {
            const int segments = 24;
            Vector3 previous = centre + transform.right * radius;

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector3 next = centre + (transform.right * Mathf.Cos(angle) + transform.forward * Mathf.Sin(angle)) * radius;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
