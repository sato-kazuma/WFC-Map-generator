using Helpers;
using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse
{
    public class InputReader : IInputReader<TileBase>
    {
        private Tilemap _inputTilemap;

        public InputReader(Tilemap input)
        {
            _inputTilemap = input;
        }

        public IValue<TileBase>[][] ReadInputToGrid()
        {
            //we will read The input Tile map
            var grid = ReadInputTileMap();

            //we need a grid that will store out tile bases, it will be a jagged array
            TileBaseValue[][] gridOfValues = null;

            if (grid != null)
            {
                gridOfValues = MyCollectionExtension.CreateJaggedArray<TileBaseValue[][]>(grid.Length,grid[0].Length);//row,col
                //we will loop and create a new storage grid
                for (int row = 0; row < grid.Length; row++)
                {
                    for (int col = 0; col < grid[0].Length; col++)
                    {
                        gridOfValues[row][col] = new TileBaseValue(grid[row][col]);
                    }
                }
            }
            return gridOfValues;
        }

        private TileBase[][] ReadInputTileMap()
        {
            InputImageParameters imageParameters = new InputImageParameters(_inputTilemap);
            return CreateTileBaseGrid(imageParameters);
        }

        //will store tilebase value
        private TileBase[][] CreateTileBaseGrid(InputImageParameters imageParameters)
        {
            TileBase[][] gridOfinputTiles = null;
            gridOfinputTiles = MyCollectionExtension.CreateJaggedArray<TileBase[][]>(imageParameters.Height,imageParameters.Width);
            for (int row = 0; row < imageParameters.Height; row++)
            {
                for (int col = 0; col < imageParameters.Width; col++)
                { 
                    //we started from the bottom left corner and we are filling this grid also from lower left corner
                    gridOfinputTiles[row][col] = imageParameters.StackOftiles.Dequeue().Tile;
                }
            }
            return gridOfinputTiles;
        }
    }
}
