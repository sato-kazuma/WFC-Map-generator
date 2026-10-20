# WFC Map Generator

C# / Unity (2019.2+) 2D procedural map generation with the **Wave Function Collapse** algorithm.
First successful attempt at exploring programming in the field of video game development.

## Usage

1. Open `Assets/ScenePackage/Scene.unity`.
2. Paint a sample map on the **Input** tilemap (the painted tiles must form a filled rectangle).
3. Select the **WFCGenerator** object, set the options and press Play
   (or right click the component → **Generate**, it also works in edit mode).
   If no output tilemap is assigned, one is created next to the input.

| Option | Meaning |
| --- | --- |
| `patternSize` | 1 = simple tiled model (neighbours copied from the input), 2+ = overlapping model |
| `periodicInput` | the input wraps around its borders |
| `periodicOutput` | the output wraps around its borders (tileable) |
| `equalWeights` | ignore how often a pattern appears in the input |
| `seed` | 0 = random, otherwise reproducible output |
| `maxAttempts` | restarts allowed after a contradiction |

## Structure

```
Assets/Scripts
├── Core                  pure C#, no Unity dependency
│   ├── Direction.cs        directions, offsets, opposite
│   ├── ValuesManager.cs    generic values -> integer indices grid
│   ├── Pattern.cs          NxN pattern + overlap check
│   ├── PatternManager.cs   pattern extraction, frequencies, neighbour rules
│   └── WFCSolver.cs        observe / propagate / collapse
└── Unity
    ├── TilemapReader.cs    Tilemap <-> grid
    └── WFCGenerator.cs     MonoBehaviour entry point
```
