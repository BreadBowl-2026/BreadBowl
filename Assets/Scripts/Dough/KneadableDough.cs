using System.Collections;
using UnityEngine;

namespace BreadBowl.Dough
{
    public class KneadableDough : MonoBehaviour
    {
        [SerializeField] private KneadSettings settings;
        [SerializeField, Min(2)] private int resolution = 32;
        [SerializeField] private FlourPuff flourPuff;


        private KneadGrid grid;
        private int quarterTurns;
        private bool isFolding;

        public KneadSettings Settings => settings;
        public int QuarterTurn => quarterTurns;
        public float Orientation => quarterTurns * 90f;
        public bool IsFolding => isFolding;

        // adding using.System; breaks compilation because of Random.Range being ambiguous between
        // System and UnityEngine
        public event System.Action<Vector2, float> Pressed;
        public event System.Action<float> FoldStarted;
        public event System.Action GridChanged;
        

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
            // the +4 is necessary to prevent the player frame breaking after
            // a certain amount of CCW turns
            quarterTurns = ((quarterTurns + turns) % 4 + 4) %4;
        }

        public void Fold()
        {
            KneadFold.Apply(Grid, quarterTurns, settings.FoldBlend);
            GridChanged?.Invoke();

            // this needs to be null checked otherwise an empty flourPuff
            // breaks the data layer
            if (flourPuff != null)
            {
                flourPuff.Puff();
            }
        }

        public void PalmPress(Vector2 playerPosition, float radius, float amount, KneadBrushFalloff falloff)
        {
            Vector2 doughPosition = PlayerToDough(playerPosition);
            KneadBrush.Apply(Grid, doughPosition, radius, amount, falloff);
            Pressed?.Invoke(doughPosition, radius);
            GridChanged?.Invoke();
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
            GridChanged?.Invoke();
        }

        private void OnDisable()
        {
            isFolding = false;
        }

        public void BeginFold()
        {
            if (isFolding)
            {
                return;
            }

            StartCoroutine(FoldRoutine());
        }

        private IEnumerator FoldRoutine()
        {
            float half = settings.FoldDuration * 0.5f;

            isFolding = true;
            FoldStarted?.Invoke(settings.FoldDuration);
            
            yield return new WaitForSeconds(half);
            Fold();

            yield return new WaitForSeconds(half);
            isFolding = false;
        }

        /// <summary>
        /// Returns a copy of the grid cells.
        /// </summary>
        public float[] GetCells()
        {
            return Grid.GetCells();
        }

        /// <summary>
        /// Returns the resolution of the grid.
        /// </summary>
        public int GetResolution()
        {
            return resolution;
        }

        public KneadResult CreateResult()
        {
            return new KneadResult(this);
        }
    }
}

