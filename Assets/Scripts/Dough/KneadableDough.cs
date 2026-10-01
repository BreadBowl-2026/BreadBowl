using UnityEngine;

namespace BreadBowl.Dough
{
    public class KneadableDough : MonoBehaviour
    {
        [SerializeField] private KneadSettings settings;
        [SerializeField, Min(2)] private int resolution = 32;

        private KneadGrid grid;
        private int quarterTurns;

        public KneadSettings Settings => settings;
        public int QuarterTurn => quarterTurns;
        public float Orientation => quarterTurns * 90f;

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

        public void Rotate(int turns)
        {
            quarterTurns = ((quarterTurns + turns) % 4) %4;
        }

        public void Fold()
        {
            KneadFold.Apply(Grid, quarterTurns, settings.FoldBlend);
        }

        public void PalmPress(Vector2 playerPosition, float radius, float amount, KneadBrushFalloff falloff)
        {
            KneadBrush.Apply(Grid, PlayerToDough(playerPosition), radius, amount, falloff);
        }

        public Vector2 PlayerToDough(Vector2 playerPosition)
        {
            float u = playerPosition.x;
            float v = playerPosition.y;

            return quarterTurns switch
            {
                1 => new Vector2(1f - v, u),
                2 => new Vector2(1f - u, 1f - v),
                3 => new Vector2(v, 1f - u),
                _ => playerPosition // 4
            };
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
            quarterTurns = 0;
        }
    }
}

