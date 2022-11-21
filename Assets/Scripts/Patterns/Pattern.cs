using Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{
    //will store a grid of integers that contains the value indices that the pattern represents
    //for patternsize =1, it will be a single value stored in a jagged array of 1
    public class Pattern
    {
        private int _index;
        private int[][] _grid;
        public string HashIndex { get; set; }
        public int Index { get => _index; }
        public Pattern(int[][] grid, string hashCode, int index)
        {
            _grid = grid;
            HashIndex = hashCode;
            _index = index;
        }

        //This method will take coord and value as integers and set it to grid
        public void SetGridValue(int x,int y,int value)
        {
        //Remember that y is row,x is colums
            _grid[y][x] = value;
        }
        //This method returns the value from the grid
        public int GetGridValue(int x,int y)
        {
            return _grid[y][x];
        }
        //This method check value at positions, it will compare values
        public bool CheckValueAtPosition(int x,int y,int value)
        {
            return value.Equals(GetGridValue(x, y));
        }
        //it will compare pattern to another pattern( we check if they can be neighbour)
        internal bool ComparePatternToAnotherPattern(Direction dir,Pattern pattern)
        {
            //The first grid is created using direction method
            int[][] myGrid = GetGridValueInDirection(dir);
            //The second grid is created using direction method, but in the opposite direction
            int[][] otherGrid = pattern.GetGridValueInDirection(dir.GetOppositeDirectionTo());
            //If the 2 grid can overlap, then we return true
            for (int row = 0; row < myGrid.Length; row++)
            {
                for (int col = 0; col < myGrid[0].Length; col++)
                {
                    if (myGrid[row][col] != otherGrid[row][col])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //this method is responsible for accessing the correct part of our value grid
        //depending on which direction we want to compare
        //We will create a new jagged array that is of the correct size and use it for
        //selecting the correct part of our value grid
        private int[][] GetGridValueInDirection(Direction dir)
        {
            int[][] gridPartToCompare;

            switch (dir)
            {
                /*
                Remember that our patterns are always square, so we can use only one lenght
                Because we always leave one row unchecked and take all the other row for comparison
                We remove 1 from the proper place, depending on the direction we are working towards
                */
                case Direction.Up:
                    gridPartToCompare = MyCollectionExtension.CreateJaggedArray<int[][]>(_grid.Length - 1, _grid.Length);
                    CreatePartOfGrid(0, _grid.Length, 1, _grid.Length, gridPartToCompare);
                    break;
                case Direction.Down:
                    gridPartToCompare = MyCollectionExtension.CreateJaggedArray<int[][]>(_grid.Length - 1, _grid.Length);
                    CreatePartOfGrid(0, _grid.Length, 0, _grid.Length - 1, gridPartToCompare);
                    break;
                case Direction.Left:
                    gridPartToCompare = MyCollectionExtension.CreateJaggedArray<int[][]>(_grid.Length, _grid.Length - 1);
                    CreatePartOfGrid(0, _grid.Length - 1, 0, _grid.Length, gridPartToCompare);
                    break;
                case Direction.Right:
                    gridPartToCompare = MyCollectionExtension.CreateJaggedArray<int[][]>(_grid.Length, _grid.Length - 1);
                    CreatePartOfGrid(1, _grid.Length, 0, _grid.Length, gridPartToCompare);
                    break;
                default:
                    return _grid;
            }
            return gridPartToCompare;
        }

        private void CreatePartOfGrid(int xmin, int xmax, int ymin, int ymax, int[][] gridPartToCompare)
        {
            //We need to create a list of integers in which to store the values
            List<int> tempList = new List<int>();
            for (int row = ymin; row < ymax; row++)
            {
                for (int col = xmin; col < xmax; col++)
                {
                    tempList.Add(_grid[row][col]);
                }
            }

            //Now we loop through this list
            for (int i = 0; i < tempList.Count; i++)
            {
                //We can recreate x and y coordinates
                int x = i % gridPartToCompare.Length;
                int y = i / gridPartToCompare.Length;
                //Fill the grid
                gridPartToCompare[x][y] = tempList[i];
            }


        }
    }
}

