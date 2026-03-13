using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystemVisual : MonoBehaviour
{

    [SerializeField] private Transform gridSystemVisualSinglePrefab;
    private GridSystemVisualSingle[,] gridSystemVisualSingleArray;
    private GridSystem gridSystem;

    private void Start()
    {
        //Initialize our array
        gridSystemVisualSingleArray = new GridSystemVisualSingle[gridSystem.GetWidth(), gridSystem.GetHeight()];

        //Loop through each cell in the array and instantiate a new "GridSystemVisualSingle" for each one.
        for(int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for(int z = 0; z < gridSystem.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Transform gridSystemVisualSingleTransform = 
                    Instantiate(gridSystemVisualSinglePrefab, gridSystem.GetWorldPosition(gridPosition), Quaternion.identity);

                //Set the parent of this new 'visual' object to this game object to keep our hierarchy clean.
                gridSystemVisualSingleTransform.parent = this.transform;

                //scale the visual by the grid's cell size
                gridSystemVisualSingleTransform.localScale *= gridSystem.GetCellSize();

                //Add the GridSystemVisualSingle to our array.
                GridSystemVisualSingle visualComponent = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();
                visualComponent.SetGridSystem(gridSystem);
                visualComponent.SetGridPosition(gridPosition);
                gridSystemVisualSingleArray[x,z] = visualComponent;
            }
        }
    }

    public void SetGridSystem(GridSystem gridSystem)
    {
        this.gridSystem = gridSystem;
    }
}
