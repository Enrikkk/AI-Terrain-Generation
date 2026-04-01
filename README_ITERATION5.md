# Project Iteration 5: Player Controller and Cave Systems

**GitHub Repository:** [Enrikkk/AI-Terrain-Generation](https://github.com/Enrikkk/AI-Terrain-Generation)

Welcome to the 5th iteration! This phase focused on two major components: implementing a fully functional first-person player controller and developing a procedurally-generated 3D cave system using cellular automata and Marching Cubes. This has been one of the most technically challenging iterations, involving complex algorithms, mathematical concepts, and debugging intricate mesh generation issues.

## Player Controller Implementation

The first major goal was to make the simulation actually playable from the game view, rather than just exploring it in the Scene view.

### First-Person Movement
- **WASD Movement**: Players can walk through the world at a base speed of 5 units/second with acceleration and friction physics
- **Shift Sprint**: Holding Shift doubles the movement speed to 15 units/second for fast traversal
- **Space Bar Jump**: Players can jump with forward momentum reduction to prevent infinite air control
- **Mouse Look**: Free-look camera control with pitch/yaw clamping and 2× sensitivity

### Flight Mode
One of the key interactive features:
- **Press `F`**: Toggle flight mode to fly freely in the camera direction
- **While Flying**: Use WASD to move and mouse to look around
- **Shift in Flight**: Increases flight speed by 3× (15 units/second → 45 units/second)
- **Space/Ctrl**: Move up/down while flying

The flight mode was essential for testing and exploring the cave system thoroughly during development.

## Flow Field Visualization Control

Enhanced the existing flow field system with a visibility toggle:
- **Press `H`**: Hide/show the grid arrows and cell visualization for the flow field. This lets players observe bird behavior with or without seeing the underlying force field.

## Cave System Development Journey

This was the centerpiece of the iteration—a fully procedural 3D cave system that players can explore. The development process involved multiple algorithmic approaches, mathematical challenges, and extensive debugging.

### Initial Approach: Cube Instancing

The first attempt was conceptually simple: generate a 3D boolean grid using cellular automata, then instantiate a cube for every AIR cell. This failed catastrophically:
- Thousands of GameObject instances caused severe performance degradation
- The visual result was a blocky, voxel-like structure with no smooth surfaces
- Memory usage made the caves impossible to explore

### Pivot to Marching Cubes Algorithm

Marching Cubes is the industry-standard approach for converting volumetric data (voxel grids) into smooth triangle meshes. Here's how it works:

**Core Concept:**
1. Process the 3D grid in groups of 8 neighboring grid corners (a "cube")
2. Determine which corners are solid (ROCK) and which are air using a bitmask
3. Use a lookup table (TriTable) to determine which triangles should be generated at the cube's surface boundary
4. Each triangle vertex sits at the midpoint of an edge between solid and air corners (not at the corners themselves)

This produces a smooth, continuous mesh instead of blocky cubes.

### Development Challenges and Solutions

**Challenge 1: TriTable Compilation Errors**
- **Problem**: Initial TriTable used wrong array syntax (`new int[256][]` instead of `new int[,]`)
- **Solution**: Switched to `new int[,]` with inferred dimensions

**Challenge 2: IndexOutOfRangeException**
- **Problem**: TriTable data was corrupted (copied from EdgeTable, which had invalid indices)
- **Solution**: Completely rewrote the table with the verified Paul Bourke Marching Cubes standard table

**Challenge 3: Missing TriTable Entries (192-255)**
- **Problem**: Table only had 192/256 entries; remaining cubeIndex values were skipped via bounds check, creating geometric gaps
- **Root Cause**: Entries 192-255 represent configurations with 6-7 solid corners. The corrupted data referenced edge indices that were buried inside solid rock, creating spikes and holes
- **Solution**: Derived all 64 missing entries mathematically using the **complement property**: `entry[X] = entry[255-X] with triangle winding reversed`. This ensures the surface geometry is correct without relying on guessed values

**Challenge 4: Mesh Gaps Between Adjacent Cubes**
- **Problem**: Each cube independently computed its edge midpoints, creating duplicate vertices. Adjacent cubes' surfaces had visible gaps
- **Solution**: Implemented **vertex welding** using a `Dictionary<Vector3, int>`. Before adding a vertex, check if one at that exact world position already exists. If so, reuse its index. This stitches cubes together into a single watertight mesh

**Challenge 5: Black/Invisible Mesh**
- **Problem**: Mesh was rendered but completely black, making it invisible
- **Cause**: No normals (direction each surface faces) or incorrect normals
- **Solution**:
  - Enabled **emission** on the material with a subtle warm stone glow `(0.08, 0.07, 0.06)` so surfaces are always faintly visible
  - Set material's **Render Face** to `Both` so surfaces are visible from inside and outside
  - Used `RecalculateNormals()` after vertex welding so lighting averages correctly across the continuous surface

**Challenge 6: Backface Culling Issues**
- **Problem**: Trying to duplicate triangles with reversed winding to make them visible from both sides created zero-length normals (averaging forward and backward canceled to zero)
- **Solution**: Used material's built-in **Render Face → Both** setting instead; this handles double-sided rendering without affecting normals

**Challenge 7: Missing Texture on Cave Surfaces**
- **Problem**: Rock texture wasn't appearing on the mesh despite being assigned to the material
- **Cause**: Mesh had no UV coordinates (texture mapping information)
- **Solution**: Implemented **triplanar mapping** that projects UVs from world position based on surface normal direction:
  - Floor/ceiling surfaces: project from X-Z (top-down)
  - Front/back walls: project from X-Y
  - Left/right walls: project from Y-Z
  - Blend the three projections using normal direction as weights
  - This ensures no stretching or rotation, and the texture looks correct from any angle

### Current Cave System Status

**What Works:**
- Procedurally generated caves using 30×30×30 cellular automata grid with 55% initial rock fill
- Smooth Marching Cubes mesh with proper lighting and visibility
- Rock texture applied via triplanar UV mapping with adjustable scale (default 0.1)
- Player can walk and fly inside caves; mesh collider allows realistic physics

**Visual Quality:**
- Caves have organic, naturally-flowing tunnel structures
- Low-poly but smooth appearance suitable for game exploration
- No major geometric artifacts or see-through gaps

**What Still Needs Work:**
- **Cave Entrances**: Terrain holes (SetHoles) not yet visible; need to debug coordinate mapping between cave grid space and terrain height/hole maps
- **Entrance Smoothing**: Bowl-shaped depressions around cave openings need refinement
- **Cave Entrance Count**: Currently randomized (1-3) but integration with terrain incomplete

## Technical Summary

| Technique | Purpose |
|---|---|
| **Cellular Automata (5 iterations)** | Generates realistic, connected cave geometry |
| **Marching Cubes (256 configurations)** | Converts voxel grid to smooth triangle mesh |
| **Vertex Welding** | Stitches adjacent cubes into watertight mesh |
| **Triplanar UV Mapping** | Applies texture without stretching on curved surfaces |
| **Emission + Render Face Both** | Makes cave interior visible with proper lighting |

## Future Plans

- **Cave Entrances**: Debug and complete the terrain integration so caves visibly connect to the surface
- **Advanced Lighting**: Consider adding dynamic lights inside caves or torches to improve navigation
- **Procedural Variation**: Experiment with different cellular automata rules or noise functions for diverse cave types
- **Village Generation**: Next iteration will use L-Systems to procedurally generate surface villages that complement the underground cave network

This iteration was immensely rewarding but challenging. The Marching Cubes algorithm, while conceptually elegant, required precise implementation and careful debugging to achieve a production-quality result. The cave system is now a solid foundation for future expansions!
