# Procedural Terrain & Physics Simulation (Iteration 2)

## Overview

This iteration focuses on implementing physics-based interactions and environmental dynamics into the procedural terrain system. The major update introduces a Newtonian day/night cycle and procedural vegetation spawning, moving beyond static mesh generation.

## Key Features

### 1. Physics-Based Day/Night Cycle

Instead of a simple rotation animation, the celestial bodies (Sun and Moon) are physical objects controlled by **Newton’s Laws of Universal Gravitation**.

* **Orbital Mechanics:** The Sun and Moon are Rigidbodies that orbit the terrain based on gravitational forces applied towards a central mass point.
* **Forces Applied:** The orbit is maintained by balancing an initial velocity vector (tangential) against a constant gravitational pull (centripetal force).
* **Cycle Duration:** The simulation is tuned for a complete 20-minute day/night cycle.
* **Lighting:** Both celestial bodies have child Directional Lights that dynamically track the map center. Light intensity and color temperature shift to simulate realistic day (warm/bright) and night (cold/dim) ambients.
* **Assets:** Sun and Moon spheres utilize custom textures generated via Google Gemini.

### 2. Dynamic Weather & Atmosphere

Atmospheric depth has been added to enhance realism, utilizing noise algorithms for variation.

* **Dynamic Fog:** Fog density is not static; it is driven by **Perlin Noise**, creating a "rolling mist" effect where thickness varies randomly over time.
* **Day/Night Integration:** Fog color interpolates based on the sun's position relative to the horizon—appearing as a gray mist during the day and shifting to pitch black at night.

### 3. Procedural Vegetation (Trees)

Vegetation generation has been added to the terrain generation pipeline.

* **Biome Logic:** Trees spawn probabilistically based on height data. Lower elevations (valleys/plains) have a higher density of trees, while higher elevations (peaks) remain barren.
* **LOD Support:** Trees are instantiated with Level of Detail (LOD) groups to maintain performance.

### 4. Interactivity

* **Map Regeneration:** The system allows for runtime regeneration, creating a completely unique seed and terrain layout on demand.

## Technical Implementation

* **Physics:** Unity Rigidbody & Constant Force application (`FixedUpdate`).
* **Algorithms:** Perlin Noise (Terrain Height & Fog Density), Probability functions (Tree Spawning).

## Known Issues

* **Terrain Texturing:** Currently, the terrain utilizes a default material. The logic for applying splat maps based on height/slope is still under development and not functioning in this build.

## Future Roadmap (Iteration 3)

* **Texture Implementation:** Resolve shader/layer issues to correctly apply grass, rock, and snow textures based on biome height.
* **Artificial Life:** Implement autonomous agents (creatures) that wander the map using force-based steering behaviors or random movement patterns.
