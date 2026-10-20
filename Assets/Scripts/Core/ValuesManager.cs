using System;
using System.Collections.Generic;

namespace WaveFunctionCollapse
{
    //Converts a grid of generic values (e.g. TileBase) into a grid of integer indices.
    //Every distinct value gets a unique index, so the rest of the algorithm works only with ints.
    //Grid layout is [row][col] with row 0 at the bottom.
    public class ValuesManager<T>
    {
        private readonly int[][] grid;
        private readonly List<T> values = new List<T>();

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int ValueCount { get { return values.Count; } }

        public ValuesManager(T[][] gridOfValues, IEqualityComparer<T> comparer = null)
        {
            if (gridOfValues == null || gridOfValues.Length == 0 || gridOfValues[0] == null || gridOfValues[0].Length == 0)
            {
                throw new ArgumentException("WFC: input grid is empty");
            }

            Height = gridOfValues.Length;
            Width = gridOfValues[0].Length;

            //Value -> index lookup is O(1) instead of scanning the dictionary values for each cell
            var indexOfValue = new Dictionary<T, int>(comparer ?? EqualityComparer<T>.Default);
            grid = new int[Height][];

            for (int row = 0; row < Height; row++)
            {
                if (gridOfValues[row] == null || gridOfValues[row].Length != Width)
                {
                    throw new ArgumentException("WFC: input grid must be rectangular");
                }
                grid[row] = new int[Width];
                for (int col = 0; col < Width; col++)
                {
                    T value = gridOfValues[row][col];
                    int index;
                    if (indexOfValue.TryGetValue(value, out index) == false)
                    {
                        index = values.Count;
                        indexOfValue.Add(value, index);
                        values.Add(value);
                    }
                    grid[row][col] = index;
                }
            }
        }

        public int GetGridValue(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new IndexOutOfRangeException("Grid doesn't contain x: " + x + " y: " + y);
            }
            return grid[y][x];
        }

        //Treats the input as periodic (toroidal): any coordinate, even far outside the grid, wraps around
        public int GetGridValueWrapped(int x, int y)
        {
            return grid[Mod(y, Height)][Mod(x, Width)];
        }

        public T GetValueFromIndex(int index)
        {
            if (index < 0 || index >= values.Count)
            {
                throw new ArgumentOutOfRangeException("index", "No index " + index + " in values");
            }
            return values[index];
        }

        private static int Mod(int a, int n)
        {
            int r = a % n;
            return r < 0 ? r + n : r;
        }
    }
}
