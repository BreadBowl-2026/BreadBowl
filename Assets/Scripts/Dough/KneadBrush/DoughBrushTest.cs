using UnityEngine;

namespace BreadBowl.Dough
{
    [RequireComponent(typeof(KneadableDough))]
    public class DoughBrushTest : MonoBehaviour
    {
        [SerializeField] private Vector2 center = new Vector2(0.5f, 0.5f);
        [SerializeField, Range(0.01f, 1f)] private float radius = 0.2f;
        [SerializeField, Min(0f)] private float amount = 0.5f;
        [SerializeField] private KneadBrushFalloff falloff = KneadBrushFalloff.Smooth;

        [ContextMenu("Apply Brush")]
        private void ApplyBrush()
        {
            GetComponent<KneadableDough>().PalmPress(center, radius, amount, falloff);
        }
    }
}

