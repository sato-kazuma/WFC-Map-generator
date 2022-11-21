using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{
    public class NeighbourStrategySizeDefault : IFindNeighbourStrategy
    {
        public Dictionary<int, PatternNeighbours> FindNeighbours(PatternDataResults patternFinderResults)
        {
            Dictionary<int, PatternNeighbours> result = new Dictionary<int, PatternNeighbours>();
            FindNeighbourForEachPattern(patternFinderResults, result);
            return result;
        }

        //Thid method will check every neighbour in each direction and it will add it to the Dictionary
        //Of course we will check if the patterns is already present, if it does they will be merged
        //Else we will create a new mapping for the neighbours
        private void FindNeighbourForEachPattern(PatternDataResults patternFinderResults, Dictionary<int, PatternNeighbours> result)
        {
            for (int row = 0; row < patternFinderResults.GetGridLenghtY(); row++)
            {
                for (int col = 0; col < patternFinderResults.GetGridLenghtX(); col++)
                {
                    PatternNeighbours neighbours = PatternFinder.CheckNeighboursInEachDirection(col, row, patternFinderResults);
                    PatternFinder.AddNeighboursToDictionary(result, patternFinderResults.GetIndexAt(col, row), neighbours);
                }
            }
        }
    }
}
