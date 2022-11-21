using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WaveFunctionCollapse
{
    public class PatternManager 
    {
        //will store pattern data, the information about patterns
        Dictionary<int, PatternData> patternDataIndexDictionary;
        //contains data about neighbours for the given pattern
        Dictionary<int, PatternNeighbours> patternPossibleNeighboursDictionary;
        //we will store patternsize in this class, as it will be passed by the user
        int _patternSize = -1;
        //this is an abstaction of a strategy for finding neighbours
        //we discussed two strategy: searching a grid(patternsize= 1) and comparing pattern values
        //that can overlap(patternsize>1)
        IFindNeighbourStrategy strategy;

        public PatternManager(int patternsize)
        {
            _patternSize = patternsize;
        }

        //We will create a generic method.It takes a generic valuesmanager class 
        //Pull equal weights and string strategy name
        //This method will be called at the start , to process the valuemanager data
        //We will decide what strategy to use
        public void ProcessGrid<T>(ValuesManager<T> valuesManager,bool equalWeights,string strategyName=null)
        {
            NeighbourStrategyFactory stategyFactory = new NeighbourStrategyFactory();
            //If null return patternsize+ "", else strategyname
            strategy = stategyFactory.CreateInstances(strategyName == null ? _patternSize + "" : strategyName);
            CreatePatterns(valuesManager,strategy,equalWeights);
        }

        private void CreatePatterns<T>(ValuesManager<T> valueManager, IFindNeighbourStrategy strategy, bool equalWeights)
        {
            var patternFinderResult = PatternFinder.GetPatternDataFromGrid(valueManager, _patternSize, equalWeights);

            /*this is some testing code*/
            //*******************************************************************************************************
            //*******************************************************************************************************
            StringBuilder builder = null;
            List<string> list = new List<string>();
            for (int row = 0; row < patternFinderResult.GetGridLenghtY(); row++)
            {
                builder = new StringBuilder();
                for (int col = 0; col < patternFinderResult.GetGridLenghtX(); col++)
                {
                    builder.Append(patternFinderResult.GetIndexAt(col, row) + " ");
                }
                list.Add(builder.ToString());
            }
            list.Reverse();
            foreach (var item in list)
            {
                Debug.Log(item);
            }
            //*******************************************************************************************************
            //*******************************************************************************************************
            /*here finish the testing code*/

            //Here we'll retrieve our pattern index dictionary, our pattern will be already mapped to the indices
            patternDataIndexDictionary = patternFinderResult.PatternIndexDictionary;
            //Finding neighbours
            GetPatternNeighbours(patternFinderResult,strategy);
        }
        //It will return our neighbour dictionary
        private void GetPatternNeighbours(PatternDataResults patternFinderResult, IFindNeighbourStrategy strategy)
        {
            patternPossibleNeighboursDictionary = PatternFinder.FindPossibleNeighboursForAllPatterns(strategy, patternFinderResult);
        }

        public PatternData GetPatternDataFromIndex(int index)
        {
            return patternDataIndexDictionary[index];
        }

        public HashSet<int> GetPossibleNeighboursForPatternInDirection(int patternIndex,Direction dir)
        {
            return patternPossibleNeighboursDictionary[patternIndex].GetNeighboursInDirection(dir);
        }

        public float GetPatternFrequency(int index)
        {
            return GetPatternDataFromIndex(index).FrequencyRelative;
        }

        public float GetPatternFrequencyLog2(int index)
        {
            return GetPatternDataFromIndex(index).FrequencyRelativeLog2;
        }

        public int GetNumberOfPattern()
        {
            return patternDataIndexDictionary.Count;
        }

    }
}
