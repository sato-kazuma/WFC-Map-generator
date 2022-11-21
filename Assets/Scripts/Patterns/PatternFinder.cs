using Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{
    //This class will have logic to find patterns and use strategies for
    //finding neighbors for all the patterns that have been found before
    public static class PatternFinder
    {
        //To find patterns we will calculate their hash code, add them to the pattern hash code dictionary
        //and add those to the pattern index dictionary
        internal static PatternDataResults GetPatternDataFromGrid<T>(ValuesManager<T> valuesManager, int patternSize, bool equalWeights)
        {
            Dictionary<string, PatternData> patternHashCodeDictionary=new Dictionary<string, PatternData>();
            Dictionary<int, PatternData> patternIndexDictionary=new Dictionary<int, PatternData>();

            Vector2 sizeOfgrid = valuesManager.GetGridSize();

            int patternGridSizeX = 0, patternGridSizeY = 0;

            int rowMin = -1, rowMax = -1, colMin = -1, colMax = -1;

            if(patternSize < 3)
            {
                //This calculations are valid only for pattern 1 and 2
                patternGridSizeX = (int)sizeOfgrid.x + 3 - patternSize;
                patternGridSizeY = (int)sizeOfgrid.y + 3 - patternSize;
                rowMax = patternGridSizeY - 1;
                colMax = patternGridSizeX - 1;
            }
            else
            {
                patternGridSizeX = (int)sizeOfgrid.x+ patternSize - 1;
                patternGridSizeY = (int)sizeOfgrid.y + patternSize - 1;
                rowMin = 1 - patternSize;
                colMin = 1 - patternSize;
                rowMax = (int)sizeOfgrid.y;
                colMax = (int)sizeOfgrid.x;
            }

            int[][] patternIndicesGrid = MyCollectionExtension.CreateJaggedArray<int[][]>(patternGridSizeY, patternGridSizeX);
            int totalFrequency = 0,patternIndex = 0;

            //We are looping through each possible pattern and we have taken the values from the offset and
            //the original value grid, and we will create the patterns based on the grid value
            //Patternsize=1->one value else jagged array of values
            for (int row = rowMin; row < rowMax; row++)
            {
                for (int col = colMin; col < colMax; col++)
                {
                    //This grid contains the value extractedfrom value manager depending on patternsize
                    int[][] gridValues = valuesManager.GetPatternValuesFromGridAt(col, row, patternSize);
                    //We are taking our grid of integers, converting them to byte array
                    //computing the hash code from it, and using the hash code to check
                    //if we have this value array inside the dictionary
                    //we use hashvalue so we don't have the same pattern twice
                    string hashValue = HashCodeCalculator.CalculateHashCode(gridValues);

                    if (patternHashCodeDictionary.ContainsKey(hashValue) == false)
                    {
                        Pattern pattern = new Pattern(gridValues, hashValue, patternIndex);
                        patternIndex++;
                        AddNewPattern(patternHashCodeDictionary, patternIndexDictionary, hashValue, pattern);
                    }
                    else
                    {
                        if (equalWeights == false)
                        {
                            //we are adding the key and then calling addtoFrequency
                            patternIndexDictionary[patternHashCodeDictionary[hashValue].Pattern.Index].AddToFrequency();
                        }
                    }
                    totalFrequency++;
                    if (patternSize < 3)
                    {
                        patternIndicesGrid[row + 1][colMax + 1] = patternHashCodeDictionary[hashValue].Pattern.Index;
                    }
                    else
                    {
                        patternIndicesGrid[row + patternSize - 1][col + patternSize - 1] = patternHashCodeDictionary[hashValue].Pattern.Index;
                    }
                }
            }

            CalculateRelativeFrequency(patternIndexDictionary, totalFrequency);
            return new PatternDataResults(patternIndicesGrid, patternIndexDictionary);

        }

        private static void CalculateRelativeFrequency(Dictionary<int, PatternData> patternIndexDictionary, int totalFrequency)
        {
            foreach (var item in patternIndexDictionary.Values)
            {
                item.CalculateRelativeFrequency(totalFrequency);
            }
        }

        private static void AddNewPattern(Dictionary<string, PatternData> patternHashCodeDictionary, Dictionary<int, PatternData> patternIndexDictionary, string hashValue, Pattern pattern)
        {
            PatternData patternData = new PatternData(pattern);
            patternHashCodeDictionary.Add(hashValue, patternData);
            patternIndexDictionary.Add(pattern.Index, patternData);
        }

        internal static Dictionary<int, PatternNeighbours> FindPossibleNeighboursForAllPatterns(IFindNeighbourStrategy patternFinder, PatternDataResults patternDataResult)
        {
            return patternFinder.FindNeighbours(patternDataResult);
        }

        public static PatternNeighbours CheckNeighboursInEachDirection(int x,int y, PatternDataResults patternDataResults)
        {
            PatternNeighbours patternNeighbours = new PatternNeighbours();
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                int PossiblePatternIndex = patternDataResults.GetNeighbourInDirection(x, y, dir);
                if (PossiblePatternIndex >= 0)
                {
                    patternNeighbours.AddPatternToDictionary(dir, PossiblePatternIndex);
                }
            }
            return patternNeighbours;
        }

        public static void AddNeighboursToDictionary(Dictionary<int,PatternNeighbours> dictionary,int patternIndex,PatternNeighbours neighbours)
        {
            if(dictionary.ContainsKey(patternIndex)== false)
            {
                dictionary.Add(patternIndex, neighbours);
            }
            dictionary[patternIndex].AddNeighbour(neighbours);
        }

        public static bool AreArraysTheSame(int[][] arr1, int[][] arr2)
        {
            string arr1hash = HashCodeCalculator.CalculateHashCode(arr1);
            string arr2hash = HashCodeCalculator.CalculateHashCode(arr2);
            return arr1hash.Equals(arr2hash);

        }

    }
}
