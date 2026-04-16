using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BiomePreset // "Struct" used for biome generation.
{
    public float scale;
    public float heightMultiplier;
}

public class MultiBiomeTerrainGenerator : MonoBehaviour
{

    public Terrain terrain;
    public GameObject beePrefab;
    public GameObject waterPrefab;
    public List<GameObject> portalPrefabs;
    public CaveGenerator caveGenerator;
    private float biomeScale; // Specific scale used for biomes, not very big no make biomes big too (we don't want a 10x10 biome surrounded by other kind of biomes...)

    public BiomePreset mountainZone;
    public BiomePreset plainsZone;
    public BiomePreset desertZone;

    // Height values - for biome styling.
    // Water height is randomized each generation within a small, natural-looking range.
    private float waterHeight;
    private float grassHeight = 0.15f;
    private float dirtHeight = 0.20f;

    void Start()
    {
        // Prefab for the bees.
        beePrefab = Resources.Load<GameObject>("Bee");

        // Initialize biome related parameters.
        biomeScale = 5f;

        // Mountains zone.
        mountainZone = new BiomePreset();
        mountainZone.scale = 17f;
        mountainZone.heightMultiplier = 70f;

        // Plains zone.
        plainsZone = new BiomePreset();
        plainsZone.scale = 15f;
        plainsZone.heightMultiplier = 30f;

        // Desert zone.
        desertZone = new BiomePreset();
        desertZone.scale = 20f;
        desertZone.heightMultiplier = 15f;

        // Randomize water level slightly each generation (between dry and flooded-looking maps)
        waterHeight = Random.Range(0.06f, 0.10f);

        // Initialize portal prefabs list.
        //this.portalPrefabs = new List<GameObject>();

        if(terrain == null)
            terrain = GetComponent<Terrain>();
        
        // Establish maximum height as the height given to mountains.
        terrain.terrainData.size = new Vector3(terrain.terrainData.size.x, mountainZone.heightMultiplier, terrain.terrainData.size.z);
        GenerateMultiBiomeTerrain();
    }

