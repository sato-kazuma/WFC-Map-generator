using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaveFunctionCollapse;

//This class will be passing the neighbours, it will store the dictionary for direction and
//Hashset of integer vakue that will represent neighbours in each direction
//For each pattern we will have a pattern neighbour class instance that will store neighbours in each direction

namespace WaveFunctionCollapse
{
    public class PatternNeighbours
    {
        public Dictionary<Direction, HashSet<int>> directionPatternNeighbourDictionary = new Dictionary<Direction, HashSet<int>>();

        public void AddPatternToDictionary(Direction dir,int patternIndex)
        {
            //If the neighbour pattern contains direction, we will add it to the hashset
            if (directionPatternNeighbourDictionary.ContainsKey(dir))
            {
                directionPatternNeighbourDictionary[dir].Add(patternIndex);
            }
            else //we will create a new hashset with a single value(patternindex)
            {
                directionPatternNeighbourDictionary.Add(dir, new HashSet<int>() { patternIndex });
            }
            //Only unique indices will be in it
        }

        internal HashSet<int> GetNeighboursInDirection(Direction dir)
        {
            //if it contains the key, we will return it
            if (directionPatternNeighbourDictionary.ContainsKey(dir))
            {
                return directionPatternNeighbourDictionary[dir];
            }
            //else we will return an empty hashset, because the information is more valid than a null value
            return new HashSet<int>();
        }

        //This method will be used in one of our strategies.When we have multiple instances of the same pattern
        //it will create a new pattern neighbour for each of those and it will add those other neighbour to the
        //existing pattern neighbours , for  the creation of a general neighbours list
        public void AddNeighbour(PatternNeighbours neighbours)
        {
            foreach (var item in neighbours.directionPatternNeighbourDictionary)
            {
                //If our dictionary contains key, we'll add it to the list
                //Otherwise we will create a new one

                if (directionPatternNeighbourDictionary.ContainsKey(item.Key) == false)//if we don't have the key
                {
                    directionPatternNeighbourDictionary.Add(item.Key,new HashSet<int>());
                }
                directionPatternNeighbourDictionary[item.Key].UnionWith(item.Value);//will add unique value into an hashset
            }
        }
    }
}
