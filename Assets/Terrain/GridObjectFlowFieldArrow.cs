using UnityEngine;

public class GridObjectFlowFieldArrow : GridObject{
    private float arrowRotation;

    public GridObjectFlowFieldArrow(GridSystem gridSystem, GridPosition gridPosition, float arrowRotation) : base(gridSystem, gridPosition){
        this.arrowRotation = arrowRotation;
    }

    public override string ToString(){
        string gridCoordinates = getGridPosition().ToString();
        Vector3 worldCoordinates = getGridSystem().GetWorldPosition(getGridPosition());
        return "arrow: \n" + arrowRotation;
    }

    public float GetArrowRotation(){
        return arrowRotation;
    }

    public void SetArrowRotation(float arrowRotation) {
        this.arrowRotation = arrowRotation;
    }
}
