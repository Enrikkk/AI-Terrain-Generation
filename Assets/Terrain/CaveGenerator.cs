using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    // Attributes.
    [Header("Terrain Settings")]
    public int gridWidth = 100; // Cave system size.
    public int gridDepth = 100;
    public int gridHeight = 40;
    public int cellSize = 5;
    public double fillPercent = 0.45; // How filled the cave system initially is.
    public int smoothingIterations = 5; // Number of Celullar Automata Iterations that will be run in order to smooth the terrain.
    public int fillThreshold = 8; // Amount of rock cells that have to surround an actual cell for it to become/stay as a rock cell.

    [Header("Array of Grid Cells")]
    public bool[,,] grid;
    private int[,,] rocksSurroundingCell; // Amount of rock surrounding a single cell in the cave system.
    private List<Vector3Int> airCells = new List<Vector3Int>();

    void Awake()
    {
        // Auto-position the cave below the terrain base.
        Terrain terrain = Terrain.activeTerrain;
        if(terrain != null)
        {
            this.transform.position = new Vector3(
                terrain.transform.position.x,
                terrain.transform.position.y,
                terrain.transform.position.z
            );
        }

        // Fill up the grid.
        this.initialiseGrid();

        // Run smoothing iterations using cellular automata.
        for(int i = 0; i < this.smoothingIterations; ++i)
        {
            this.smoothCaveSystem();
        }

        this.countAirCells(); // Done here and once -> Allows portal creation in the cave system in a more efficient way.
    }

    private void countAirCells() 
    {
        // Collect all air cells from the middle Y layers (avoid top/bottom boundary rock).
        int yMin = this.gridHeight / 4;
        int yMax = (this.gridHeight * 3) / 4;

        for(int x = 1; x < this.gridWidth - 1; x++)
            for(int y = yMin; y < yMax; y++)
                for(int z = 1; z < this.gridDepth - 1; z++)
                    if(!this.grid[x, y, z] && y + 1 < this.gridHeight && this.grid[x, y + 1, z]) // AIR cell with ROCK directly below = floor
                        this.airCells.Add(new Vector3Int(x, y, z));
    }

    void Start()
    {
        // Hand the finished grid to the renderer.
        GetComponent<CaveRenderer>().RenderCave(this.grid, this.gridWidth, this.gridHeight, this.gridDepth, this.cellSize);
    }

    // Spawns a portal at a random AIR cell inside the cave and returns it.
    public GameObject SpawnPortalInsideCave(GameObject portalPrefab, Terrain terrain)
    {
        if(airCells.Count == 0)
        {
            Debug.LogWarning("No air cells found inside cave to spawn portal!");
            return null;
        }

        // Pick a random air cell.
        Vector3Int cell = airCells[Random.Range(0, airCells.Count)];

        // Convert grid position to world position (Y is negated — cave digs downward).
        Vector3 worldPos = this.transform.position + new Vector3(
            cell.x * this.cellSize,
           -cell.y * this.cellSize,
            cell.z * this.cellSize
        );

        GameObject portal = Instantiate(portalPrefab, worldPos, Quaternion.identity);
        portal.transform.localScale *= 2.5f;
        Portal portalComponent = portal.GetComponentInChildren<Portal>();
        portalComponent.setTerrain(terrain);
        portalComponent.setCavePortal(true);

        return portal;
    }

    // Auxiliary function to initialise the Grid.
    private void initialiseGrid()
    {
        this.grid = new bool[this.gridWidth, this.gridHeight, this.gridDepth]; // Create it.
        System.Random randomNumberGenerator = new System.Random(); // Random number generator -> Used to randomly assign (or not) a value to a grid cell (empty or full) based on it's probability (fillPercent).
        for(int x = 0; x < this.gridWidth; ++x)
        {
            for(int y = 0; y < this.gridHeight; ++y)
            {
                for(int z = 0; z < this.gridDepth; ++z)
                {
                    this.grid[x, y, z] = (this.fillPercent < randomNumberGenerator.NextDouble()); // If the obtained number is less than the fillPercent value, we generate a non-empty cell.
                }
            }
        }
    }

    // Main cave system smoother.
    private void smoothCaveSystem()
    {
        // First we get the surrounding rock cell count for each cell.
        this.fillSurroundingRockCellCount();

        // Afterwards, we update the actual grid based on this.
        for(int x = 0; x < this.gridWidth; ++x)
        {
            for(int y = 0; y < this.gridHeight; ++y)
            {
                for(int z = 0; z < this.gridDepth; ++z)
                {
                    this.grid[x, y, z] = (this.rocksSurroundingCell[x, y, z] >= this.fillThreshold);
                }
            }
        }
    }

    // Auxiliar function to fill up the array of surrounding rock cell count.
    private void fillSurroundingRockCellCount()
    {
        // First, initialize count array.
        this.rocksSurroundingCell = new int[this.gridWidth, this.gridHeight, this.gridDepth];

        // Now, fill it up.
        for(int x = 0; x < this.gridWidth; ++x)
        {
            for(int y = 0; y < this.gridHeight; ++y)
            {
                for(int z = 0; z < this.gridDepth; ++z)
                {
                    int rockCount = 0;
                    for(int nx = x - 1; nx <= x + 1; ++nx)
                    {
                        for(int ny = y - 1; ny <= y + 1; ++ny)
                        {
                            for(int nz = z - 1; nz <= z + 1; ++nz)
                            {
                                if(nx == x && ny == y && nz == z) continue; // Skip the cell itself.
                                // Out-of-bounds cells are treated as ROCK (solid walls on cave edges).
                                if(nx < 0 || nx >= this.gridWidth || ny < 0 || ny >= this.gridHeight || nz < 0 || nz >= this.gridDepth)
                                    rockCount++;
                                else if(this.grid[nx, ny, nz])
                                    rockCount++;
                            }
                        }
                    }

                    this.rocksSurroundingCell[x, y, z] = rockCount;
                }
            }
        }
    }

    // Punches holes in the terrain and carves smooth bowl depressions at the shallowest cave entrance locations.
    private void CreateCaveEntrances(Terrain terrain) // Not used -> Re`ñaced by portal logic.
    {
        if(terrain == null) return;

        TerrainData data = terrain.terrainData;
        int holesRes = data.holesResolution;
        int hmRes = data.heightmapResolution;

        // --- Pass 1: collect all X/Z columns that have at least one AIR cell, record the shallowest y. ---
        List<Vector3Int> candidates = new List<Vector3Int>();
        for(int x = 0; x < this.gridWidth; x++)
        {
            for(int z = 0; z < this.gridDepth; z++)
            {
                for(int y = 0; y < this.gridHeight; y++)
                {
                    if(!this.grid[x, y, z])
                    {
                        candidates.Add(new Vector3Int(x, y, z));
                        break; // Only the topmost AIR cell per column.
                    }
                }
            }
        }

        // --- Pass 2: sort by y ascending — smallest y = cave ceiling closest to terrain surface. ---
        candidates.Sort((a, b) => a.y.CompareTo(b.y));

        // --- Pass 3: keep only the shallowest N entrances. ---
        int maxEntrances = Random.Range(1, 4); // 1 to 3 entrances, different each run.
        int count = Mathf.Min(maxEntrances, candidates.Count);

        // --- Prepare the hole map (all solid) and read the current height map. ---
        bool[,] holes = new bool[holesRes, holesRes];
        for(int i = 0; i < holesRes; i++)
            for(int j = 0; j < holesRes; j++)
                holes[i, j] = true;

        float[,] heights = data.GetHeights(0, 0, hmRes, hmRes);

        // --- Pass 4: for each chosen entrance, punch a hole and carve a smooth bowl. ---
        for(int i = 0; i < count; i++)
        {
            int x = candidates[i].x;
            int y = candidates[i].y;
            int z = candidates[i].z;

            float worldX = terrain.transform.position.x + x * this.cellSize;
            float worldZ = terrain.transform.position.z + z * this.cellSize;

            // Hole map index.
            int holeX = Mathf.RoundToInt(((worldX - terrain.transform.position.x) / data.size.x) * (holesRes - 1));
            int holeZ = Mathf.RoundToInt(((worldZ - terrain.transform.position.z) / data.size.z) * (holesRes - 1));
            holes[holeZ, holeX] = false;

            // Height map index.
            int hx = Mathf.RoundToInt(((worldX - terrain.transform.position.x) / data.size.x) * (hmRes - 1));
            int hz = Mathf.RoundToInt(((worldZ - terrain.transform.position.z) / data.size.z) * (hmRes - 1));

            // Dynamic depth: 30% of the gap between terrain surface and cave ceiling, clamped to a safe range.
            float terrainSurfaceY = terrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + terrain.transform.position.y;
            float caveCeilingWorldY = this.transform.position.y - (y * this.cellSize);
            float gap = terrainSurfaceY - caveCeilingWorldY;
            float entranceDepth = Mathf.Clamp((gap / data.size.y) * 0.3f, 0.01f, 0.08f);

            SmoothCaveEntrance(heights, hx, hz, hmRes, 8, entranceDepth);
        }

        data.SetHeights(0, 0, heights);
        data.SetHoles(0, 0, holes);
    }

    // Carves a smooth cosine bowl into the height map centered at (centerHX, centerHZ).
    private void SmoothCaveEntrance(float[,] heights, int centerHX, int centerHZ, int hmRes, int radius, float entranceDepth)
    {
        for(int dx = -radius; dx <= radius; dx++)
        {
            for(int dz = -radius; dz <= radius; dz++)
            {
                int hx = centerHX + dx;
                int hz = centerHZ + dz;

                if(hx < 0 || hx >= hmRes || hz < 0 || hz >= hmRes) continue;

                float dist = Mathf.Sqrt(dx * dx + dz * dz);
                if(dist > radius) continue; // Circle shape, not square.

                float falloff = (1f + Mathf.Cos(Mathf.PI * dist / radius)) / 2f;
                heights[hz, hx] -= entranceDepth * falloff;
                heights[hz, hx] = Mathf.Max(0f, heights[hz, hx]); // Never carve below terrain floor.
            }
        }
    }

    void Update()
    {
        
    }
}
