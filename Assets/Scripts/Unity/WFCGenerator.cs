using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

namespace WaveFunctionCollapse
{
    //Entry point: put it on a GameObject, assign the input tilemap and press Play
    //(or use "Generate" from the component context menu, it works in edit mode too).
    public class WFCGenerator : MonoBehaviour
    {
        [Header("Tilemaps")]
        [Tooltip("Sample map: the painted tiles must form a filled rectangle")]
        public Tilemap input;
        [Tooltip("Where the result is written. If empty, a new tilemap is created next to the input")]
        public Tilemap output;

        [Header("Output")]
        public int outputWidth = 20;
        public int outputHeight = 20;
        [Tooltip("Wrap the output around its borders (tileable result)")]
        public bool periodicOutput = false;

        [Header("Patterns")]
        [Tooltip("1 = simple tiled model (neighbours taken from the input), 2+ = overlapping model")]
        [Range(1, 5)]
        public int patternSize = 1;
        [Tooltip("Treat the input as wrapping around its borders")]
        public bool periodicInput = true;
        [Tooltip("Ignore how often a pattern appears in the input")]
        public bool equalWeights = false;

        [Header("Run")]
        [Tooltip("0 = random seed every time")]
        public int seed = 0;
        [Tooltip("Restarts allowed when the algorithm hits a contradiction")]
        public int maxAttempts = 10;
        public bool generateOnStart = true;

        private void Start()
        {
            if (generateOnStart)
            {
                Generate();
            }
        }

        private void OnValidate()
        {
            outputWidth = Math.Max(1, outputWidth);
            outputHeight = Math.Max(1, outputHeight);
            patternSize = Math.Max(1, patternSize);
            maxAttempts = Math.Max(1, maxAttempts);
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (input == null)
            {
                Debug.LogError("WFC: assign the input tilemap", this);
                return;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                var valuesManager = new ValuesManager<TileBase>(TilemapReader.ReadTiles(input));
                var patternManager = new PatternManager(patternSize);
                patternManager.ProcessGrid(valuesManager, equalWeights, periodicInput);
                var solver = new WFCSolver(patternManager, outputWidth, outputHeight, periodicOutput);

                var random = seed == 0 ? new System.Random() : new System.Random(seed);
                bool success = false;
                int attempt = 0;
                while (success == false && attempt < maxAttempts)
                {
                    attempt++;
                    success = solver.Run(random.Next());
                }

                TileBase[][] tiles = ToTiles(solver.GetOutputPatternGrid(), patternManager, valuesManager);
                TilemapReader.WriteTiles(GetOrCreateOutput(), tiles, Vector3Int.zero);

                string stats = valuesManager.ValueCount + " tiles, " + patternManager.PatternCount + " patterns, "
                    + attempt + " attempt(s), " + stopwatch.ElapsedMilliseconds + " ms";
                if (success)
                {
                    Debug.Log("WFC: done (" + stats + ")", this);
                }
                else
                {
                    Debug.LogWarning("WFC: contradiction in every attempt, the output is incomplete (" + stats + "). "
                        + "Try a bigger maxAttempts, a different pattern size or a richer input.", this);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message, this);
            }
        }

        //Each output cell shows the bottom left value of its pattern
        private static TileBase[][] ToTiles(int[][] patternGrid, PatternManager patternManager, ValuesManager<TileBase> valuesManager)
        {
            TileBase[][] tiles = new TileBase[patternGrid.Length][];
            for (int row = 0; row < patternGrid.Length; row++)
            {
                tiles[row] = new TileBase[patternGrid[row].Length];
                for (int col = 0; col < patternGrid[row].Length; col++)
                {
                    int patternIndex = patternGrid[row][col];
                    if (patternIndex >= 0)
                    {
                        int valueIndex = patternManager.GetPattern(patternIndex).GetGridValue(0, 0);
                        tiles[row][col] = valuesManager.GetValueFromIndex(valueIndex);
                    }
                }
            }
            return tiles;
        }

        private Tilemap GetOrCreateOutput()
        {
            if (output != null)
            {
                return output;
            }

            //Sibling of the input under the same Grid, placed to the right of the painted area
            var outputObject = new GameObject("Output");
            outputObject.transform.SetParent(input.transform.parent, false);
            Vector3 cellSize = input.layoutGrid.cellSize;
            BoundsInt inputBounds = input.cellBounds;
            outputObject.transform.localPosition = input.transform.localPosition
                + new Vector3((inputBounds.xMax + 2) * cellSize.x, inputBounds.yMin * cellSize.y, 0);

            output = outputObject.AddComponent<Tilemap>();
            outputObject.AddComponent<TilemapRenderer>();
            return output;
        }
    }
}
