using UnityEngine;

public class GridSystem
{

    // Atributes.
    private int width;
    private int height;
    private float cellSize;
    private Vector3 origin;

    // Represent the objects in the grid system with a 2D array, but the array will be able to hold any type of grid.
    private GridObject[,] gridObjectArray;

    private GridSystemVisual gridSystemVisual;

    // Done to instantiate specific type of GridObject.
    public delegate GridObject InstantiateGridObjectDelegate(GridSystem gridSystem, GridPosition gridPosition);
    
    // Constructor.
    public GridSystem(int width, int height, float cellSize, Vector3 origin, GridSystemVisual gridSystemVisual, InstantiateGridObjectDelegate instantiateAppropriateGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;
        this.gridObjectArray = new GridObject[width, height];
        this.gridSystemVisual = gridSystemVisual;
    
        for(int x = 0; x < this.width; ++x)
        {
            for(int z = 0; z < this.height; ++z)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                this.gridObjectArray[x, z] = instantiateAppropriateGridObject(this, gridPosition);
            }
        }
    }

    //Draw some debug lines to visualize the grid
    public void drawDebugLines()
    {
        for (int x = 0; x < this.width; x++)
        {
            for (int z = 0; z < this.height; z++)
            {
                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z) + Vector3.right * 0.2f, Color.black, 0.1f);
            }
        }
    }

    //Get the Unity world coordinates of the center of this GridPosition
    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x, 0, gridPosition.z) * cellSize + origin + new Vector3(cellSize, 0, cellSize) * 0.5f;
    }

    //Get the Unity world coordinates of the cell at this x,z index
    public Vector3 GetWorldPosition(int x, int z)
    {
        return GetWorldPosition(GetGridPosition(x, z));
    }

    //Return the GridPosition object associated with this x,z index
    public GridPosition GetGridPosition(int x, int z)
    {
        return gridObjectArray[x,z].getGridPosition();
    }

    // Get the specific GridObject at the given GridPosition
    public GridObject GetGridObject(GridPosition gridPosition)
    {
        return gridObjectArray[gridPosition.x, gridPosition.z];
    }

    public GridObject GetGridObject(int x, int z)
    {
        return this.GetGridObject(new GridPosition(x, z));
    }

    // Create the debug objects that show coordinates
    public void CreateDebugObjects(Transform debugPrefab, Transform parent = null)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                
                // Rotate 180 on the X-axis so the text faces downwards instead of upwards
                Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.Euler(180, 0, 0), parent);
                debugTransform.localScale = Vector3.one * (this.cellSize*0.6f);
                GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>();
                gridDebugObject.SetGridObject(GetGridObject(gridPosition));
            }
        }
    }

    // Return which square grid you are at inside based on your location in the world.
    public GridPosition GetGridPosition(Vector3 worldPosition) {
        Vector3 worldPositionWithOriginOffset = worldPosition - origin;
        return new GridPosition(Mathf.RoundToInt(Mathf.Floor(worldPositionWithOriginOffset.x / cellSize)), Mathf.RoundToInt(Mathf.Floor(worldPositionWithOriginOffset.z / cellSize)));
    }

    // Check if the given world position resides within the grid boundaries mathematically.
    public bool IsValidGridPosition(Vector3 worldPosition) {
        float minX = origin.x;
        float minZ = origin.z;
        float maxX = origin.x + (width * cellSize);
        float maxZ = origin.z + (height * cellSize);
        
        return worldPosition.x >= minX && worldPosition.x < maxX &&
               worldPosition.z >= minZ && worldPosition.z < maxZ;
    }

    // Check if the given GridPosition is within the grid boundaries.
    public bool IsValidGridPosition(GridPosition gridPosition) {
        return this.IsValidGridPosition(new Vector3(gridPosition.x, 0, gridPosition.z));
    }
    // Origin setter.
    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    // Getters.
    public int GetWidth() { return this.width; }
    public int GetHeight() { return this.height; }
    public float GetCellSize() { return this.cellSize; }
    public Vector3 GetOrigin() { return this.origin; }

}