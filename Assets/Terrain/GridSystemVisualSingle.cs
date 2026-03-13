using UnityEngine;

public class GridSystemVisualSingle : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    private GridSystem gridSystem;
    private GridPosition gridPosition;

    public void Show() {
        meshRenderer.enabled = true;
    }

    public void Hide() {
        meshRenderer.enabled = false;
    }

    public MeshRenderer GetMeshRenderer() {
        return meshRenderer;
    }

    public void SetGridSystem(GridSystem gridSystem) {
        Debug.Log("SETTING GRID SYSTEM TO " + gridSystem);
        this.gridSystem = gridSystem;
    }

    public GridSystem GetGridSystem() {
        return gridSystem;
    }

    public void SetGridPosition(GridPosition gridPosition) {
        this.gridPosition = gridPosition;
    }

    public GridPosition GetGridPosition() {
        return this.gridPosition;
    }
}
