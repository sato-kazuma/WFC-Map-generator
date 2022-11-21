using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaveFunctionCollapse
{
    //The grid will rapresent  our tile map as integer values that are
    //assigned to an Ivalue object, and dictionary will store the mapping
    //so we can retrieve the value from our index
    public class ValuesManager<T>
    {
        //to store map values, we will use jagged array
        int[][] _grid;
        //we also need a dictionary that will keep the mapping for every value
        //and will asign an unique integer value
        Dictionary<int, IValue<T>> valueIndexDictionary = new Dictionary<int, IValue<T>>();
        //index integer value, to assign a new key value for our Ivalue index dictionary
        int index = 0;

        public ValuesManager(IValue<T>[][] gridOfValues)
        {
            CreateGridOfIndices(gridOfValues);
        }

        internal Vector2 GetGridSize()
        {
            if (_grid == null)
            {
                return Vector2.zero;
            }
            return new Vector2(_grid[0].Length, _grid.Length);
        }

        private void CreateGridOfIndices(IValue<T>[][] gridOfValues)
        {
            //assigning the grid to a new jagged array with row and col
            _grid = MyCollectionExtension.CreateJaggedArray<int[][]>(gridOfValues.Length, gridOfValues[0].Length);
            //loop through and assign indices to each unique value
            for (int row = 0; row < gridOfValues.Length; row++)
            {
                for (int col = 0; col < gridOfValues[0].Length; col++)
                {
                    SetIndexToGridPosition(gridOfValues, row, col);
                }
            }
        }

        private void SetIndexToGridPosition(IValue<T>[][] gridOfValues, int row, int col)
        {
            //we will check dictionary(if it contains the same value as our ivalue) , this is possible because of equitable implementation
            //if our dictionary contains the same value, we'll assign the index that already exist otherwise we create a new index and assign
            //and assign it to our grid and to our high value instance
            if (valueIndexDictionary.ContainsValue(gridOfValues[row][col]))
            {
                var key = valueIndexDictionary.FirstOrDefault(x => x.Value.Equals(gridOfValues[row][col]));
                //we assign the key to the grid
                _grid[row][col] = key.Key;
            }
            else//we will create a new index, adding it to the dictionary and increase the index
            {
                _grid[row][col] = index;
                valueIndexDictionary.Add(_grid[row][col], gridOfValues[row][col]);
                index++;
            }
        }

        //we are using coordinates, but it's the same with col and row
        public int GetGridValue(int x, int y)
        {
            //check if the parameters are inside the grid
            //col->x,row->y
            if (x >= _grid[0].Length || y >= _grid.Length || x < 0 || y < 0)
            {
                throw new System.IndexOutOfRangeException("Grid doeant contain x: " + x + " y: " + y + " value");
            }
            return _grid[y][x];
        }

        //this method will get value from index
        public IValue<T> GetValueFromIndex(int index)
        {
            if (valueIndexDictionary.ContainsKey(index))
            {
                return valueIndexDictionary[index];
            }
            throw new System.Exception("No index " + index + " in valueDictionary");
        }

        public int GetGridValuesIncludingOffset(int x, int y)
        {
            int yMax = _grid.Length;
            int xMax = _grid[0].Length;
            //if we are negative, we will add it to the max size, so we will be back to what we need-> -1,-1 in grid(3*3)-> 3(+1 because of difference height and indices)-1=3->last col and row->what we needed
            if (x < 0 && y < 0)
            {
                return GetGridValue(xMax + x, yMax + y);
            }
            //y-ymax->if we have same size, we get zero otherwise we get 0 + difference-> the number of row we need
            if (x < 0 && y >= yMax)
            {
                return GetGridValue(xMax + x, y - yMax);
            }
            if (x >= xMax && y < 0)
            {
                return GetGridValue(x - xMax, yMax + y);
            }
            if (x >= xMax && y >= yMax)
            {
                return GetGridValue(x - xMax, y - yMax);
            }

            if (x < 0)
            {
                return GetGridValue(xMax + x, y);
            }
            if (x >= xMax)
            {
                return GetGridValue(x - xMax, y);
            }
            if (y < 0)
            {
                return GetGridValue(x, yMax + y);
            }

            if (y >= yMax)
            {
                return GetGridValue(x, y - yMax);
            }
            return GetGridValue(x, y);

        }

        //pattern bigger than size one
        //we assume x and y are bottom left corner, from there we gave the pattern to right and up, so we will pass 2D
        //passed parameters x col and y row and get the number of pattern the patternsize is
        public int[][] GetPatternValuesFromGridAt(int x, int y, int patternSize)
        {
            int[][] arrayToReturn = MyCollectionExtension.CreateJaggedArray<int[][]>(patternSize, patternSize);
            for (int row = 0; row < patternSize; row++)
            {
                for (int col = 0; col < patternSize; col++)
                {
                    arrayToReturn[row][col] = GetGridValuesIncludingOffset(x + col, y + row);
                }
            }

            return arrayToReturn;
        }
    }

}