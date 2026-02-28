using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{

    public Transform centerPoint;       // Object that we are orbitating.
    public float gravitationalMass;     // Mass of the object that we are orbitating (affects how quickly we orbitate the object).
                                        // the bigger the mass of the object (and therefore it's gravity) the quicker we have 
                                        // to rotate around it to stay in orbit.

    // The mass of the object that we are going to orbit will be set up purposefully to make the day/time cycle last a determinate amount of time.
    // For this we need to use Kepler's third law, which goes as:
    // Time = 2*pi * sqrt(radius^3 / (gravity_constant * mass))
    //
    // If we solve for mass variable, we get:
    // gravity_constant * mass = (4 * pi² * radius³) / time²
    //
    // Therefore, if we want our day and night to last 10 minutes each (20 mins total), the calculation goes as: 
    // (we can ignore gravity_constant since we are in a game, it will make calculations simpler).
    // mass = (4 * pi² * 1000³) / (60 * 20)² = 27 415.567
    //
    // Therefore, that is the mass that would need to be assigned to the orbital pivot (central element of the orbit).

    private Rigidbody rb;       // Us.

    void Start()
    {
        rb = GetComponent<Rigidbody>();     // Get our physics component.
        rb.useGravity = false;              // Turn off default gravity.
        InitializeOrbit();                  // Start the orbitig movement.
    }


    void FixedUpdate() // Used because it runs at the same rate as physics engine.
    {
        ApplyGravity(); // Apply gravity to the object.
    }

    // Function to give the object the initial force needed to stay in the orbit (before applying gravity towards center point).
    // Throughout the script, we will be using some of Newton's formulas; these include the gravity constant, which even if we 
    // mention it, it won't be used since it is not needed.
    void InitializeOrbit()
    {
        float distance = Vector3.Distance(centerPoint.position, transform.position); // Radius (distance to the center point) of the orbit.

        float speed = Mathf.Sqrt(gravitationalMass / distance); // Speed needed to stay in orbit (sqrt(gravity_force * mass) / radius)

        Vector3 directionToCenter = (centerPoint.position - transform.position).normalized;
        Vector3 sidewaysDir = Vector3.Cross(directionToCenter, Vector3.forward).normalized; // Calculate orbit direction (tangent).

        rb.linearVelocity = sidewaysDir * speed; // Apply calculated velocity and direction.
    }

    // Function to apply the gravitational force (towards the object that we are orbitating).
    void ApplyGravity()
    {
        // Get direction and distance to center point (direction of vector).
        Vector3 directionToCenter = (centerPoint.position - transform.position).normalized;
        float distance = Vector3.Distance(centerPoint.position, transform.position);

        // Force needed to apply gravity (Newton's formula -> F = (gravity_force * mass) / (radius²)) (magnitude of the vector.)
        float forceMagnitude = gravitationalMass / (distance * distance);

        rb.AddForce(directionToCenter * forceMagnitude); // Apply direction and magnitude of the force vector.
        transform.LookAt(centerPoint); // Make object looks towards the middle point (this ensures that the directional light works fine).
    }
}
