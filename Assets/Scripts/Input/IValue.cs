using System;
using System.Collections.Generic;

namespace WaveFunctionCollapse
{
    /*we will store whatever we will read as input as a generic object
    -----------------------------------------------------------------------------------
    IEquatable will allow us to use classes that use a value interface inside
    generic collection such as dictionary(which only accept unique keys and 
    every time you add a new one it compares it with all the keys it contains
    loaded definition of how to compare those keys, we will not be able to distinguish
    between two high value classes(classi padre) because we know that they store different
    values, but dictionary has no way to checking it and that's why we need IEquatable interface
    which we will implemennt in a concrete classes that inherits a value tipe
    -----------------------------------------------------------------------------------
    IEqualityComparer is used in a link library that will allows us to query the data
    inside our collection.It will come handy when we want, for example, to check if any of the
    cells in our output grid is not yet sold
    */
    public interface IValue<T>: IEqualityComparer<IValue<T>>, IEquatable<IValue<T>>
    {
       T Value { get; }
    }
}
