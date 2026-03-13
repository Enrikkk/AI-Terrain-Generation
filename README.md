# AI Procedural Terrain Generation & Artificial Ecosystem Simulation

**GitHub Repository:** [Enrikkk/AI-Terrain-Generation](https://github.com/Enrikkk/AI-Terrain-Generation)

## Overview

This project is a continuously evolving Unity simulation that combines procedural generation, physics-based interactions, and artificial life into a dynamic, living ecosystem. Instead of a static map, the environment features a Newtonian day/night cycle, atmospheric weather, highly procedural vegetation, and autonomous entities that navigate the terrain via flow fields and mathematical oscillations.

This project was built iteratively to explore advanced topics in AI for Game Development.

---

## Project Evolution & Iterations

### [Iteration 1: Setting the Foundation](./README_ITERATION1.md)
The first iteration laid the groundwork for the core simulation. 
* **Procedural Environments:** Integrated dynamic fog driven by Perlin Noise that shifts into pitch black during nighttime.
* **Flora & Fauna Initialization:** Established the base structure for procedural trees, specifically implementing rare Cherry Blossom variations (10% spawn chance) alongside the initial logic for buzzing bees around the canopies, controlled via Custom Triple Sine mathematical oscillation.
* **Runtime Regeneration:** Allowed the user to regenerate a completely new seeded map on demand.

### [Iteration 2: The Physics Framework](./README_ITERATION2.md)
The second iteration transformed the static skybox into a physically accurate universe.
* **Newtonian Heavenly Bodies:** The Sun and the Moon were converted to physical Rigidbodies. Instead of basic rotation scripting, they orbit the terrain using Newton's Laws of Universal Gravitation, balancing centripetal force and tangential velocity.
* **Dynamic Lighting Shifts:** Directional lights attached to the orbiting bodies were programmed to aggressively track the map center and shift color temperature to naturally simulate day (warm) and night (cold).
* **Biome Logic & LODs:** Implemented probabilistic tree spawning based on elevation heights (valleys vs peaks) and hooked the trees into Unity's LOD groups for vast performance improvements.

### [Iteration 3: Breathing Life into the Map](./README_ITERATION3.md)
The third iteration focused completely on creating a vibrant, moving ecosystem instead of just adding more raw terrain features.
* **Dynamic Pollinators:** Greatly refined the bees around the Cherry Blossom trees. 
* **Organic Flight Patterns:** Instead of just simple sine wave oscillation, Perlin Noise was layered into the bees' velocity vectors. This combination allowed the bees to wander naturally and unpredictably through the 3D space of the tree canopy.
* **Generative Art Generation:** Assets and textures for the Sun, Moon, trees, and specifically the bees were AI-generated through Google Gemini's image models to fit a cohesive aesthetic perfectly.

### [Iteration 4: Flow Fields, Flocking & Shaders](./README_ITERATION4.md)
The fourth iteration heavily expanded the map's complexities, adding water shaders and a massive flocking algorithm system.
* **Aesthetic Water & Textures:** Solved underlying logic bugs in the splat map application to accurately blend rock, dirt, and grass textures according to terrain height. A new water layer was added utilizing a gorgeous Unity Asset Store shader, while forcing the underlying basin terrain to exclusively utilize the "dirt" texture.
* **Grid Systems & Flow Fields:** A robust, invisible 100x100 Grid System was programmed to drape directly over the map. Every cell inside the grid calculates a specific directional vector represented physically via arrows.
* **Dynamic Bird Flocks:** Flocking bird prefabs (varying from 1 to 15 birds per flock) were introduced to act as "Vehicles". Their individual animators were forcibly de-synchronized for organic wing-flapping. They accurately read the arrow vector stored deeply in their local Grid cell, steering smoothly and gracefully utilizing mass and maxForce values toward its heading. 
* **Interactive Winds:** The underlying grid vectors can be universally shifted via Keyboard inputs:
  * `P`: Shuffles the universal flow field using scaled **Perlin Noise** to ensure birds take massive, sweeping, unified paths over the map.
  * `G`: Shuffles the universal flow field randomly via a **Gaussian Distribution**.

---

## Future Roadmap
- **UI System:** Bring an explicit User Interface overlay to document interactable controls for the user inside the game.
- **Visibility Toggles:** Allow users to hide or reveal the Flow Field system vectors during runtime so they can visually "debug" the flock's decisions.
- **Game View Transition:** Expand the simulation beyond the Scene window so that everything is interactable entirely from the Playable Game View camera.
