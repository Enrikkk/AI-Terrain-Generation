# Procedural Ecosystem Terrain Generator

## Project Overview

This project represents the first iteration of an ecosystem simulation, inspired by concepts found in *The Nature of Code*. The initial goal was to simulate a living ecosystem (such as a sea, lake, or forest), but the focus of this phase shifted to establishing the physical foundation: **Procedural Map Generation**.

This project implements a multi-biome terrain system using Perlin Noise within the Unity Engine.

## Features

* **Procedural Terrain Generation:** Utilizes Perlin Noise to create varied landscapes rather than static meshes.
* **Multi-Biome System:** The map is divided into three distinct zones, determined by a secondary Perlin Noise layer:
* **Plains:** Lower height, gentler slopes.
* **Dirt/Wasteland:** Intermediate terrain.
* **Mountains:** High altitude, steep scale.


* **Adaptive Smoothing:** Implements the **Box Blur** algorithm to mitigate sharp cliffs and height disparities where different biomes meet.
* **Custom Assets:** Includes AI-generated, cartoon-style textures for the terrain layers.

## Implementation Details

### 1. Map Generation Logic

The core generation logic is handled in the `MultiBiomeTerrainGenerator.cs` script (located in the `Assets/` folder).

* **Heightmap:** Standard Perlin Noise is used to generate the base geometry.
* **Biome Distribution:** A second layer of Perlin Noise determines which biome appears in a specific coordinate.
* Each biome has roughly a 33% chance of appearance.
* The biome selection dynamically alters the **Scale** and **Height Multiplier** of the terrain at that location.



### 2. Terrain Smoothing

To address the issue of jagged cliffs forming at the borders between biomes (due to sudden changes in height multipliers), a **Box Blur** algorithm was implemented. This post-processing pass averages neighboring pixel heights to create smoother transitions between zones.

### 3. Texturing & Assets

* **Textures:** The project utilizes custom textures generated using **Google Gemini's AI** (Nano Banana Pro model).
* **Style:** The assets were generated with a specific prompt to achieve a stylized, "cartoon-like" aesthetic.
* **Application:** Textures are intended to be applied procedurally based on height and biome type.

## Current Status & Known Issues

While the geometry generation is functional, there are current limitations in the rendering pipeline:

* **Texture Blending:** Currently, there is a bug where only the grass texture appears on the terrain.
* **Lighting/Color:** The rendered texture appears darker than intended (likely due to material settings or lighting configurations).
* **Smoothing:** While Box Blur has improved the terrain, some transitions between biomes remain slightly rough or unsmooth.

## Learning Outcomes

This project required significant self-directed learning outside of the standard class curriculum. Key technical skills acquired include:

* **Unity Terrain System:** Understanding `TerrainData`, heightmaps, and resolution settings.
* **C# Scripting for Physics:** Implementing mathematical noise functions and smoothing algorithms.
* **Asset Management:** importing and configuring custom Terrain Layers.
