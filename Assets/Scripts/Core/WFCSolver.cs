using System;

namespace WaveFunctionCollapse
{
    //The Wave Function Collapse core.
    //Every output cell starts with all patterns possible ("superposition"). Each step:
    //  1. Observe: pick the undecided cell with the lowest Shannon entropy and collapse it to one
    //     pattern, chosen at random weighted by the pattern frequency.
    //  2. Propagate: remove from the neighbouring cells every pattern that is no longer supported,
    //     and keep going until nothing changes.
    //If a cell runs out of possible patterns we have a contradiction and the run fails.
    //
    //Propagation uses support counters (AC-4 style): compatible[cell, pattern, dir] counts how many
    //patterns still possible in the neighbour cell (opposite 'dir') allow 'pattern' here.
    //When it drops to zero the pattern is banned. This makes each ban O(neighbours) instead of
    //re-checking the whole neighbourhood.
    public class WFCSolver
    {
        private readonly int width, height, patternCount;
        private readonly bool periodicOutput;
        //propagator[dir][pattern] = patterns allowed one cell away in 'dir'
        private readonly int[][][] propagator;
        private readonly double[] weights, weightLogWeights;
        private readonly double sumOfWeights, sumOfWeightLogWeights, startingEntropy;

        //All the per-cell state lives in flat arrays to avoid allocations during the run
        private readonly bool[] wave;            //[cell * patternCount + pattern]
        private readonly int[] compatible;       //[(cell * patternCount + pattern) * 4 + dir]
        private readonly int[] sumsOfOnes;
        private readonly double[] sumsOfWeights, sumsOfWeightLogWeights, entropies;
        private readonly int[] stackCells, stackPatterns;
        private readonly double[] distribution;
        private int stackSize;
        private bool contradiction;

        private Random random;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public WFCSolver(PatternManager patternManager, int width, int height, bool periodicOutput)
        {
            if (patternManager == null || patternManager.PatternCount == 0)
            {
                throw new ArgumentException("WFC: the pattern manager has no patterns, call ProcessGrid first");
            }
            if (width < 1 || height < 1)
            {
                throw new ArgumentException("WFC: output size must be at least 1x1");
            }

            this.width = width;
            this.height = height;
            this.periodicOutput = periodicOutput;
            patternCount = patternManager.PatternCount;

            propagator = new int[DirectionHelper.Count][][];
            foreach (Direction dir in DirectionHelper.All)
            {
                propagator[(int)dir] = new int[patternCount][];
                for (int t = 0; t < patternCount; t++)
                {
                    propagator[(int)dir][t] = patternManager.GetPossibleNeighboursForPatternInDirection(t, dir);
                }
            }

            weights = new double[patternCount];
            weightLogWeights = new double[patternCount];
            for (int t = 0; t < patternCount; t++)
            {
                weights[t] = patternManager.GetPatternWeight(t);
                weightLogWeights[t] = weights[t] * Math.Log(weights[t]);
                sumOfWeights += weights[t];
                sumOfWeightLogWeights += weightLogWeights[t];
            }
            startingEntropy = Math.Log(sumOfWeights) - sumOfWeightLogWeights / sumOfWeights;

            int cells = width * height;
            wave = new bool[cells * patternCount];
            compatible = new int[cells * patternCount * DirectionHelper.Count];
            sumsOfOnes = new int[cells];
            sumsOfWeights = new double[cells];
            sumsOfWeightLogWeights = new double[cells];
            entropies = new double[cells];
            stackCells = new int[cells * patternCount];
            stackPatterns = new int[cells * patternCount];
            distribution = new double[patternCount];
        }

        //Runs the algorithm once. Returns true if every cell collapsed without contradictions.
        public bool Run(int seed)
        {
            random = new Random(seed);
            Clear();

            if (BanUnsupportedPatterns() == false)
            {
                return false;
            }

            while (true)
            {
                bool? finished = Observe();
                if (finished.HasValue)
                {
                    return finished.Value;
                }
                if (Propagate() == false)
                {
                    return false;
                }
            }
        }

