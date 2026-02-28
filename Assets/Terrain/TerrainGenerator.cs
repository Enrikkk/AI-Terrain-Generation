using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Terrain terrain;
    public float scale = 10f;
    public float heightMultiplier = 120f;

    void Start()
    {
        if(terrain == null)
            terrain = GetComponent<Terrain>();
        
        GenerateTerrain();
    }


    void GenerateTerrain()
    {
        // First, get the data of the terrain.
        TerrainData myTerrainData = terrain.terrainData;
        myTerrainData.size = new Vector3(myTerrainData.size.x, heightMultiplier, myTerrainData.size.z);

        int length = myTerrainData.heightmapResolution; // Map is length x length size.
        float[,] heights = new float[length, length]; // Array to store map heights.

        for(int x = 0; x < length; ++x)
        {
            for(int y = 0; y < length; ++y)
            {
                float xCoord = (float)x / length*scale;
                float yCoord = (float)y / length*scale;

                heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }

        myTerrainData.SetHeights(0, 0, heights);

    }



}
