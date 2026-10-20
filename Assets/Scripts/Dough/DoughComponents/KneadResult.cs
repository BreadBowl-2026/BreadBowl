using System;
using UnityEngine;

namespace BreadBowl.Dough
{
    [Serializable]
    public class KneadResult
    {
        [SerializeField] private KneadSettings settings;
        [SerializeField] private int resolution;
        [SerializeField] private float[] cells;
        [SerializeField] private bool[] inside;
        [SerializeField] private float underFraction;
        [SerializeField] private float wellFraction;
        [SerializeField] private float overFraction;

        public KneadSettings Settings => settings;
        public int Resolution => resolution;
        public float UnderFraction => underFraction;
        public float WellFraction => wellFraction;
        public float OverFraction => overFraction;

        public KneadResult(KneadableDough dough)
        {
            KneadGrid grid = dough.Grid;
            settings = dough.Settings;
            resolution = grid.Resolution;
            cells = new float[resolution * resolution];
            inside = new bool[resolution * resolution];

            Vector2 center = new Vector2(0.5f, 0.5f);
            int insideCount = 0;
            int under = 0;
            int well = 0;
            int over = 0;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector2 playerUV = new Vector2((x + 0.5f) / resolution, (y + 0.5f) / resolution);
                    Vector2 doughUV = dough.PlayerToDough(playerUV);
                    int doughX = Mathf.Min(Mathf.FloorToInt(doughUV.x * resolution), resolution - 1);
                    int doughY = Mathf.Min(Mathf.FloorToInt(doughUV.y * resolution), resolution - 1);

                    int index = y * resolution + x;
                    cells[index] = grid.Get(doughX, doughY);
                    inside[index] = (playerUV - center).sqrMagnitude <= 0.25f;

                    if (!inside[index])
                    {
                        continue;
                    }

                    insideCount++;

                    switch (settings.Classify(cells[index]))
                    {
                        case KneadLevel.Over:
                            over++;
                            break;
                        case KneadLevel.Well:
                            well++;
                            break;
                        default: // KneadLevel.Under
                            under++;
                            break;
                    }
                }
            }

            underFraction = (float)under / insideCount;
            wellFraction = (float)well / insideCount;
            overFraction = (float)over / insideCount;
        }

        public float Get(int x, int y)
        {
            return cells[y * resolution + x];
        }

        public bool IsInside(int x, int y)
        {
            return inside[y * resolution + x];
        }

        /// <summmary>
        /// A cell's value is at the center (i + 0.5) / resolution, not at a corner. To convert
        /// uv -> grid space, multiply by the resolution and subtract 0.5f. This function takes
        /// any point in the data grid from the player frame (u goes left -> right and v goes
        /// near -> far) and returns kneadedness blended from the 4 nearest cell centers. This
        /// prevents the load rising in the resolution grid in the bake step.
        /// ┌───────────────┬───────────────┐
        /// │               │               │
        /// │   (leftX,farY)│  (rightX,farY)│
        /// │       ●·······│·······●       │
        /// │       ·       │       ·       │
        /// ├───────·───────┼───────·───────┤
        /// │       ·    ×  │       ·       │
        /// │       ●·······│·······●       │
        /// │  (leftX,nearY)│ (rightX,nearY)│
        /// │               │               │
        /// └───────────────┴───────────────┘
        /// The caller asks for a knead value at a vertex. This provides that knead value by 
        /// blending the discrete knead values of the 4 surrounding cells depending on where 
        /// that vertex (uv point) lands on the grid. It could be any place between any 4
        /// cells.
        public float Sample(Vector2 uv)
        {
            float gridX = Mathf.Clamp(uv.x * resolution - 0.5f, 0f, resolution - 1);
            float gridY = Mathf.Clamp(uv.y * resolution - 0.5f, 0f, resolution - 1);

            int leftX = Mathf.Min(Mathf.FloorToInt(gridX), resolution - 2);
            int nearY = Mathf.Min(Mathf.FloorToInt(gridY), resolution - 2);
            int rightX = leftX + 1;
            int farY = nearY + 1;

            float blendX = gridX - leftX;
            float blendY = gridY - nearY;

            float nearRow = Mathf.Lerp(Get(leftX, nearY), Get(rightX, nearY), blendX);
            float farRow = Mathf.Lerp(Get(leftX, farY), Get(rightX, farY), blendX);

            return Mathf.Lerp(nearRow, farRow, blendY);
        }
    }
}