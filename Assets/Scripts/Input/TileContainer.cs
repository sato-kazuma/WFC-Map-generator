using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse{
    //it will store tile base object and assign it to x and y coordinates
    //or row and colums,depending on which implementations you choose
    public class TileContainer
    {
        public TileBase Tile { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public TileContainer(TileBase tile,int X,int Y)
        {
            this.Tile = tile;
            this.X = X;
            this.Y = Y;
        }


    }
}
