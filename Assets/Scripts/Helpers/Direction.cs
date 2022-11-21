using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{
    //this class will store direction, so we can find neighbours in the given direction
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    //this class will contain only a method that will return the opposite direction, so we can compare pattern opposite 
    //neighbour that we have
    public static class DirectionHelper
    {
        public static Direction GetOppositeDirectionTo(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                default:
                    return direction;
            }
        }
    }
}
