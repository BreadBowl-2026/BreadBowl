using UnityEngine;

namespace BreadBowl.Dough
{
    public static class KneadBrush
    {
        // defines the center point on the grid where kneadedness will be applied
        // as well as how far from the center and which type of falloff
        public static void Apply(
            KneadGrid grid,
            Vector2 center,
            float radius,
            float amount,
            KneadBrushFalloff falloff
        ){
            if (radius <= 0f || amount <= 0f)
            {
                return;
            }

            int resolution = grid.Resolution;
            int minX = Mathf.Max(0, Mathf.FloorToInt((center.x - radius) * resolution));
            int maxX = Mathf.Min(resolution - 1, Mathf.FloorToInt((center.x + radius) * resolution));
            int minY = Mathf.Max(0, Mathf.FloorToInt((center.y - radius) * resolution));
            int maxY = Mathf.Min(resolution - 1, Mathf.FloorToInt((center.y + radius) * resolution));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 cellCenter = new Vector2((x + 0.5f) / resolution, (y + 0.5f) / resolution);
                    float distance = Vector2.Distance(cellCenter, center);

                    if (distance >= radius)
                    {
                        continue;
                    }


                    float weight = Weight(distance / radius, falloff);
                    grid.Set(x, y, grid.Get(x, y) + amount * weight);
                }
            }
        }

        private static float Weight(float t, KneadBrushFalloff falloff)
        {
            return falloff switch
            {
                KneadBrushFalloff.Linear => 1f - t,
                KneadBrushFalloff.Smooth => Mathf.SmoothStep(1f, 0f, t),
                _ => 1f //KneadBrushFalloff.Hard
            };
        }
    }
}