    void GenerateMultiBiomeTerrain()
    {
        TerrainData myTerrainData = terrain.terrainData;
        int length = myTerrainData.heightmapResolution;
        float[,] heights = new float[length, length];
        
        int seed = Random.Range(0, 10000); // For random seed map generation.

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float biomeX = (float)x / length * biomeScale;
                float biomeY = (float)y / length * biomeScale;
                float biomeValue = Mathf.PerlinNoise(biomeX+seed, biomeY+seed);

                float currentScale = 0f;
                float currentHeightMulti = 0f;

                if (biomeValue < 0.33f) // Mountains.
                {
                    currentScale = mountainZone.scale;
                    currentHeightMulti = mountainZone.heightMultiplier;
                }
                else if (biomeValue < 0.66f) // Plains.
                {
                    currentScale = plainsZone.scale;
                    currentHeightMulti = plainsZone.heightMultiplier;
                }
                else // Desert.
                {
                    currentScale = desertZone.scale;
                    currentHeightMulti = desertZone.heightMultiplier;
                }

                float detailX = (float)x / length * currentScale;
                float detailY = (float)y / length * currentScale;
                
                heights[x, y] = Mathf.PerlinNoise(detailX, detailY) * (currentHeightMulti / myTerrainData.size.y);
            }
        }
        
        heights = SmoothTerrain(heights, 3); // Smooth the terrain so it doesn
        myTerrainData.SetHeights(0, 0, heights);

        // Final functions for adding more life to the terrain.
        PaintTerrain();
        GenerateTrees();
        SpawnWater();

        for(int i = 0; i < Random.Range(5, 15); ++i) // Generate random amount of surface-cave portal pairs.
        {
            GameObject surfacePortal = SpawnPortal("random"); // Spawn random surface portal.

            // Spawn a random cave portal.
            if(caveGenerator != null)
            {
                GameObject cavePortal = caveGenerator.SpawnPortalInsideCave(portalPrefabs[Random.Range(0, portalPrefabs.Count)], this.terrain);
                if(cavePortal != null)
                    PairPortals(surfacePortal, cavePortal); // Pair them (you can teleport from one to the other and vice-versa).
            }
        }
    }

    void PairPortals(GameObject portalA, GameObject portalB)
    {
        Portal scriptA = portalA.GetComponentInChildren<Portal>();
        Portal scriptB = portalB.GetComponentInChildren<Portal>();

        if (scriptA != null && scriptB != null)
        {
            scriptA.otherPortal = portalB;
            scriptB.otherPortal = portalA;
        }
    }

    // Auxiliar function to paint the terrain.
    void PaintTerrain()
    {
        TerrainData data = terrain.terrainData;

        int mapWidth = data.alphamapWidth; // Map texture pixel dimension.
        int mapHeight = data.alphamapHeight;

        // Map to contain the textures.
        float[,,] splatmapData = new float[mapWidth, mapHeight, 3];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float y_01 = (float)y / mapHeight;
                float x_01 = (float)x / mapWidth;

                float height = data.GetInterpolatedHeight(y_01, x_01) / data.size.y; // height in meters and normalized.

                // Array of layers.
                float[] splat = new float[3];

                if (height < waterHeight + 0.03) // Underwater (Dirt/Sand)
                {
                    splat[0] = 0; splat[1] = 1; splat[2] = 0; // Dirt
                }
                else if (height < grassHeight + 0.15f) // Grass.
                {
                    splat[0] = 1; splat[1] = 0; splat[2] = 0; // 1.0 = Full visibility, 0.0 = Invisible.
                }
                else if (height < dirtHeight + 0.15f) // Dirt.
                {
                    splat[0] = 0; splat[1] = 1; splat[2] = 0;
                }
                else // Mountain.
                {
                    splat[0] = 0; splat[1] = 0; splat[2] = 1;
                }

                for (int i = 0; i < 3; i++)
                {
                    splatmapData[x, y, i] = splat[i];
                }
            }
        }

        data.SetAlphamaps(0, 0, splatmapData); // Apply coloring
    }

    // Auxiliar function to smooth the terrain.
    float[,] SmoothTerrain(float[,] rawHeights, int iterations)
    {
        int width = rawHeights.GetLength(0);
        int height = rawHeights.GetLength(1);

        float[,] smoothedHeights = new float[width, height];

        for (int i = 0; i < iterations; i++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float totalHeight = 0f;
                    int count = 0;

                    for (int nX = x - 1; nX <= x + 1; nX++)
                    {
                        for (int nY = y - 1; nY <= y + 1; nY++)
                        {
                            if (nX >= 0 && nX < width && nY >= 0 && nY < height)
                            {
                                totalHeight += rawHeights[nX, nY];
                                count++;
                            }
                        }
                    }

                    smoothedHeights[x, y] = totalHeight / count;
                }
            }
            
            if (i < iterations - 1)
                rawHeights = (float[,])smoothedHeights.Clone();
        }

        return smoothedHeights;
    }

    // Logic for tree generation.
    void GenerateTrees()
    {
        TerrainData data = terrain.terrainData;
        data.treeInstances = new TreeInstance[0];
        List<TreeInstance> treeList = new List<TreeInstance>();
        int spawnedBees = 0;

        // Optional: Create an empty parent object to hold all the bees so your hierarchy stays clean
        GameObject beeContainer = new GameObject("Bee Swarm Container");
        if(beePrefab == null)
            Debug.Log("beePrefab is null");

        for(int x = 0; x < data.size.x; x += 1)
        {
            for(int z = 0; z < data.size.z; z += 1)
            {
                float normX = (float)x / data.size.x;
                float normZ = (float)z / data.size.z;
                bool addTree = false;
                
                float height = data.GetInterpolatedHeight(normX, normZ) / data.size.y;
                
                // Do not spawn trees underwater!
                if(height > waterHeight) 
                {
                    if(height < grassHeight && Random.Range(0f, 1f) < 0.008f) { addTree = true; }
                    else if(height < dirtHeight && Random.Range(0f, 1f) < 0.005f) { addTree = true; }
                    else if(height >= dirtHeight && Random.Range(0f, 1f) < 0.003f) { addTree = true; }
                }
                
                if(addTree) 
                {
                    TreeInstance tree = new TreeInstance();
                    tree.position = new Vector3(normX, height, normZ); 
                    
                    bool isCherryBlossom = Random.Range(0, 10) == 0; 
                    
                    tree.prototypeIndex = isCherryBlossom ? 1 : 0; // Cherry blossom index = 1 -> Normal index = 0.
                    
                    tree.widthScale = Random.Range(0.7f, 1.3f);
                    tree.heightScale = Random.Range(0.5f, 1.5f);
                    tree.color = Color.white;
                    tree.lightmapColor = Color.white;

                    treeList.Add(tree);

                    if(isCherryBlossom && beePrefab != null)
                    {
                        float worldX = terrain.transform.position.x + (normX * data.size.x);
                        float worldY = terrain.transform.position.y + (height * data.size.y);
                        float worldZ = terrain.transform.position.z + (normZ * data.size.z);
                        
                        float treeTopHeight = worldY + (tree.heightScale * 5f); 
                        Vector3 spawnPos = new Vector3(worldX, treeTopHeight, worldZ);

                        int beeCount = Random.Range(2, 5);
                        spawnedBees += beeCount;
                        for (int i = 0; i < beeCount; i++)
                        {
                            GameObject newBee = Instantiate(beePrefab, spawnPos, Quaternion.identity);
                            newBee.transform.parent = beeContainer.transform;
                        }
                    }
                }
            }
        }

        data.treeInstances = treeList.ToArray();    
        terrain.Flush();                            
        Debug.Log("Tree Generation Complete. Total Trees Planted: " + treeList.Count);
        Debug.Log("Bees flying around: " + spawnedBees);
    }

    // Spawns the water plane at the correct world-space height and scale.
    void SpawnWater()
    {
        if (waterPrefab == null)
        {
            Debug.LogWarning("waterPrefab is not assigned! Please drag a URP water prefab into the Inspector.");
            return;
        }

        TerrainData data = terrain.terrainData;
        float terrainWidth  = data.size.x;
        float terrainLength = data.size.z;

        // Convert normalized waterHeight to world-space Y position.
        float worldWaterY = terrain.transform.position.y + (waterHeight * mountainZone.heightMultiplier);

        // Center of the terrain in world space.
        float centerX = terrain.transform.position.x + terrainWidth  / 2f;
        float centerZ = terrain.transform.position.z + terrainLength / 2f;

        GameObject waterObj = Instantiate(waterPrefab, new Vector3(centerX, worldWaterY, centerZ), Quaternion.identity);
        waterObj.name = "WaterSurface";

        // Unity's default Plane primitive is 10x10 units, so divide terrain size by 10.
        // If your prefab uses a different base size, adjust accordingly.
        waterObj.transform.localScale = new Vector3(terrainWidth / 10f, 1f, terrainLength / 10f);

        Debug.Log($"Water spawned at Y={worldWaterY:F2} (normalized waterHeight={waterHeight:F3})");
    }

    // Function to spawn a portal (randomly portal style chosen) in the middle of the map.
    GameObject SpawnPortal(string position)
    {
        if (this.portalPrefabs.Count == 0) // List is empty -> No profabs have been assigned -> Give error. 
        {
            Debug.LogWarning("Portal Prefabs have not been assigned!! Please assign your prefabs to the portalPrefabs list.");
        }

        // Then, if list of prefabs have been assigned we just proceed to create one based on the given position.
        // First, get terrain data.
        TerrainData data = terrain.terrainData;
        float terrainWidth = data.size.x;
        float terrainLength = data.size.z;
        
        float terrainCenterX = terrain.transform.position.x + terrainWidth/2f;
        float terrainCenterZ = terrain.transform.position.z + terrainLength/2f;
        float terrainBaseY = terrain.transform.position.y; // Base terrain position, afterwards we will have to calculate the height of the given position (it might be a hill, for example).

        float portalX = 0f;
        float portalZ= 0f;

        if (position == "random")
        {
            float shiftX = Random.Range(0.0f, 1.0f); // Generate a float between 0 and 1.
            float shiftZ = Random.Range(0.0f, 1.0f);

            // Instantiate a random position for the portal.
            // Shift it (up to) half of the map size of a given dimension since we are starting on the middle.
            // The -5 is so that the portal isn't generated just at the very edge of the map (unlikely but possible).
            portalX = terrainCenterX + (Random.value > 0.5f ? 1 : -1) * (shiftX * (terrainWidth / 2f) - 5f);
            portalZ = terrainCenterZ + (Random.value > 0.5f ? 1 : -1) * shiftZ * (terrainLength / 2f) - 5f;
        }
        else if (position == "center")
        {
            portalX = terrainCenterX;
            portalZ = terrainCenterZ;
        }


        // Quaternion.identity to make portal not be rotated/shifted due to initial position of the parent object.
        // We dynamically sample the Y coordinate from the terrain given the X and Z coordinates.
        // In the terrain.SampleHeight function we give 0 as the Y input because Unity just ignores that value, so anything could be used there.
        GameObject portal = Instantiate(this.portalPrefabs[Random.Range(0, portalPrefabs.Count)], new Vector3(portalX, terrain.SampleHeight(new Vector3(portalX, 0, portalZ)), portalZ), Quaternion.identity);
        Portal portalComponent = portal.GetComponentInChildren<Portal>();
        portalComponent.setTerrain(this.terrain);
        portal.transform.localScale *= 2.5f; // Make the portal bigger.
        return portal;
    }

    void Update()
    {
        
    }

}
