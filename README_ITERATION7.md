# Project Iteration 7: L-System Village Generation (Exploratory)

**GitHub Repository:** [Enrikkk/AI-Terrain-Generation](https://github.com/Enrikkk/AI-Terrain-Generation)

This iteration began exploring procedural village generation using Lindenmayer Systems (L-Systems). The goal was to produce organic road networks and building layouts on the Plains biome surface using a grammar-based string rewriting system interpreted as turtle graphics. The core grammar engine was implemented; full terrain integration was not required for the project's scope.

---

## L-System Grammar Engine

### What is an L-System?

A Lindenmayer System is a parallel string-rewriting grammar invented by botanist Aristid Lindenmayer to model plant growth. It consists of:

- **Axiom** — the starting string (seed)
- **Production rules** — a mapping from each symbol to its replacement string
- **Iterations** — how many times rules are applied simultaneously to every symbol

After N iterations the resulting string is interpreted as a sequence of drawing commands for a virtual "turtle" that traces geometry in 3D space.

### `LSystemGenerator.cs`

A pure C# class (not a `MonoBehaviour`) responsible for grammar expansion only. It has no Unity lifecycle dependency and can be instantiated anywhere.

**Fields:**
- `string axiom` — starting string (e.g. `"F+F+F+F"`)
- `Dictionary<char, string> rules` — maps each variable character to its replacement
- `int iterations` — number of rewriting passes

**Method:** `string GenerateString()` — applies all production rules simultaneously for N iterations using `StringBuilder` for performance, then returns the final string.

### Grammar for Road Generation

| Parameter | Value |
|---|---|
| Axiom | `F+F+F+F` |
| Rule | `F → F+F-F-F+F` (Koch curve variant) |
| Angle | 90° |
| Step length | 8 world units |
| Iterations | 3 |

**Symbol meanings:**

| Symbol | Meaning |
|---|---|
| `F` | Move forward (draw a road segment) |
| `+` | Turn left by `angle` |
| `-` | Turn right by `angle` |
| `[` | Push turtle state onto stack |
| `]` | Pop turtle state from stack |

After 3 iterations the axiom `F+F+F+F` (4 `F`s) expands to a string with **4 × 5³ = 500 `F` symbols**, each representing one road segment — enough geometry for a dense village street network.

### Why StringBuilder?

C# strings are immutable. Using `+=` in a loop creates a new string object on every iteration, which becomes prohibitively slow as the string grows to hundreds of characters. `StringBuilder` maintains a mutable internal buffer and appends in O(1) amortized time.

---

## Planned Architecture (Not Implemented)

The following components were designed but not implemented as the project was complete at Iteration 6:

### `VillageGenerator.cs` (MonoBehaviour)

Would orchestrate the full pipeline:
1. **`FindPlainsSpawnPoint(seed)`** — scan heightmap at coarse intervals, classify each point using the same Perlin biome formula as `MultiBiomeTerrainGenerator`, compute slope from four cardinal neighbours, select the flattest Plains candidate
2. **`InterpretLSystem(string, origin)`** — turtle graphics interpreter walking the L-System string and emitting `(start, end)` road segment pairs with terrain-snapped Y coordinates via `terrain.SampleHeight()`
3. **`RenderRoads(segments)`** — flat quad mesh per segment (four corners offset perpendicular to road direction, vertices Y-sampled from terrain) accumulated into a single combined `Mesh`
4. **`PlaceBuildings(segments)`** — step along each segment, offset perpendicular by a setback distance, instantiate `GameObject.CreatePrimitive(PrimitiveType.Cube)` with randomised scale, parented under a `"Village Buildings"` container

### Integration Point

Called from `MultiBiomeTerrainGenerator.GenerateMultiBiomeTerrain()` after `SpawnWater()`, with the terrain seed exposed via a public getter so the biome classification remains deterministic.

---

## Technical Summary

| Technique | Purpose | Script |
|---|---|---|
| L-System string rewriting | Grammar-based procedural road layout | LSystemGenerator.cs |
| Koch curve variant (`F→F+F-F-F+F`) | Generates branching, winding street networks from a square axiom | LSystemGenerator.cs |
| `StringBuilder` iteration | Efficient string expansion without O(n²) allocations | LSystemGenerator.cs |
| Turtle graphics (planned) | Converts symbol string into 3D road segment geometry | VillageGenerator.cs |
| Perlin biome classification (planned) | Constrains village to flat Plains terrain | VillageGenerator.cs |
