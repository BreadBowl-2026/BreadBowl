using System;
using UnityEngine;

namespace BreadBowl.Dough
{
    /// <summary>
    /// KneadGrid represents the grid where force will be applied to the dough during kneading.
    /// The resolution is the number of cells on each side of thegrid and each cell will store 
    /// a value that represents the amount of "kneadedness" on that section of the dough. 
    /// The grid is always square, different dough shapes could be mapped to fit within
    /// the grid to accommodate the different bread style we want to implement over time.
    ///
    /// Using a flat array instead of 2D array so that Unity can serialize the dough state
    /// which will be helpful for handing off to the bake stage.
    ///
    /// x runs left -> right across the dough, y runs near -> far. The rest of the dough scripts
    /// rely on this orientation.
    /// </summary>
    [Serializable]
    public class KneadGrid
    {
        [SerializeField] private int resolution;
        // 2D array would be simpler to read, but Unity can't serialize a 2D array
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

        /// <summary>
        /// Gets the value stored in a cell of the 2D grid.
        /// Uses IndexOf to interpret the value from the flat array.
        /// </summary>
        public float Get(int x, int y)
        {
            return cells[IndexOf(x, y)];
        }

        /// <summary>
        /// Clamps to 0 and positive values since kneadedness should never be negative
        /// </summary>
        public void Set(int x, int y, float value)
        {
            cells[IndexOf(x, y)] = Math.Max(0f, value);
        }

        /// <summary>
        /// The cells array is flat, but represents a 2D grid. To get to P(3,2) 
        /// in a 32x32 grid, you need to go 2 * 32 + 3 = 67 to access the right value.
        /// Bounds checks for each dimension prevent silent wrap around to the next 
        /// row that the formula itself can't account for. 
        /// </summary>
        private int IndexOf(int x, int y)
        {
            if (x < 0 || x >= resolution)
            {
                throw new ArgumentOutOfRangeException(nameof(x), x,
                    $"x must be between 0 and {resolution - 1}");
            }

            if (y < 0 || y >= resolution)
            {
                throw new ArgumentOutOfRangeException(nameof(y), y,
                    $"y must be between 0 and {resolution - 1}");
            }

            return y * resolution + x;
        }
    }
}
