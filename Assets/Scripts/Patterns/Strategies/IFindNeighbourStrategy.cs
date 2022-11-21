using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaveFunctionCollapse;

namespace WaveFunctionCollapse
{
    public interface IFindNeighbourStrategy 
    {
        Dictionary<int, PatternNeighbours> FindNeighbours(PatternDataResults patternFinderResults);
    }
}
