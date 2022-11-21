namespace WaveFunctionCollapse
{
    public interface IInputReader<T>
    {
        //a generic interface that will output a grid of generic high value
        //all the methods will be stored in the same namespace: WaveFunctionCollapse
        IValue<T>[][] ReadInputToGrid();
    }
}

//a generic interface that will output a grid of generic high value
//all the methods will be stored in the same namespace: WaveFunctionCollapse