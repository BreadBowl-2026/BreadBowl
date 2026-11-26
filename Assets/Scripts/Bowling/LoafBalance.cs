using BreadBowl.Dough;
using UnityEngine;

namespace BreadBowl.Bowling
{
    [RequireComponent(typeof(Rigidbody))]
    public class LoafBalance : MonoBehaviour
    {
        [SerializeField, Range(0f, 50f)] private float strength = 2f;
        [SerializeField] private float gizmoRadius = 0.1f;

        public void Apply(Mesh mesh, KneadResult knead)
        {
            GetComponent<Rigidbody>().centerOfMass = CenterOfMass(mesh, knead);
        }

        private Vector3 CenterOfMass(Mesh mesh, KneadResult knead)
        {
            Bounds bounds = mesh.bounds;

            if (knead == null)
            {
                return Vector3.Scale(bounds.center, transform.lossyScale);
            }

            Vector2 uv = OverkneadedCenter(knead);
            Vector3 target = new Vector3(
                Mathf.Lerp(bounds.min.x, bounds.max.x, uv.x),
                bounds.center.y,
                Mathf.Lerp(bounds.min.z, bounds.max.z, uv.y));

            float pull = Mathf.Clamp01(knead.OverFraction * strength);
            Vector3 meshCenter = Vector3.Lerp(bounds.center, target, pull);

            return Vector3.Scale(meshCenter, transform.lossyScale);
        }

        private Vector2 OverkneadedCenter(KneadResult knead)
        {
            int resolution = knead.Resolution;
            Vector2 sum = Vector2.zero;
            int count = 0;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    if (!knead.IsInside(x, y) ||
                        knead.Settings.Classify(knead.Get(x, y)) != KneadLevel.Over)
                    {
                        continue;
                    }

                    sum += new Vector2((x + 0.5f) / resolution, (y + 0.5f) / resolution);
                    count++;
                }
            }

            return count > 0 ? sum / count : new Vector2(0.5f, 0.5f);
        }

        private void OnDrawGizmos()
        {
            float height = GetComponent<Renderer>().bounds.max.y + gizmoRadius * 2f;
            Vector3 center = transform.position;
            Vector3 centerOfMass = GetComponent<Rigidbody>().worldCenterOfMass;

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(new Vector3(center.x, height, center.z), gizmoRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(centerOfMass.x, height, centerOfMass.z), gizmoRadius);
        }
    }
}