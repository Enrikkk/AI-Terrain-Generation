using UnityEngine;

public class Vehicle : MonoBehaviour
{

    // Attributes.
    [SerializeField] Transform vechiblePrefab;

    // Movement attributes.
    [SerializeField] private Vector3 velocity;
    [SerializeField] private Vector3 acceleration;
    [SerializeField] private float mass;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxForce;

    // Flow field specific data.
    [SerializeField] private FlowField flowField;
    [SerializeField] private int flowFieldGridWidth;
    [SerializeField] private int flowFieldGridHeight;
    [SerializeField] private int flowFieldCellSize;
    [SerializeField] private int flowFieldCellWidth;
    [SerializeField] private int flowFieldCellHeight;

    void Start()
    {
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        
        maxSpeed = Random.Range(0.1f, 0.3f);
        mass = 150f;
        maxForce = 0.05f;

        // Make bird animation to happen.
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (Animator anim in animators) {
            anim.enabled = true;
            anim.speed = Random.Range(0.8f, 1.5f);
            
            // Explicitly force the bird into its default state on the base layer (Layer 0) 
            // We use 0 instead of "Fly" because the different bird clusters (05Birds, 10Birds) 
            // name their animations "05BirdsFly" and "10BirdsFly" respectively!
            // We also pass Random.value as the 3rd argument to desynchronize the wing flaps!
            anim.Play(0, -1, Random.value);
        }
    }

    // Auxiliar function so that we can give the vehicle a flow field. Only given if it is supposed to.
    public void assignFlowField(FlowField flowField) {
        this.flowField = flowField;
        this.flowFieldGridWidth = this.flowField.gridSystem.GetWidth();
        this.flowFieldGridHeight = this.flowField.gridSystem.GetHeight();
        this.flowFieldCellSize = Mathf.RoundToInt(this.flowField.gridSystem.GetCellSize());
        this.flowFieldCellWidth = this.flowFieldGridWidth * this.flowFieldCellSize;
        this.flowFieldCellHeight = this.flowFieldGridHeight * this.flowFieldCellSize;
    }

    void Update()
    {
        if( this.flowField != null) // If it is a flow field vehicle.
        {
            this.FollowFlowField();
        }
        // Apply velocity.
        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        this.transform.position += velocity;

        // Make the vehicle look where it is moving to.
        if (velocity != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
        }
        acceleration = Vector3.zero;
    }

    // Function to receive flow field forces if actually inside one.
    private void FollowFlowField() {
        // If we are out of the grid, get back inside.
        // Do it like if it was the pacman map. If you exit right, enter left and vice versa.
        if(!this.flowField.gridSystem.IsValidGridPosition(this.transform.position)) {
            Vector3 worldPos = this.transform.position;
            Vector3 origin = this.flowField.gridSystem.GetOrigin();
            
            float minX = origin.x;
            float maxX = origin.x + this.flowFieldCellWidth;
            float minZ = origin.z;
            float maxZ = origin.z + this.flowFieldCellHeight;

            // Wrap X axis
            if (worldPos.x < minX) worldPos.x = maxX - 0.1f;
            else if (worldPos.x >= maxX) worldPos.x = minX + 0.1f;

            // Wrap Z axis
            if (worldPos.z < minZ) worldPos.z = maxZ - 0.1f;
            else if (worldPos.z >= maxZ) worldPos.z = minZ + 0.1f;

            this.transform.position = worldPos;
        }

        // Now, apply the correspondent force to the vehicle.
        // First get arrow rotation.
        GridPosition gridPosition = this.flowField.gridSystem.GetGridPosition(this.transform.position);
        GridObjectFlowFieldArrow flowFieldArrow = this.flowField.gridSystem.GetGridObject(gridPosition) as GridObjectFlowFieldArrow;
        float arrowRotation = flowFieldArrow.GetArrowRotation();
        
        // Use Unity's exact rotation system to get the True 3D direction vector
        Vector3 direction = (Quaternion.Euler(0, arrowRotation, 0) * Vector3.right).normalized;
        
        Vector3 desiredVelocity = direction * this.maxSpeed;
        Vector3 steeringForce = desiredVelocity - this.velocity;
        steeringForce = Vector3.ClampMagnitude(steeringForce, this.maxForce);

        this.ApplyForce(steeringForce);
    }

    private void ApplyForce(Vector3 force) {
        this.acceleration += force / this.mass;
    }
}
