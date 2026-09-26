using System;
using UnityEngine;

namespace BreadBowl.Dough
{
    /*
    KneadGrid represents the grid where force will be applied to the dough during kneading.
    The resolution is the x and y dimensions of the grid and each cell will store a value
    that represents the amount of "kneadedness" on that section of the dough. 

    Using a flat array instead of 2D array so that unity can serialize the dough state
    which will be helpful for handing off to the bake stage.
    */
    [Serializable]
    public class KneadGrid
    {
        [SerializeField] private int resolution;
        // 2D array would be simpler to read, but unity can't serialize a 2D array
        [SerializeField] private float[] cells;

        public int Resolution => resolution;

        public KneadGrid(int resolution)
        {
            if (resolution < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(resolution), 
                "Resolution must be at least 2 so it can fold");
            }

            this.resolution = resolution;
            cells = new float[resolution * resolution];
        }

        public float Get(int x, int y)
        {
            return cells[IndexOf(x, y)];
        }

        public void Set(int x, int y, float value)
        {
            cells[IndexOf(x, y)] = Math.Max(0f, value);
        }

        // helper method to prevent out of bounds errors that would fail silently
        private int IndexOf(int x, int y)
        {
            if (x < 0 || x >= resolution)
            {
                throw new ArgumentOutOfRangeException(nameof(x), x,
                $"x must be between 0 and {resolution - 1}");
            }

            if (y < 0 || y >= resolution)
            {
                throw new ArgumentOutOfRangeException(nameof(x), x,
                $"y must be between 0 and {resolution - 1}");
            }

            return y * resolution + x;
        }
    }
}
