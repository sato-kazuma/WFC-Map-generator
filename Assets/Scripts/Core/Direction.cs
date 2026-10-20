namespace WaveFunctionCollapse
{
    //The four directions in which we look for neighbours.
    //Up = +y, Down = -y, Left = -x, Right = +x (row 0 is the bottom row, like a Unity tilemap).
    //Opposite directions differ only in the lowest bit, so the opposite is computed with a XOR.
    public enum Direction
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3
    }

    public static class DirectionHelper
    {
        public const int Count = 4;

        public static readonly Direction[] All = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        private static readonly int[] offsetX = { 0, 0, -1, 1 };
        private static readonly int[] offsetY = { 1, -1, 0, 0 };

        public static Direction GetOppositeDirectionTo(this Direction direction)
        {
            return (Direction)((int)direction ^ 1);
        }

        public static int OffsetX(this Direction direction)
        {
            return offsetX[(int)direction];
        }

        public static int OffsetY(this Direction direction)
        {
            return offsetY[(int)direction];
        }
    }
}
