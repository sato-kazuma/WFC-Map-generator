using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{

    //We'll loop through each pattern in pattern results index dictionary
    //Each loop will go through each pattern and we'll compares these patterns together
    public class NeighbourStrategySize2andmore : IFindNeighbourStrategy
    {
        public Dictionary<int, PatternNeighbours> FindNeighbours(PatternDataResults patternFinderResult)
        {
            Dictionary<int, PatternNeighbours> result = new Dictionary<int, PatternNeighbours>();

            foreach (var patternDataToCheck in patternFinderResult.PatternIndexDictionary)
            {
                foreach (var possibleNeighbourForPattern in patternFinderResult.PatternIndexDictionary)
                {
                    FindNeighboursInAllDirections(result,patternDataToCheck,possibleNeighbourForPattern);
                }
            }
            return result;
        }

        //In this method we'll compare the pattern and check in all direction if they can be neighbours
        //If positive, it will add neighbours to those patterns that matches
        private void FindNeighboursInAllDirections(Dictionary<int, PatternNeighbours> result, KeyValuePair<int, PatternData> patternDataToCheck, KeyValuePair<int, PatternData> possibleNeighbourForPattern)
        {
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                if (patternDataToCheck.Value.CompareGrid(dir, possibleNeighbourForPattern.Value))
                {
                    if (result.ContainsKey(patternDataToCheck.Key) == false)
                    {
                        result.Add(patternDataToCheck.Key, new PatternNeighbours());
                    }
                    result[patternDataToCheck.Key].AddPatternToDictionary(dir, possibleNeighbourForPattern.Key);
                }
            }
        }
    }
}
