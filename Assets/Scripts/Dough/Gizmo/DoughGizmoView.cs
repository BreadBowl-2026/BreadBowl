using UnityEngine;

namespace BreadBowl.Dough
{
    /// <summary>
    /// Uses Unity Gizmos to create an editor only debug view to visualize the data 
    /// layer for KneadableDough. Colors each cell in a 2D grid according to its
    /// KneadLevel. 
    /// </summary>
    [RequireComponent(typeof(KneadableDough))]
    public class DoughGizmoView : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float gridWidth = 1f;
        [SerializeField, Range(0.1f, 1f)] private float cellFill = 0.9f;
        [SerializeField, Range(0f, 1f)] private float minBrightness = 0.35f;
        [SerializeField] private Color32 underColor = new Color32(255, 99, 71, 255);
        [SerializeField] private Color32 wellColor = new Color32(34, 139, 34, 255);
        [SerializeField] private Color32 overColor = new Color32(218, 112, 214, 255);

        private const float CellHeightRatio = 0.1f;

        private void OnDrawGizmos()
        {
            KneadableDough dough = GetComponent<KneadableDough>();

            if (dough.Settings == null)
            {
                return;
            }

            KneadGrid grid = dough.Grid;
            float cellWidth = gridWidth / grid.Resolution;    // width of one cell
            float halfCell = cellWidth * 0.5f;                // half step of cell width point at its middle
            float gridStart = -gridWidth * 0.5f;              // offset to center grid on the object
            Vector3 cubeSize = new Vector3(                   // size of each cell. shrunk by cellFill to create the gaps
                cellWidth * cellFill,                         // x
                cellWidth * CellHeightRatio,                  // y
                cellWidth * cellFill);                        // z

            Quaternion doughRotation = Quaternion.Euler(0f, dough.Orientation, 0f);
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Rotate(doughRotation);

            // each cell needs to be drawn according to a center point in Unity's space. 
            // the grid is centered on the object, so its left edge is half the grid
            // width to the left. grid Y maps to local Z
            for (int y = 0; y < grid.Resolution; y++)
            {
                for (int x = 0; x < grid.Resolution; x++)
                {
                    float localX = gridStart + x * cellWidth + halfCell;
                    float localZ = gridStart + y * cellWidth + halfCell;
                    Vector3 cubeCenter = new Vector3(localX, 0f, localZ);

                    Gizmos.color = ColorFor(dough.Settings, grid.Get(x, y));
                    Gizmos.DrawCube(cubeCenter, cubeSize);
                }
            }

            Vector3 markerStart = new Vector3(0f, cellWidth, 0f);
            Vector3 markerEnd = new Vector3(0f, cellWidth, gridWidth * 0.5f);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(markerStart, markerEnd);
        }

        /// <summary>
        /// Returns a color with brightness adjusted to its current level of 
        /// kneadedness with its current knead level. Creates the gradient
        /// effect that shows progress from the beginning of one knead 
        /// stage to the end.
        /// </summary>
        private Color32 ColorFor(KneadSettings settings, float kneadedness)
        {
            Color levelColor = settings.Classify(kneadedness) switch
            {
                KneadLevel.Well => wellColor,
                KneadLevel.Over => overColor,
                _ => underColor // KneadLevel.Under
            };

            Color dimColor = Color.Lerp(Color.black, levelColor, minBrightness);
            return Color.Lerp(dimColor, levelColor, settings.KneadLevelProgress(kneadedness));
        }
    }
}

