using UnityEngine;

public class TripleSine : MonoBehaviour
{
    // Attributes.
    [SerializeField] private Vector3 amplitude;
    [SerializeField] private Vector3 angle;
    [SerializeField] private Vector3 baseAngularVelocity; 
    [SerializeField] private Vector3 currentAngularVelocity;
    private Vector3 startPosition;
    private float noiseOffset;
    private float noiseTime;
    [SerializeField] private float noiseSpeed = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        amplitude = new Vector3(Random.Range(3f, 5.5f), Random.Range(3f, 4.5f), Random.Range(3f, 5.5f));
        angle = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
        baseAngularVelocity = new Vector3(Random.Range(-0.007f, 0.01f), Random.Range(-0.007f, 0.007f), Random.Range(-0.009f, 0.01f));
        startPosition = this.transform.position;
        noiseOffset = Random.Range(0f, 10000f); 
    }

    void FixedUpdate()
    {
        noiseTime += Time.fixedDeltaTime * noiseSpeed;
        float perlinValue = Mathf.PerlinNoise(noiseOffset + noiseTime, 0f);
        float speedMultiplier = Mathf.Lerp(0.2f, 2.0f, perlinValue);
        currentAngularVelocity = baseAngularVelocity * speedMultiplier;

        float offsetX = amplitude.x * Mathf.Sin(angle.x);
        float offsetY = amplitude.y * Mathf.Sin(angle.y);
        float offsetZ = amplitude.z * Mathf.Sin(angle.z);
        this.transform.position = startPosition + new Vector3(offsetX, offsetY, offsetZ);
        
        angle += currentAngularVelocity;
    }
}