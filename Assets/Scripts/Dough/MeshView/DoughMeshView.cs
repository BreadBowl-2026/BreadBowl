using UnityEngine;

namespace BreadBowl.Dough
{
    /// <summary>
    /// Cosmetic mesh view for Kneadable Dough. Applies dents to the mesh that 
    /// correspond to keypresses wherever the player presses that relax back
    /// to their rest shape. Does not use about the data layer KneadGrid at all.
    /// </summary>
    [RequireComponent(typeof(KneadableDough))]
    public class DoughMeshView : MonoBehaviour
    {
        // the child that holds the visual mesh. rotating visual instead of the 
        // KneadableDough object keeps the transform in the player frame and 
        // avoids double rotating the data layer
        [SerializeField] private Transform visual;
        [SerializeField, Min(0f)] private float turnSpeed = 540f;        
        [SerializeField, Min(0f)] private float dentSpeed = 0.5f;
        [SerializeField, Min(0f)] private float maxDentDepth = 0.15f;
        [SerializeField, Min(0f)] private float relaxRate = 1.0f;
        [SerializeField, Min(0.001f)] private float hingeWidth = 0.15f;
        [SerializeField, Range(0.05f, 1f)] private float flattenHeight = 0.35f;
        [SerializeField, Min(1f)] private float flattenSpread = 1.4f;

        private KneadableDough dough;
        private Mesh mesh;
        private Bounds restBounds;
        private Vector3[] restVertices;
        private Vector3[] restNormals;
        private Vector3[] vertices;
        private float[] dentDepths;
        private bool folding;
        private float foldDuration;
        private float foldElapsed;
        private float foldAngle;
        private float foldSettle;
        private Vector3 foldForward;
        private Vector3 foldAxis;
        private Vector3 foldPivot;
        private float foldFlatten;

        private void Awake()
        {
            dough = GetComponent<KneadableDough>();

            if (visual == null)
            {
                return;
            }

            // important to use .mesh instead of .sharedMesh since every Sphere object
            // references the same mesh. this creates a copy so denting the dough doesn't dent
            // every Sphere component in the scene
            mesh = visual.GetComponent<MeshFilter>().mesh;
            mesh.MarkDynamic();
            restBounds = mesh.bounds;
            restVertices = mesh.vertices;
            restNormals = mesh.normals;
            vertices = new Vector3[restVertices.Length];
            dentDepths = new float[restVertices.Length];
        }

        private void OnEnable()
        {
            dough.Pressed += AddDent;
            dough.FoldStarted += StartFold;
        }

        private void OnDisable()
        {
            dough.Pressed -= AddDent;
            dough.FoldStarted -= StartFold;
        }

        private void OnDestroy()
        {
            if (mesh != null)
            {
                Destroy(mesh);
            }
        }

        // LateUpdate to ensure KneadInput.Update has already been applied each frame
        private void LateUpdate()
        {
            if (visual == null)
            {
                return;
            }

            Turn();
            Relax();
            AdvanceFold();
            ApplyShape();
        }

        // rotates the visual towards the actual dough's current orientation. ensure 270 -> 0
        // goes 90 degrees in one direction instead of snapping back 270 the other way
        private void Turn()
        {
            Quaternion target = Quaternion.Euler(0f, dough.Orientation, 0f);
            visual.localRotation = Quaternion.RotateTowards(visual.localRotation, target, turnSpeed * Time.deltaTime);
        }

        // called by the KneadableDough.Pressed event. center and radius are in 2D dough UV terms.
        // since the visual is already rotated when this is called, it maps straight onto the mesh's
        // localized x/z from a top down perspective
        private void AddDent(Vector2 center, float radius)
        {
            if (mesh == null)
            {
                return;
            }

            // grid u maps to local x and grid v maps to local z, same as the gizmo view
            // using the rest bounds instead of a hard coded radius should let this work
            // for any mesh
            Vector2 localCenter = new Vector2(
                Mathf.Lerp(restBounds.min.x, restBounds.max.x, center.x),
                Mathf.Lerp(restBounds.min.z, restBounds.max.z, center.y));
            float localRadius = radius * restBounds.size.x;
            float amount = dentSpeed * Time.deltaTime;
            KneadBrushFalloff falloff = dough.Settings.PressFalloff;

            for (int i = 0; i < restVertices.Length; i++)
            {
                // only dent the top of the dough
                if (restNormals[i].y <= 0f)
                {
                    continue;
                }

                // distance is measured looking straight down, ignoring height
                Vector2 topDown = new Vector2(restVertices[i].x, restVertices[i].z);
                float distance = Vector2.Distance(topDown, localCenter);

                if (distance >= localRadius)
                {
                    continue;
                }

                float weight = KneadBrush.Weight(distance / localRadius, falloff);
                dentDepths[i] = Mathf.Min(dentDepths[i] + amount * weight, maxDentDepth);
            }
        }

        // exponential decay toward the rest shape. deep dents recover quickly at first then
        // settle slowly. using exp keeps it independent from the frame rate
        private void Relax()
        {
            float keep = Mathf.Exp(-relaxRate * Time.deltaTime);

            for (int i = 0; i < dentDepths.Length; i++)
            {
                dentDepths[i] *= keep;
            }
        }

        private void ApplyShape()
        {
            for (int i = 0; i < restVertices.Length; i++)
            {
                Vector3 dented = restVertices[i] + Vector3.down * dentDepths[i];
                vertices[i] = folding ? FoldVertex(dented) : dented;
            }

            mesh.vertices = vertices;

            // lighting comes from normals so dents would be invisible without this
            mesh.RecalculateNormals();
        }

        private void StartFold(float duration)
        {
            folding = true;
            foldDuration = duration;
            foldElapsed = 0f;
            foldForward = Quaternion.Euler(0f, -dough.Orientation, 0f) * Vector3.forward;
            foldAxis = Vector3.Cross(foldForward, Vector3.up);
            foldPivot = new Vector3(
                restBounds.center.x,
                restBounds.min.y + restBounds.size.y * flattenHeight,
                restBounds.center.z
            );
        }

        private void AdvanceFold()
        {
            if (!folding)
            {
                return;
            }

            foldElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(foldElapsed / foldDuration);

            foldFlatten = Mathf.SmoothStep(0f, 1f, t * 4f);
            foldAngle = 180f * Mathf.SmoothStep(0f, 1f, t * 4f - 1f);
            foldSettle = Mathf.SmoothStep(0f, 1f, t * 2f - 1f);

            if (t >= 1f)
            {
                folding = false;
            }
        }

        private Vector3 FoldVertex(Vector3 position)
        {
            Vector3 shaped = Vector3.Lerp(position, Flatten(position), foldFlatten);
            Vector3 offset = shaped - foldPivot;
            float distance = Vector3.Dot(offset, foldForward);

            if (distance > 0f)
            {
                float bend = Mathf.SmoothStep(0f, 1f, distance / hingeWidth);
                Quaternion rotation = Quaternion.AngleAxis(foldAngle * bend, foldAxis);
                shaped = foldPivot + rotation * offset;
            }

            return Vector3.Lerp(shaped, position, foldSettle);
        }

        private Vector3 Flatten(Vector3 position)
        {
            Vector3 center = restBounds.center;
            float along = Vector3.Dot(position - center, foldForward);
            Vector3 flat = position + foldForward * along * (flattenSpread - 1f);
            flat.y = restBounds.min.y + (position.y - restBounds.min.y) * flattenHeight;

            return flat;
        }

    }
}
