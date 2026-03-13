using UnityEngine;

public class GridObject
{

    // Attributes.
    private GridSystem gridSystem;
    private GridPosition gridPosition;

    // Constructor.
    public GridObject(GridSystem gridSystem, GridPosition gridPosition) {
        this.gridSystem = gridSystem;
        this.gridPosition = gridPosition;
    }

    // Grid System Getter.
    public GridSystem getGridSystem() {
        return this.gridSystem;
    }

    // Grid Position Getter.
    public GridPosition getGridPosition() {
        return this.gridPosition;
    }

    public override string ToString() {
        return "grid:\n" + gridPosition.ToString() + "\nWorld:\n" + gridSystem.GetWorldPosition(gridPosition).ToString();
    }

}