# Procedural Terrain & Artificial Ecosystem Simulation

**GitHub Repository:** [Enrikkk/AI-Terrain-Generation](https://github.com/Enrikkk/AI-Terrain-Generation)
## Overview
This project focuses on implementing physics-based interactions, environmental dynamics, and artificial life into a procedural terrain system. Moving beyond static mesh generation, the environment features a Newtonian day/night cycle, dynamic atmospheric weather, procedural vegetation spawning, and a living ecosystem of flora and fauna. 

## Key Features

### 1. Physics-Based Day/Night Cycle
Instead of a simple rotation animation, the celestial bodies (Sun and Moon) are physical objects controlled by Newton’s Laws of Universal Gravitation.
* **Orbital Mechanics:** The Sun and Moon are Rigidbodies that orbit the terrain based on gravitational forces applied towards a central mass point. The orbit is maintained by balancing an initial tangential velocity vector against a constant centripetal gravitational pull. 
* **Cycle & Lighting:** The simulation runs on a complete 20-minute day/night cycle. Both celestial bodies feature child Directional Lights that dynamically track the map center, shifting light intensity and color temperature to simulate warm days and cold, dim nights.

### 2. Dynamic Weather & Atmosphere
Atmospheric depth is simulated using noise algorithms to enhance environmental realism.
* **Dynamic Fog:** Fog density is driven by Perlin Noise, creating a "rolling mist" effect where the thickness randomly varies over time. 
* **Day/Night Integration:** The fog's color interpolates based on the sun's position relative to the horizon, appearing as a gray mist during the day and shifting to pitch black at night.

### 3. Procedural Vegetation & Ecosystems
Vegetation generation is fully integrated into the terrain pipeline, bringing life and color to the map.
* **Biome Logic & LOD:** Trees spawn probabilistically based on height data, resulting in higher tree density in lower elevations (valleys and plains) while peaks remain barren. All trees utilize Level of Detail (LOD) groups to maintain high performance.
* **Cherry Blossoms:** To add vibrancy, approximately 10% of all generated trees are cherry blossoms.
* **Dynamic Wildlife (Bees):** To pollinate the cherry blossoms, bees actively fly around the tree canopies. Their movement is governed by a custom triple sine script that allows them to wander smoothly through 3D space. 
Additionally, Perlin Noise dynamically adjusts their velocity on the fly, making their flight patterns feel organic, varied, and unpredictable.

### 4. Interactivity
* **Map Regeneration:** The system supports runtime map regeneration, allowing users to create a completely unique seed and terrain layout on demand.

## Art & Textures
The custom textures for the Sun, Moon, cherry blossom trees, and bees were generated using Google Gemini (specifically utilizing the Nano Banana image generation model for the flora and fauna assets).

## Technical Implementation
* **Physics:** Utilizes Unity Rigidbody and Constant Force applications inside `FixedUpdate`.
* **Algorithms:** Relies heavily on Perlin Noise for terrain height, fog density, and dynamic bee velocity. Also implements probability functions for tree spawning and custom triple sine oscillation for entity movement.

## Known Issues
* **Terrain Texturing:** The terrain currently utilizes a default material. The logic for applying splat maps based on height and slope is still under development and is not functioning in this current build.

## Future Roadmap
* **Texture Implementation:** Resolve shader and layer issues to correctly apply grass, rock, and snow textures based on biome height.
* **Interactive Forces:** Implement a global wind system capable of pushing entities (like the bees) and interacting with the environment.
* **Expanded Artificial Life:** Add more wildlife and autonomous agents that wander the map using force-based steering behaviors or random movement patterns.
* **Player Implementation:** Introduce a controllable player character to allow users to experience the physical forces and living entities firsthand.
