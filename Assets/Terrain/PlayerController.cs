using UnityEngine;


public class PlayerController : MonoBehaviour
{
    // Attributes
    private CharacterController characterController;
    
    [Header("Movement Stats")]
    [SerializeField] private float walkAcceleration = 60f;
    [SerializeField] private float sprintAcceleration = 100f;
    [SerializeField] private float maxWalkSpeed = 5f;
    [SerializeField] private float maxSprintSpeed = 15f;
    [SerializeField] private float groundFriction = 10f;
    [SerializeField] private float airAcceleration = 15f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] [Range(0f, 1f)] private float jumpForwardMomentumMultiplier = 0.7f; // Shrinks horizontal speed exactly when jumping

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform cameraTransform;

    [Header("Fly Settings")]
    [SerializeField] private float flySpeed = 20f;
    [SerializeField] private float fastFlyMultiplier = 3f;
    private bool isFlying = false;

    [Header("Player Portal Interaction Settings")]
    public float initialPortalCooldownTime = 5f;
    public float actualPortalCooldownTime = 0f;
    
    // State Tracking
    private Vector3 currentVelocity = Vector3.zero;
    private float pitch = 0; // Hanldes camera rotation when looking up/down (when we look up or down our body doesn't rotate that way, only our head does).

    void Start()
    {
        this.characterController = GetComponent<CharacterController>();

        // Now lock the cursor at the center of the screen and hide it.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Toggle fly mode on 'F'
        if (Input.GetKeyDown(KeyCode.F))
        {
            this.isFlying = !this.isFlying;
            if (this.isFlying)
            {
                this.currentVelocity = Vector3.zero;
            }
        }

        if(this.actualPortalCooldownTime > 0f) { 
            this.actualPortalCooldownTime -= Time.deltaTime;
            this.actualPortalCooldownTime = Mathf.Max(0f, this.actualPortalCooldownTime);
        }
        this.HandleMovement();      // Function to move the player if requested.
        this.HandleView();          // Function to handle the rotation of the head and body based on the mouse movement.
    }

    // Function to handle the player's movement at each frame using force integration.
    private void HandleMovement()
    {
        // --- FLY MODE STATE ---
        if (this.isFlying)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            
            float upAxis = 0f;
            if (Input.GetKey(KeyCode.Space)) upAxis = 1f;
            if (Input.GetKey(KeyCode.LeftControl)) upAxis = -1f;

            // Use cameraTransform.forward to fly exactly where we are looking
            Vector3 flyDirection = (transform.right * h + this.cameraTransform.forward * v + transform.up * upAxis).normalized;

            float currentFlySpeed = this.flySpeed;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentFlySpeed *= this.fastFlyMultiplier;
            }

            this.currentVelocity = flyDirection * currentFlySpeed;
            this.characterController.Move(this.currentVelocity * Time.deltaTime);
            return; 
        }

        // 1. Get Input Direction (normalized)
        float horizontal = Input.GetAxisRaw("Horizontal"); 
        float vertical = Input.GetAxisRaw("Vertical"); 
        
        Vector3 inputDir = (transform.right * horizontal + transform.forward * vertical).normalized;

        bool isSprinting = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) && this.characterController.isGrounded);
        float currentMaxSpeed = isSprinting ? this.maxSprintSpeed : this.maxWalkSpeed;

        if (this.characterController.isGrounded)
        {
            // Reset gravity to stick to ground
            if (this.currentVelocity.y < 0f)
            {
                this.currentVelocity.y = -2f;
            }

            // Apply Ground Friction
            Vector3 horizontalVel = new Vector3(this.currentVelocity.x, 0f, this.currentVelocity.z);
            float speed = horizontalVel.magnitude;
            if (speed > 0)
            {
                float drop = speed * this.groundFriction * Time.deltaTime;
                float multiplier = Mathf.Max(speed - drop, 0f) / speed;
                this.currentVelocity.x *= multiplier;
                this.currentVelocity.z *= multiplier;
            }

            // Apply Input Acceleration
            if (inputDir.magnitude > 0)
            {
                float projectedSpeed = Vector3.Dot(new Vector3(this.currentVelocity.x, 0f, this.currentVelocity.z), inputDir);
                float addSpeed = Mathf.Clamp(currentMaxSpeed - projectedSpeed, 0, ((isSprinting && this.characterController.isGrounded) ? this.sprintAcceleration : this.walkAcceleration) * Time.deltaTime);
                this.currentVelocity += inputDir * addSpeed;
            }

            // Jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                this.currentVelocity.y = this.jumpForce;
                
                // Cut horizontal speed so we don't feel like we are launching horizontally
                this.currentVelocity.x *= this.jumpForwardMomentumMultiplier;
                this.currentVelocity.z *= this.jumpForwardMomentumMultiplier;
            }
        }
        else
        {
            // Air Control (Acceleration without Friction)
            if (inputDir.magnitude > 0)
            {
                float projectedSpeed = Vector3.Dot(new Vector3(this.currentVelocity.x, 0f, this.currentVelocity.z), inputDir);
                float addSpeed = Mathf.Clamp(currentMaxSpeed - projectedSpeed, 0, this.airAcceleration * Time.deltaTime);
                this.currentVelocity += inputDir * addSpeed;
            }

            // Apply Gravity
            this.currentVelocity.y += Physics.gravity.y * Time.deltaTime;
        }

        // Apply the final movement
        this.characterController.Move(this.currentVelocity * Time.deltaTime);
    }

    // Function to handle the rotation of the head and body based on the mouse movement.
    private void HandleView()
    {
        // First get how much we should rotate based on where the mouse is.
        float mouseX = Input.GetAxis("Mouse X") * this.mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * this.mouseSensitivity;

        // First horizontal rotation (Yaw).
        // We can stratightly rotate the whole body on the horizontal axis since when you look around you are usually 
        // rotating your whole body. This allows the paer to move towards where it is looking at.
        transform.Rotate(Vector3.up * mouseX);

        // Now vectirual rotation (Pitch).
        this.pitch -= mouseY; // We subtract for correct coordination.
        this.pitch = Mathf.Clamp(this.pitch, -90f, 90f); // Clamp pitch so that so can't continue looking upwards (or downwards) until we are looking backwards. At most totally up/down.

        // Finally, apply the pitch (vertical) rotation.
        this.cameraTransform.localEulerAngles = new Vector3(this.pitch, 0f, 0f);
    }

    // Function to make the player not teleport again through a portal for some time if it already did that.
    public void startPortalCoolDown() {
        this.actualPortalCooldownTime = this.initialPortalCooldownTime;
    }

}
