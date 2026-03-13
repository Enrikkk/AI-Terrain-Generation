using UnityEngine;

public class GridSystemVisualSingleFlowFieldArrow : GridSystemVisualSingle {

    // Separate reference to the Arrow child's MeshRenderer.
    // GetMeshRenderer() returns the border Quad's renderer — we need the Arrow child's renderer instead.
    [SerializeField] private MeshRenderer arrowMeshRenderer;

    // Fetched lazily in Update() to avoid a race condition.
    private GridObjectFlowFieldArrow gridObjectFlowFieldArrow;

    void Update() {
        // Lazy initialization: fetch once, as soon as the references are available.
        if (gridObjectFlowFieldArrow == null) {
            if (GetGridSystem() == null) return; // Not ready yet, wait another frame.
            gridObjectFlowFieldArrow = GetGridSystem().GetGridObject(GetGridPosition()) as GridObjectFlowFieldArrow;
        }

        if(gridObjectFlowFieldArrow != null) {
            float targetRotation = gridObjectFlowFieldArrow.GetArrowRotation();
            // Rotate the Arrow child object, not the border Quad.
            arrowMeshRenderer.transform.rotation = Quaternion.Euler(90, targetRotation, 0);
        }
    }

}