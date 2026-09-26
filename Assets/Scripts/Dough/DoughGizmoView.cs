using UnityEngine;

namespace BreadBowl.Dough
{
    [RequireComponent(typeof(KneadableDough))]
    public class DoughGizmoView : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float size = 0.1f;
        [SerializeField, Range(0.1f, 1f)] private float cellFill = 0.9f;
        [SerializeField] private Color32 underColor = new Color32(255, 99, 71, 255);
        [SerializeField] private Color32 wellColor = new Color32(34, 139, 34, 255);
        [SerializeField] private Color32 overColor = new Color32(218, 112, 214, 255);

        private void OnDrawGizmos()
        {
            KneadableDough dough = GetComponent<KneadableDough>();

            if (dough.Settings == null)
            {
                return;
            }

            KneadGrid grid = dough.Grid;
            float cellSize = size / grid.Resolution;
            float halfSize = size * 0.5f;
            Vector3 cellScale = new Vector3(cellSize * cellFill, cellSize * 0.1f, cellSize * cellFill);

            Gizmos.matrix = transform.localToWorldMatrix;

            for (int y = 0; y < grid.Resolution; y++)
            {
                for (int x = 0; x < grid.Resolution; x++)
                {
                    Vector3 center = new Vector3((x + 0.5f) * cellSize - halfSize, 0f, (y + 0.5f) * cellSize - halfSize);
                    Gizmos.color = ColorFor(dough.Settings.Classify(grid.Get(x, y)));
                    Gizmos.DrawCube(center, cellScale);
                }
            }
        }

        private Color32 ColorFor(KneadLevel level)
        {
            return level switch
            {
                KneadLevel.Well => wellColor,
                KneadLevel.Over => overColor,
                _ => underColor
            };
        }
    }
}

