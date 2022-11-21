using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
This class will process tilemap and extract from it tiles
Tilemap is a structure that stores towels as a 1D array
This means that we don't have a straight-up grid like
We need to create one*
We need also a method that will extract towels that are painted
from the while tile map
*/
namespace WaveFunctionCollapse
{
    public class InputImageParameters
    {
        // ? means it can be a null value
        //this will be the corners of our tile map
        Vector2Int? bottomRightTileCoords = null;
        Vector2Int? topLeftTileCoords = null;
        //this will store the bounds
        BoundsInt inputTilemapBounds;
        //the storage for the tile extracted
        TileBase[] inputTilemapTilesArray;
        //we will be adding a queue of tiles to dispute the tiles that are painted
        //or not known from our extracted tile in the tilebase array
        Queue<TileContainer> stackOftiles = new Queue<TileContainer>();
        private int width = 0, height = 0;
        private Tilemap inputTilemap;
        public Queue<TileContainer> StackOftiles { get => stackOftiles; set => stackOftiles = value; }
        public int Height { get => height; }
        public int Width { get => width; }
    

        public InputImageParameters(Tilemap inputTilemap)
        {
            this.inputTilemap = inputTilemap;
            this.inputTilemapBounds = this.inputTilemap.cellBounds;
            this.inputTilemapTilesArray = this.inputTilemap.GetTilesBlock(this.inputTilemapBounds);

            ExtractNonEmptyTiles();
            VerifyInputTiles();


        }

        private void VerifyInputTiles()
        {
            if (topLeftTileCoords == null || bottomRightTileCoords==null)
            {
                throw new System.Exception("WFC:Input Tilemap is Empty");
            }
            int minX = bottomRightTileCoords.Value.x;
            int maxX = topLeftTileCoords.Value.x;

            int minY = bottomRightTileCoords.Value.y;
            int maxY = topLeftTileCoords.Value.y;

            width = Math.Abs(maxX - minX) + 1;//we start from zero, we must add 1
            height = Math.Abs(maxY - minY) + 1;

            int tileCount = width * height;
            if(stackOftiles.Count != tileCount)
            {
                throw new Exception("WFC:Tilemaps has empty space");
            }
            if (stackOftiles.Any(tile => tile.X > maxX || tile.X < minX || tile.Y > maxY || tile.Y < minY))
            {
                throw new Exception("WFC:Tilemap must be a square/rectangle");

            }
        }

        //we will extract the non empty tiles
        //we are creating a 1D structure from a 2D structure
        private void ExtractNonEmptyTiles()
        {
            for (int row = 0; row < inputTilemapBounds.size.y; row++)
            {
                for (int col = 0; col < inputTilemapBounds.size.x; col++)
                {
                    int index = col + (row * inputTilemapBounds.size.x);
                    //jagged array->two dimensional array which will store data in forms of row and colums
                    TileBase tile = inputTilemapTilesArray[index];
                    if(bottomRightTileCoords== null && tile != null)
                    {
                        bottomRightTileCoords = new Vector2Int(col, row);
                    }
                    if (tile!=null)
                    {
                        //we want to enqueue each non null tile into out queue
                        //this will allow us to recreate the grid from our 1D array
                        //into a 2d array
                        stackOftiles.Enqueue(new TileContainer(tile, row, col));
                        topLeftTileCoords = new Vector2Int(col, row);
                    }
                }
            }
        }
    }
}
