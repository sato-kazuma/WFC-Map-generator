using UnityEngine;
using UnityEngine.Tilemaps;
using WaveFunctionCollapse;
using System.Text;
using System.Collections.Generic;
using System;

public class Test : MonoBehaviour
{
    public Tilemap input;
    // Start is called before the first frame update
    void Start()
    {

        //Verification part 1:Reading input->VALIDATED
        //--------------------------------------------------------------------------------------------------------
        //InputReader reader = new InputReader(input);
        //var grid = reader.ReadInputToGrid();
        //for (int row = 0; row < grid.Length; row++)
        //{
        //    for (int col = 0; col < grid[0].Length; col++)
        //    {
        //        Debug.Log("-Row: " + row + " -Col " + col + " -Tilename: " + grid[row][col].Value.name + "-");
        //    }
        //}
        //--------------------------------------------------------------------------------------------------------

        //Verification part 2:ValuesManager->VALIDATED
        //--------------------------------------------------------------------------------------------------------
        //InputReader reader = new InputReader(input);
        //var grid = reader.ReadInputToGrid();
        //ValuesManager<TileBase> valuesManager = new ValuesManager<TileBase>(grid);
        //StringBuilder builder = null;
        //List<string> list = new List<string>();

        //for (int row = -1; row <= grid.Length; row++)
        //{
        //    builder = new StringBuilder();
        //    for (int col = -1; col <= grid[0].Length; col++)
        //    {
        //        builder.Append(valuesManager.GetGridValuesIncludingOffset(col, row) + " ");
        //    }
        //    list.Add(builder.ToString());
        //}
        //list.Reverse();

        //foreach (var item in list)
        //{
        //    Debug.Log(item);
        //}
        //--------------------------------------------------------------------------------------------------------
        //Verification part 3:PatternManager->NOT VALIDATED
        InputReader reader = new InputReader(input);
        var grid = reader.ReadInputToGrid();
        ValuesManager<TileBase> valueManager = new ValuesManager<TileBase>(grid);
        int sizeOfpattern = 1;
        PatternManager manager = new PatternManager(sizeOfpattern);
        manager.ProcessGrid(valueManager, false);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
