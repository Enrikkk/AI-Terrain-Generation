# Project Iteration 4: Flow Fields, Flocks, and Textures

**GitHub Repository:** [Enrikkk/AI-Terrain-Generation](https://github.com/Enrikkk/AI-Terrain-Generation)
Welcome to the 4th iteration of the project! In this phase, the main goal was to breathe more life into the map by introducing dynamic entities and forces. Here is a breakdown of the thought process, implementations, and future plans.

## Initial Concepts vs. Reality

My initial idea was to add a water layer at the bottom of the map and populate the ponds with water strider insects. However, this approach presented several complex challenges:
- **Pond Detection:** Creating an algorithm to computationally scan the map for all existing water ponds (where water and terrain collide) is heavily complex.
- **Dynamic Grids:** It would require creating customized grids that adapt to the exact size and shape of each individual pond.
- **Asset Availability:** There were no suitable water strider insect prefabs available in the Unity Asset Store. 

Because of these hurdles, I decided to pivot to a different, equally exciting approach. 

## Environmental Upgrades

Even though the water striders were scrapped, the water itself stayed!
- **Water Layer:** Added a free water shader from the Unity Asset Store, giving the map a much cooler, vibrant tone.
- **Texture Fixes:** I was finally able to fix the texture applying algorithm! The map now correctly applies stone, dirt, and grass textures. 
- **Underwater Terrain:** The terrain directly underneath the water layer now uses a "dirt" texture, because having grass growing underwater just didn't make sense.

## The Bird Flow Field

Instead of insects, I decided to build a flow field for birds!

- **Grid System Visuals:** I implemented the Flow Field visuals based on class discussions, effectively creating a fully functioning Grid System. It took a good amount of hours to implement—even though it's conceptually simple in the end—but it works perfectly.
- **Flocking Vehicles:** I downloaded a free bird flocks asset from the Unity Asset Store, which includes prefabs for flocks of 1, 3, 5, 10, and 15 birds.
- **Polymorphism in Action:** I used these bird prefabs as my vehicles, implementing the vehicle `MonoBehaviour` script and all the code regarding the grid system and the flow field, heavily utilizing the laws of Polymorphism.
- **Polished Movement:** I tweaked their sizes, fixed their built-in Animator controllers so they actually flap their wings, and fine-tuned the physical forces (speed, mass, max force) applied to them. This creates incredibly smooth and dynamic bird flock movement that's interesting to watch.

## Interactive Flow Field Inputs

The flow field vectors aren't static; you can change them on the fly using keyboard inputs! The field defaults to a Perlin Noise distribution initially.

- **Press `P`**: Randomizes the arrows using **Perlin Noise**. I tweaked the noise scale so the arrows roughly point in a somewhat similar flow direction, taking wide sweeping paths. This distinguishes the movement from Gaussian noise!
- **Press `R`**: Randomizes the arrows using a **Gaussian Distribution**.

## Future Plans

This iteration took some time, but I am extremely glad with the results! Looking ahead, my plans are:
- **User Interface (UI):** Add an on-screen UI to show the user which keys to press and what they do.
- **Visibility Toggle:** Give the user an option to visually hide or show the flow field arrows so they can clearly see *why* the birds are moving the way they do.
- **Playable Game View:** Currently, the simulation is best explored by pressing Play and flying around in the Scene view. I want to make the game fully playable directly from the main Game view!
