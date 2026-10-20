using System;
using System.Collections.Generic;

namespace WaveFunctionCollapse
{
    public enum NeighbourStrategy
    {
        //InputAdjacency for patternSize == 1, Overlap otherwise
        Auto,
        //Two patterns are neighbours if they appear next to each other in the input
        InputAdjacency,
        //Two patterns are neighbours if they agree on all overlapping cells (needs patternSize > 1)
        Overlap
    }

    //Extracts the NxN patterns from the input, counts their frequencies and builds the
    //neighbour rules: for every pattern and direction, which patterns may sit next to it.
    public class PatternManager
    {
        private readonly int patternSize;
        private Pattern[] patterns;
        private double[] weights;
        //neighbours[dir][pattern] = sorted list of patterns allowed one cell away in 'dir'
        private int[][][] neighbours;

        public int PatternSize { get { return patternSize; } }
        public int PatternCount { get { return patterns == null ? 0 : patterns.Length; } }

        public PatternManager(int patternSize)
        {
            if (patternSize < 1)
            {
                throw new ArgumentOutOfRangeException("patternSize", "WFC: pattern size must be at least 1");
            }
            this.patternSize = patternSize;
        }

        public void ProcessGrid<T>(ValuesManager<T> valuesManager, bool equalWeights, bool periodicInput = true,
            NeighbourStrategy strategy = NeighbourStrategy.Auto)
        {
            if (strategy == NeighbourStrategy.Auto)
            {
                strategy = patternSize == 1 ? NeighbourStrategy.InputAdjacency : NeighbourStrategy.Overlap;
            }
            if (strategy == NeighbourStrategy.Overlap && patternSize == 1)
            {
                throw new ArgumentException("WFC: the Overlap strategy needs a pattern size greater than 1");
            }

            int[][] patternIndexGrid = ExtractPatterns(valuesManager, periodicInput);

            weights = new double[patterns.Length];
            for (int i = 0; i < patterns.Length; i++)
            {
                weights[i] = equalWeights ? 1.0 : patterns[i].Frequency;
            }

            var rules = new HashSet<int>[DirectionHelper.Count][];
            for (int d = 0; d < DirectionHelper.Count; d++)
            {
                rules[d] = new HashSet<int>[patterns.Length];
                for (int t = 0; t < patterns.Length; t++)
                {
                    rules[d][t] = new HashSet<int>();
                }
            }

            if (strategy == NeighbourStrategy.InputAdjacency)
            {
                FindNeighboursFromInputAdjacency(patternIndexGrid, periodicInput, rules);
            }
            else
            {
                FindNeighboursByOverlap(rules);
            }

            neighbours = new int[DirectionHelper.Count][][];
            for (int d = 0; d < DirectionHelper.Count; d++)
            {
                neighbours[d] = new int[patterns.Length][];
                for (int t = 0; t < patterns.Length; t++)
                {
                    int[] list = new int[rules[d][t].Count];
                    rules[d][t].CopyTo(list);
                    Array.Sort(list);
                    neighbours[d][t] = list;
                }
            }
        }

        public Pattern GetPattern(int index)
        {
            return patterns[index];
        }

        public double GetPatternWeight(int index)
        {
            return weights[index];
        }

        public int[] GetPossibleNeighboursForPatternInDirection(int patternIndex, Direction dir)
        {
            return neighbours[(int)dir][patternIndex];
        }

        //Reads every NxN window of the input (wrapping around the borders if periodicInput),
        //deduplicates identical windows and returns the grid of pattern indices, one per window position.
        private int[][] ExtractPatterns<T>(ValuesManager<T> valuesManager, bool periodicInput)
        {
            int gridWidth = periodicInput ? valuesManager.Width : valuesManager.Width - patternSize + 1;
            int gridHeight = periodicInput ? valuesManager.Height : valuesManager.Height - patternSize + 1;
            if (gridWidth <= 0 || gridHeight <= 0)
            {
                throw new ArgumentException("WFC: pattern size " + patternSize + " is bigger than the input ("
                    + valuesManager.Width + "x" + valuesManager.Height + ")");
            }

            var patternList = new List<Pattern>();
            var patternLookup = new Dictionary<int[], Pattern>(new IntArrayComparer());
            int[][] patternIndexGrid = new int[gridHeight][];

            for (int y = 0; y < gridHeight; y++)
            {
                patternIndexGrid[y] = new int[gridWidth];
                for (int x = 0; x < gridWidth; x++)
                {
                    int[] window = new int[patternSize * patternSize];
                    for (int py = 0; py < patternSize; py++)
                    {
                        for (int px = 0; px < patternSize; px++)
                        {
                            window[py * patternSize + px] = valuesManager.GetGridValueWrapped(x + px, y + py);
                        }
                    }

                    Pattern pattern;
                    if (patternLookup.TryGetValue(window, out pattern))
                    {
                        pattern.Frequency++;
                    }
                    else
                    {
                        pattern = new Pattern(window, patternSize, patternList.Count);
                        patternList.Add(pattern);
                        patternLookup.Add(window, pattern);
                    }
                    patternIndexGrid[y][x] = pattern.Index;
                }
            }

            patterns = patternList.ToArray();
            return patternIndexGrid;
        }

        //Rules are always added in both directions so the relation stays symmetric
        //(A has B on its right <=> B has A on its left), which the solver relies on.
        private static void FindNeighboursFromInputAdjacency(int[][] patternIndexGrid, bool periodicInput, HashSet<int>[][] rules)
        {
            int height = patternIndexGrid.Length;
            int width = patternIndexGrid[0].Length;
            Direction[] forward = { Direction.Up, Direction.Right };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    foreach (Direction dir in forward)
                    {
                        int nx = x + dir.OffsetX();
                        int ny = y + dir.OffsetY();
                        if (nx >= width || ny >= height)
                        {
                            if (periodicInput == false)
                            {
                                continue;
                            }
                            nx %= width;
                            ny %= height;
                        }
                        AddSymmetricRule(rules, patternIndexGrid[y][x], dir, patternIndexGrid[ny][nx]);
                    }
                }
            }
        }

        private void FindNeighboursByOverlap(HashSet<int>[][] rules)
        {
            Direction[] forward = { Direction.Up, Direction.Right };
            for (int a = 0; a < patterns.Length; a++)
            {
                for (int b = 0; b < patterns.Length; b++)
                {
                    foreach (Direction dir in forward)
                    {
                        if (patterns[a].CanOverlap(dir, patterns[b]))
                        {
                            AddSymmetricRule(rules, a, dir, b);
                        }
                    }
                }
            }
        }

        private static void AddSymmetricRule(HashSet<int>[][] rules, int pattern, Direction dir, int neighbour)
        {
            rules[(int)dir][pattern].Add(neighbour);
            rules[(int)dir.GetOppositeDirectionTo()][neighbour].Add(pattern);
        }

        //Structural equality for int arrays, used to deduplicate patterns
        //(replaces the previous MD5 string hash: no allocations, no hashing of bytes)
        private sealed class IntArrayComparer : IEqualityComparer<int[]>
        {
            public bool Equals(int[] a, int[] b)
            {
                if (ReferenceEquals(a, b)) return true;
                if (a == null || b == null || a.Length != b.Length) return false;
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] != b[i]) return false;
                }
                return true;
            }

            public int GetHashCode(int[] array)
            {
                unchecked
                {
                    int hash = 17;
                    for (int i = 0; i < array.Length; i++)
                    {
                        hash = hash * 31 + array[i];
                    }
                    return hash;
                }
            }
        }
    }
}
