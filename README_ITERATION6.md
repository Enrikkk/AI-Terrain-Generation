# Project Iteration 6: Portal System & Cave-Surface Integration

**GitHub Repository:** [Enrikkk/AI-Terrain-Generation](https://github.com/Enrikkk/AI-Terrain-Generation)

Welcome to the 6th iteration! This phase completed the cave system introduced in iteration 5 by solving its biggest open problem: how to actually connect the underground cave to the surface world. The solution was a fully procedural, bidirectional portal system that spawns paired portals at runtime — one on the terrain surface, one on the cave floor — and teleports the player between them seamlessly. Along the way, several critical issues were resolved: cave interior collision was broken, player teleportation required a tricky physics workaround, and finding a safe landing spot inside the cave required dynamic raycasting.

---

## Portal System

The portal system is built around a single `Portal.cs` script attached to a trigger collider. When the player walks through the collider, the script calculates a safe destination in the other world and moves them there.

### Detection

Unity's `OnTriggerEnter(Collider other)` callback fires when any collider enters the trigger zone. The portal checks if the entering object is the player by name, then retrieves the `PlayerController` component to enforce the cooldown.

One subtlety: the `Portal` script lives on a child object (the one with the mesh and trigger collider), so `MultiBiomeTerrainGenerator` uses `GetComponentInChildren<Portal>()` rather than `GetComponent<Portal>()` to reach it from the parent prefab.

### Smart Teleport Destination

The key challenge was landing the player somewhere safe, not in mid-air or inside solid geometry. The destination logic is determined by the `isCavePortal` flag, which marks whether a portal is underground or on the surface:

**Cave portal → Surface** (`isCavePortal = true`):
The player is leaving the cave and arriving on the terrain. The destination is calculated using `terrain.SampleHeight()` at the surface portal's X/Z position, snapping Y precisely to the terrain height regardless of hills or valleys.

**Surface portal → Cave** (`isCavePortal = false`):
The player is entering the cave. Rather than using a hardcoded offset (which could land outside the cave in empty air), a `Physics.Raycast` is fired downward from above the cave portal's position. The ray hits the cave mesh collider and returns the exact floor position — the player is placed 1 unit above that point.

```
Surface portal enters → Raycast down → hits cave floor → player spawns 1 unit above
Cave portal enters    → SampleHeight at surface portal X/Z → player spawns on terrain
```

### CharacterController Workaround

The player uses a `CharacterController` component, not a `Rigidbody`. Unity's `CharacterController` continuously overrides `transform.position` every frame, which means directly assigning a new position gets immediately reverted. The fix is to disable the `CharacterController` for the one frame of the teleport, assign the position, and immediately re-enable it:

```csharp
CharacterController cc = other.GetComponent<CharacterController>();
if (cc != null) cc.enabled = false;
other.transform.position = destination;
if (cc != null) cc.enabled = true;
```

### Portal Cooldown

Without a cooldown, the player immediately triggers the destination portal upon arrival and bounces back endlessly. A 5-second cooldown timer is tracked inside `PlayerController`. On entry, `startPortalCoolDown()` is called; each frame, `Update()` decrements the timer. If the timer is above zero, the portal skips the teleport and returns early.

---

## Cave-Surface Integration

Iteration 5 attempted to integrate the cave with the surface by punching holes in Unity's terrain heightmap using `TerrainData.SetHoles()`. This never worked reliably due to coordinate space mismatches. Iteration 6 replaced this approach entirely with procedural portal placement.

### Spawning the Cave Portal

`CaveGenerator.SpawnPortalInsideCave()` scans the 3D boolean grid for valid floor cells: an AIR cell (`grid[x,y,z] == false`) with a ROCK cell directly below (`grid[x,y+1,z] == true`). This guarantees the portal spawns on solid ground, not floating in the middle of a tunnel.

Only cells from the middle Y layers (25%–75% of grid height) are considered to avoid the boundary rock that surrounds the cave at the top and bottom. A random cell is picked from the valid candidates, converted to world space with the Y axis negated (the cave digs downward), and the portal prefab is instantiated there.

### Pairing Portals

`MultiBiomeTerrainGenerator.PairPortals(portalA, portalB)` sets `otherPortal` on each portal script to point at the other, establishing a bidirectional link. This is called in `GenerateMultiBiomeTerrain()` after both portals are spawned:

```
SpawnPortal("center")           → surface portal at terrain center
SpawnPortalInsideCave(...)      → cave portal on random floor cell
PairPortals(surface, cave)      → link them bidirectionally
```

### Execution Order: Awake vs Start

A subtle timing problem existed: `MultiBiomeTerrainGenerator.Start()` calls `SpawnPortalInsideCave()`, which reads the cave grid. But if `CaveGenerator.Start()` initializes the grid, both run in the same phase with no guaranteed order — the grid could be empty when the portal tries to scan it.

The fix: grid initialization (the expensive cellular automata passes) was moved from `CaveGenerator.Start()` to `CaveGenerator.Awake()`. Unity guarantees `Awake()` runs before any `Start()` across all objects, so the grid is fully populated by the time `MultiBiomeTerrainGenerator.Start()` needs it.

---

## Cave Collision Fix

After the portal system was working, a new problem appeared: the player could fall through cave walls and floors when inside the cave. Flying into the cave from the outside was solid, but once inside, all surfaces became passable.

### Root Cause: One-Sided Mesh Collider

Unity's non-convex `MeshCollider` only registers collisions on the **front face** of each triangle. The front face is determined by the triangle's winding order — the order its three vertices are listed determines which direction its normal points, which determines which side you can collide with.

The Marching Cubes triangle table generates triangles whose normals face **outward** (away from the cave interior, toward the solid rock). This means:
- From outside the cave: you face the front of triangles → solid ✓
- From inside the cave: you face the back of triangles → passable ✗

### Fix: Reverse the Winding Order

The fix is to reverse the triangle winding order in `CaveRenderer.MarchCube()` so normals point inward instead. For each triangle with vertices `[A, B, C]`, adding them as `[A, C, B]` produces an identical triangle at the same position, but with the normal pointing in the opposite direction.

The implementation collects the three vertex indices first, then adds them to the triangle list in reversed order:

```csharp
int[] triIndices = new int[3];
for(int j = 0; j < 3; j++) { /* ... look up vertex ... */ triIndices[j] = idx; }
triangles.Add(triIndices[0]);
triangles.Add(triIndices[2]); // swapped
triangles.Add(triIndices[1]); // swapped
```

### Why Not Double-Sided?

An alternative would be to add both the original and reversed triangles, making the mesh solid from both directions at the cost of 2× the triangle count. For this project, the cheaper single-direction flip is sufficient: the player always enters the cave via portal (teleported directly inside), so there is no need to push through from the outside.

---

## Fog Density Reduction

The Perlin-noise-driven fog in `FogScheduler.cs` was remapped from a density range of `(0.001, 0.02)` down to `(0.001, 0.011)`. The upper bound of `0.02` made the world nearly invisible at peak fog. The new range keeps the atmospheric effect alive while maintaining playable visibility.

---

## Challenges & Solutions

| Challenge | Solution |
|---|---|
| Cave interior passable (fell through floor/walls) | Reversed Marching Cubes triangle winding order to flip normals inward |
| `SetHoles` cave entrances never appeared | Replaced entirely with runtime portal placement |
| Player not teleporting despite detection | Disable `CharacterController` briefly during `transform.position` assignment |
| Player spawning outside cave in void | `Physics.Raycast` downward from portal to find actual cave mesh floor |
| Instant portal re-entry loop | 5-second cooldown tracked in `PlayerController` |
| Grid not ready when portal tries to scan it | Moved grid initialization from `Start()` to `Awake()` in `CaveGenerator` |

---

## Technical Summary

| Technique | Purpose | Script |
|---|---|---|
| `OnTriggerEnter` | Portal collision detection | Portal.cs |
| `Physics.Raycast` | Find safe cave floor landing position | Portal.cs |
| `terrain.SampleHeight` | Snap player to terrain surface on exit | Portal.cs |
| Triangle winding order reversal | Inward-facing normals for interior collision | CaveRenderer.cs |
| `Awake()` before `Start()` | Grid populated before portal spawn call | CaveGenerator.cs |
| `GetComponentInChildren<T>()` | Locate Portal script on child collider object | MultiBiomeTerrainGenerator.cs |
| Perlin-noise fog remapping | Atmospheric fog that doesn't obstruct gameplay | FogScheduler.cs |

---

## Future Plans

- **Village Generation**: Next iteration will use L-Systems to procedurally generate surface structures — roads, building footprints, and settlements — directly on the multi-biome terrain
- **Cave Lighting**: Dynamic point lights or torch placements inside caves to improve underground navigation without relying on emission-only lighting
- **Multiple Portal Pairs**: Expand to several cave-surface portal connections to make large cave networks fully explorable
