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
    private float biomeScale; // Specific scale used for biomes, not very big no make biomes big too (we don't want a 10x10 biome surrounded by other kind of biomes...)

    public BiomePreset mountainZone;
    public BiomePreset plainsZone;
    public BiomePreset desertZone;

    // Height values - for biome styling.
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

                // Array if layers.
                float[] splat = new float[3];

                if (height < 0.15f) // Grass.
                {
                    splat[0] = 1; splat[1] = 0; splat[2] = 0; // 1.0 = Full visibility, 0.0 = Invisible.
                }
                else if (height < 0.20f) // Dirt.
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
                
                if(height < grassHeight && Random.Range(0f, 1f) < 0.008f) { addTree = true; }
                else if(height < dirtHeight && Random.Range(0f, 1f) < 0.005f) { addTree = true; }
                else if(height >= dirtHeight && Random.Range(0f, 1f) < 0.003f) { addTree = true; }
                
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

    void Update()
    {
        
    }

}
