using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{
    //This class will use  simple factory pattern and through reflection
    //will load all the strategies and return the appropriate one by passing a name
    public class NeighbourStrategyFactory
    {
        //This will store all the classes that inherits from IFindNeighbourStrategy
        Dictionary<string, Type> strategies;

        public NeighbourStrategyFactory()
        {
            LoadTypesIFindNeighbourStrategy();
        }

        private void LoadTypesIFindNeighbourStrategy()
        {
            strategies = new Dictionary<string, Type>();
            Type[] typesInThisAssembly = Assembly.GetExecutingAssembly().GetTypes();

            foreach (var type in typesInThisAssembly)
            {
                if (type.GetInterface(typeof(IFindNeighbourStrategy).ToString()) != null)
                {
                    strategies.Add(type.Name.ToLower(), type);
                }
            }

        }

        internal IFindNeighbourStrategy CreateInstances(string nameOfStrategy)
        {
            Type t = GetTypeToCreate(nameOfStrategy);
            if (t == null)
            {
                t = GetTypeToCreate("more");
            }
            return Activator.CreateInstance(t) as IFindNeighbourStrategy;
        }

        private Type GetTypeToCreate(string nameOfStrategy)
        {
            foreach (var possibleStrategy in strategies)
            {
                if (possibleStrategy.Key.Contains(nameOfStrategy))
                {
                    return possibleStrategy.Value;
                }
            }
            return null;
        }
    }
}
