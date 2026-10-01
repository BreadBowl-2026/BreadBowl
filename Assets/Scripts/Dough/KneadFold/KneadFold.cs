using UnityEngine;

namespace BreadBowl.Dough
{
    public static class KneadFold
    {
        public static void Apply(KneadGrid grid, int quarterTurns, float blend)
        {
            if (quarterTurns % 2 == 0)
            {
                MirrorRows(grid, blend);
            }
            else // quarterTurns is odd
            {
                MirrorColumns(grid, blend);
            }
        }

        private static void MirrorRows(KneadGrid grid, float blend)
        {
            int last = grid.Resolution - 1;

            for (int y = 0; y < grid.Resolution / 2; y++)
            {
                for (int x = 0; x < grid.Resolution; x++)
                {
                    BlendPair(grid, x, y, x, last - y, blend);
                }
            }
        }

        private static void MirrorColumns(KneadGrid grid, float blend)
        {
            int last = grid.Resolution - 1;

            for (int x = 0; x < grid.Resolution / 2; x++)
            {
                for (int y = 0; y < grid.Resolution; y++)
                {
                    BlendPair(grid, x, y, last - x, y, blend);
                }
            }
        }

        private static void BlendPair(KneadGrid grid, int x, int y, int mirrorX, int mirrorY, float blend)
        {
            float value = grid.Get(x, y);
            float mirrorValue = grid.Get(mirrorX, mirrorY);
            float average = (value + mirrorValue) * 0.5f;

            grid.Set(x, y, Mathf.Lerp(value, average, blend));
            grid.Set(mirrorX, mirrorY, Mathf.Lerp(mirrorValue, average, blend));
        }
    }
}

