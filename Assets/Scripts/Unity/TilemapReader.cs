using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse
{
    //Reads the painted area of a Tilemap into a [row][col] grid (row 0 = bottom).
    //The painted tiles must form a completely filled rectangle.
    public static class TilemapReader
    {
        public static TileBase[][] ReadTiles(Tilemap tilemap)
        {
            if (tilemap == null)
            {
                throw new ArgumentNullException("tilemap");
            }

            BoundsInt bounds = tilemap.cellBounds;
            //GetTilesBlock returns the tiles ordered x first, then y, then z: we only use the z = 0 layer
            TileBase[] block = tilemap.GetTilesBlock(bounds);
            int sizeX = bounds.size.x;
            int sizeY = bounds.size.y;

            //cellBounds can be bigger than the painted area (e.g. after erasing), so find the real one
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            for (int row = 0; row < sizeY; row++)
            {
                for (int col = 0; col < sizeX; col++)
                {
                    if (block[col + row * sizeX] == null)
                    {
                        continue;
                    }
                    minX = Math.Min(minX, col);
                    maxX = Math.Max(maxX, col);
                    minY = Math.Min(minY, row);
                    maxY = Math.Max(maxY, row);
                }
            }

            if (minX == int.MaxValue)
            {
                throw new InvalidOperationException("WFC: input tilemap is empty");
            }

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            TileBase[][] grid = new TileBase[height][];
            for (int row = 0; row < height; row++)
            {
                grid[row] = new TileBase[width];
                for (int col = 0; col < width; col++)
                {
                    TileBase tile = block[(minX + col) + (minY + row) * sizeX];
                    if (tile == null)
                    {
                        throw new InvalidOperationException("WFC: input tilemap must be a filled rectangle, cell ("
                            + (bounds.xMin + minX + col) + ", " + (bounds.yMin + minY + row) + ") is empty");
                    }
                    grid[row][col] = tile;
                }
            }
            return grid;
        }

        //Writes a [row][col] grid of tiles into the tilemap, with the bottom left corner at 'origin'
        public static void WriteTiles(Tilemap tilemap, TileBase[][] grid, Vector3Int origin)
        {
            int height = grid.Length;
            int width = grid[0].Length;
            TileBase[] block = new TileBase[width * height];
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    block[col + row * width] = grid[row][col];
                }
            }
            tilemap.ClearAllTiles();
            tilemap.SetTilesBlock(new BoundsInt(origin, new Vector3Int(width, height, 1)), block);
        }
    }
}
