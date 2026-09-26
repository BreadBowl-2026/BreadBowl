using UnityEngine;

namespace BreadBowl.Dough
{
    public class KneadableDough : MonoBehaviour
    {
        [SerializeField] private KneadSettings settings;
        [SerializeField, Min(2)] private int resolution = 32;

        private KneadGrid grid;

        public KneadSettings Settings => settings;

        public KneadGrid Grid
        {
            get
            {
                // if the grid resolution is changed in the editor, this ensures
                // that a new grid is created of the right size instead of
                // creating an out of range issue
                if (grid == null || grid.Resolution != resolution)
                {
                    grid = new KneadGrid(resolution);
                }

                return grid;
            }
        }

        [ContextMenu("Randomize Kneadedness")]
        private void RandomizeKneadedness()
        {
            if (settings == null)
            {
                Debug.LogWarning("Assign knead settings before trying to randomize", this);
                return;
            }

            KneadGrid target = Grid;
            float max = settings.OverKneadedThreshold * 1.5f;

            for (int y = 0; y < target.Resolution; y++)
            {
                for (int x = 0; x < target.Resolution; x++)
                {
                    target.Set(x, y, Random.Range(0f, max));
                }
            }
        }

        [ContextMenu("Reset Knead")]
        private void ResetKnead()
        {
            grid = new KneadGrid(resolution);
        }
    }
}

