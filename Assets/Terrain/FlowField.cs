using UnityEngine;

public class FlowField : MonoBehaviour
{
    
    public GridSystem gridSystem { get; private set; }
    [SerializeField] private GridSystemVisual gridSystemVisual;
    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private Vehicle[] vehiclePrefabs;
    [SerializeField] private int vehicleSpawnCount = 15;

    private void Awake() {
        // Grid size (in terms of boxes) and box size. Summed up means a 1000x1000 terrain size.
        gridSystem = new GridSystem(10, 10, 100, this.transform.position, gridSystemVisual, InstantiateFlowFieldObject);
        
        // Pass the initialized gridSystem to the visual component
        gridSystemVisual.SetGridSystem(gridSystem);
        
        // Create the debug objects so they appear in-game
        gridSystem.CreateDebugObjects(gridDebugObjectPrefab, gridSystemVisual.transform);

        this.PerlinNoiseFlowFieldArrows();
        this.SpawnVehicles();
    }
    
    private void SpawnVehicles() {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0) return;
        
        float minX = this.transform.position.x;
        float minZ = this.transform.position.z;
        float maxX = minX + (gridSystem.GetWidth() * gridSystem.GetCellSize());
        float maxZ = minZ + (gridSystem.GetHeight() * gridSystem.GetCellSize());

        for (int i = 0; i < vehicleSpawnCount; i++) {
            // Generate a random position strictly inside the boundaries of the Flow Field map.
            Vector3 randomPosition = new Vector3(
                Random.Range(minX, maxX),
                this.transform.position.y,
                Random.Range(minZ, maxZ)
            );

            // Pick a random specific bird prefab from our array (e.g. 1 bird, 5 birds, 15 birds flock)
            Vehicle randomPrefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];

            // Instantiate a new randomly-sized bird group!
            Vehicle newVehicle = Instantiate(randomPrefab, randomPosition, Quaternion.identity);
            
            // Scale the birds up by 5 times.
            newVehicle.transform.localScale = Vector3.one * 12f;

            // Link it to the grid so it can fetch its own arrows.
            newVehicle.assignFlowField(this);
        }
    }

    private void Update()
    {
        if (gridSystem != null)
        {
            gridSystem.SetOrigin(this.transform.position);

            // Draw debug lines.
            gridSystem.drawDebugLines();
        }

        // Check if we have pressed the R key. In that case, randomize the direction of the flow field arrows.
        if(Input.GetKeyDown(KeyCode.R)) { RandomizeFlowFieldArrows(); }
        if(Input.GetKeyDown(KeyCode.P)) { PerlinNoiseFlowFieldArrows(); }
    }

    // Function to randomize the direction of all the arrows in the flow field.
    private void RandomizeFlowFieldArrows() {
        Debug.Log("Randomizing Arrows");
        for(int x = 0; x < this.gridSystem.GetWidth(); ++x) {
            for(int z = 0; z < this.gridSystem.GetWidth(); ++z) {
                GridObjectFlowFieldArrow arrowObject = gridSystem.GetGridObject(x, z) as GridObjectFlowFieldArrow;
                arrowObject.SetArrowRotation(Random.Range(0f, 360f));
            }
        }
    }

    private void PerlinNoiseFlowFieldArrows() {
        Debug.Log("Perlin Randomizing Arrows");
        float incrementAmount = 0.05f;
        float xCoordinate = Random.Range(0f, 0.5f);
        float zCoordinate = Random.Range(0f, 0.5f);

        for (int x = 0; x < gridSystem.GetWidth(); x++) {
            for (int z = 0; z < gridSystem.GetHeight(); z++) {
                float perlinValue = Mathf.PerlinNoise(xCoordinate, zCoordinate);

                //perlinValue will be a number between 0-1.
                //we want to map this to a value between 0-360.
                float arrowRotation = Unity.Mathematics.math.remap(0f, 1f, 0f, 360f, perlinValue);
                GridObjectFlowFieldArrow arrowObject = gridSystem.GetGridObject(x, z) as GridObjectFlowFieldArrow;
                arrowObject.SetArrowRotation(arrowRotation);
                zCoordinate += incrementAmount;
            }
            xCoordinate += incrementAmount;
        }
    }

    // Function to create a random FlowFieldObjectArrow for our flow field.
    private GridObject InstantiateFlowFieldObject(GridSystem gridSystem, GridPosition gridPosition) {
        float arrowRotation = Random.Range(0f, 360f);
        return new GridObjectFlowFieldArrow(gridSystem, gridPosition, arrowRotation);
    }

    /*private void OnDrawGizmos()
    {
        // Preview grid settings only when selected or visible in Scene view
        // Hard-coding the Awake values (10x10 with size 100) for previewing.
        int debugWidth = 10;
        int debugHeight = 10;
        float debugCellSize = 100f;
        Vector3 debugOrigin = this.transform.position;

        Gizmos.color = Color.cyan;
        for (int x = 0; x < debugWidth; x++)
        {
            for (int z = 0; z < debugHeight; z++)
            {
                // Draw a simple box representing each cell boundaries
                Vector3 center = debugOrigin + new Vector3(x * debugCellSize + debugCellSize / 2f, 0, z * debugCellSize + debugCellSize / 2f);
                Gizmos.DrawWireCube(center, new Vector3(debugCellSize, 0.1f, debugCellSize));
            }
        }
    } */
}
