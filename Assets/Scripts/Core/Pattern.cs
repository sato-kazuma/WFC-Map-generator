using System;

namespace WaveFunctionCollapse
{
    //A square NxN block of value indices extracted from the input.
    //Values are stored flattened (index = y * Size + x), y = 0 is the bottom row.
    public sealed class Pattern
    {
        private readonly int[] values;

        public int Index { get; private set; }
        public int Size { get; private set; }
        public int Frequency { get; internal set; }

        public Pattern(int[] values, int size, int index)
        {
            if (values == null || values.Length != size * size)
            {
                throw new ArgumentException("WFC: pattern values must contain size*size elements");
            }
            this.values = values;
            Size = size;
            Index = index;
            Frequency = 1;
        }

        public int GetGridValue(int x, int y)
        {
            return values[y * Size + x];
        }

        //True if 'other', placed one cell away from this pattern in direction 'dir',
        //agrees with this pattern on every overlapping cell.
        //Only meaningful for Size > 1 (with Size == 1 there is no overlap, so it is always true).
        public bool CanOverlap(Direction dir, Pattern other)
        {
            int dx = dir.OffsetX();
            int dy = dir.OffsetY();
            int xMin = Math.Max(0, dx), xMax = Math.Min(Size, Size + dx);
            int yMin = Math.Max(0, dy), yMax = Math.Min(Size, Size + dy);

            for (int y = yMin; y < yMax; y++)
            {
                for (int x = xMin; x < xMax; x++)
                {
                    if (GetGridValue(x, y) != other.GetGridValue(x - dx, y - dy))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
