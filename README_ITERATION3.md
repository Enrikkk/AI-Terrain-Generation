# 🌸 Development Update: Breathing Life into the Map

**GitHub Repository:** [Enrikkk/AI-Terrain-Generation](https://github.com/Enrikkk/AI-Terrain-Generation)
For this project iteration, I decided to take a slightly different approach from the previous ones. Instead of making massive structural changes to the terrain, I wanted to focus on something equally important: adding some actual *life* to the world!

## ✨ What's New?

* **Cherry Blossom Trees:** I wanted to implement bees, but bees don't just fly around regular pine trees—they need flowers, right? So, I decided to introduce the much-loved cherry blossom tree to the environment. Now, roughly 10% of all trees generated on the map will be beautiful cherry blossoms!
* **Dynamic Bees:** What good is a flowering tree without pollinators? These cherry blossom trees now feature bees flying around them, adding a great sense of motion and vitality to the ecosystem. 

## ⚙️ Under the Hood

To make the bees look natural rather than robotic, I implemented a few cool math tricks:
* **Triple Sine Oscillation:** I created a custom triple sine script that allows the bees to wander freely and smoothly through all of the 3D space around the tree canopy.
* **Perlin Noise Velocity:** The bees don't just move at a constant, boring speed. I used *Perlin Noise* to dynamically adjust their velocity on the fly, making their flight patterns feel organic, varied, and unpredictable.

## 🎨 Art & Textures

All of the textures for both the cherry blossom trees and the bees were generated using Google Gemini's Nano Banana image generation model. I custom-prompted all of the assets myself to fit the aesthetic I was going for. 

The end result is a much more colorful, vivid map with a real sense of life in it!

## 🚀 What's Next?

Looking ahead, I want to keep expanding on this idea of a living, breathing environment. Some planned features include:
* **More Wildlife:** Adding other animals and entities to catch the player's attention as they explore.
* **Interactive Forces (Wind):** Implementing a global wind system that can push entities around (like our new bees) and interact with the environment.
* **Player Implementation:** Getting a controllable player into the map to actually experience these forces and entities firsthand!