        //Pattern index per cell as [row][col] (row 0 = bottom); -1 where the cell is not decided
        //(only possible after a failed run).
        public int[][] GetOutputPatternGrid()
        {
            int[][] result = new int[height][];
            for (int y = 0; y < height; y++)
            {
                result[y] = new int[width];
                for (int x = 0; x < width; x++)
                {
                    int cell = y * width + x;
                    result[y][x] = -1;
                    if (sumsOfOnes[cell] != 1)
                    {
                        continue;
                    }
                    int offset = cell * patternCount;
                    for (int t = 0; t < patternCount; t++)
                    {
                        if (wave[offset + t])
                        {
                            result[y][x] = t;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        private void Clear()
        {
            int cells = width * height;
            for (int cell = 0; cell < cells; cell++)
            {
                for (int t = 0; t < patternCount; t++)
                {
                    wave[cell * patternCount + t] = true;
                    int c = (cell * patternCount + t) * DirectionHelper.Count;
                    for (int d = 0; d < DirectionHelper.Count; d++)
                    {
                        compatible[c + d] = propagator[d ^ 1][t].Length;
                    }
                }
                sumsOfOnes[cell] = patternCount;
                sumsOfWeights[cell] = sumOfWeights;
                sumsOfWeightLogWeights[cell] = sumOfWeightLogWeights;
                entropies[cell] = startingEntropy;
            }
            stackSize = 0;
            contradiction = false;
        }

        //A pattern with no allowed neighbour in some direction can never be placed in a cell that
        //has a neighbour there (it happens with non periodic input), so remove it up front.
        private bool BanUnsupportedPatterns()
        {
            for (int t = 0; t < patternCount; t++)
            {
                for (int d = 0; d < DirectionHelper.Count; d++)
                {
                    if (propagator[d][t].Length > 0)
                    {
                        continue;
                    }
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int cell = y * width + x;
                            if (wave[cell * patternCount + t] && GetNeighbourCell(x, y, d) >= 0)
                            {
                                Ban(cell, t);
                            }
                        }
                    }
                }
            }
            return contradiction == false && Propagate();
        }

        //null = keep going, true = all cells collapsed, false = contradiction
        private bool? Observe()
        {
            double minEntropy = double.MaxValue;
            int argMin = -1;

            for (int cell = 0; cell < sumsOfOnes.Length; cell++)
            {
                int amount = sumsOfOnes[cell];
                if (amount == 0)
                {
                    return false;
                }
                if (amount == 1)
                {
                    continue;
                }
                //Tiny noise breaks ties randomly between cells with the same entropy
                double entropy = entropies[cell] + 1e-6 * random.NextDouble();
                if (entropy < minEntropy)
                {
                    minEntropy = entropy;
                    argMin = cell;
                }
            }

            if (argMin == -1)
            {
                return true;
            }

            int offset = argMin * patternCount;
            double total = 0;
            for (int t = 0; t < patternCount; t++)
            {
                distribution[t] = wave[offset + t] ? weights[t] : 0;
                total += distribution[t];
            }

            double r = random.NextDouble() * total;
            int chosen = -1;
            for (int t = 0; t < patternCount; t++)
            {
                if (distribution[t] <= 0)
                {
                    continue;
                }
                chosen = t;
                r -= distribution[t];
                if (r < 0)
                {
                    break;
                }
            }

            for (int t = 0; t < patternCount; t++)
            {
                if (t != chosen && wave[offset + t])
                {
                    Ban(argMin, t);
                }
            }
            return null;
        }

        private bool Propagate()
        {
            while (stackSize > 0)
            {
                stackSize--;
                int cell1 = stackCells[stackSize];
                int pattern1 = stackPatterns[stackSize];
                int x1 = cell1 % width;
                int y1 = cell1 / width;

                for (int d = 0; d < DirectionHelper.Count; d++)
                {
                    int cell2 = GetNeighbourCell(x1, y1, d);
                    if (cell2 < 0)
                    {
                        continue;
                    }

                    int[] allowed = propagator[d][pattern1];
                    for (int i = 0; i < allowed.Length; i++)
                    {
                        int pattern2 = allowed[i];
                        int c = (cell2 * patternCount + pattern2) * DirectionHelper.Count + d;
                        compatible[c]--;
                        if (compatible[c] == 0)
                        {
                            Ban(cell2, pattern2);
                        }
                    }
                }

                if (contradiction)
                {
                    return false;
                }
            }
            return contradiction == false;
        }

        private void Ban(int cell, int pattern)
        {
            int index = cell * patternCount + pattern;
            wave[index] = false;

            //Zeroing the counters guarantees the same pattern is never banned twice
            int c = index * DirectionHelper.Count;
            for (int d = 0; d < DirectionHelper.Count; d++)
            {
                compatible[c + d] = 0;
            }

            stackCells[stackSize] = cell;
            stackPatterns[stackSize] = pattern;
            stackSize++;

            sumsOfOnes[cell]--;
            sumsOfWeights[cell] -= weights[pattern];
            sumsOfWeightLogWeights[cell] -= weightLogWeights[pattern];

            if (sumsOfOnes[cell] == 0)
            {
                contradiction = true;
                return;
            }
            double sum = sumsOfWeights[cell];
            entropies[cell] = Math.Log(sum) - sumsOfWeightLogWeights[cell] / sum;
        }

        //Index of the neighbour cell in direction d, or -1 if it is outside a non periodic output
        private int GetNeighbourCell(int x, int y, int d)
        {
            Direction dir = (Direction)d;
            int nx = x + dir.OffsetX();
            int ny = y + dir.OffsetY();
            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
            {
                if (periodicOutput == false)
                {
                    return -1;
                }
                nx = (nx + width) % width;
                ny = (ny + height) % height;
            }
            return ny * width + nx;
        }
    }
}
