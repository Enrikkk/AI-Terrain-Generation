using UnityEngine;

public class FogScheduler : MonoBehaviour
{
    public Transform sunTransform;
    public Color dayColor;
    public Color nightColor;

    public float dayDensity;
    
    public float nightDensity;
    private float fogVal;
    private float fogInc;
    private float fogIncMult;

    public bool enableFogOnStart = true;

    void Start()
    {
        dayColor = new Color(0.5f, 0.6f, 0.7f);
        nightColor = Color.black;
        fogVal = Random.Range(0f, 10000f);
        fogInc = 0.1f;
        fogIncMult = Random.Range(0.2f, 0.6f);

        if(enableFogOnStart)
            RenderSettings.fog = true;
    }

    void Update()
    {
        // First get fog color based on time of the day.
        float sunHeight = Vector3.Dot(sunTransform.forward, Vector3.down);
        float timeBlend = Mathf.Clamp01((sunHeight + 1f) / 2f);
        RenderSettings.fogColor = Color.Lerp(nightColor, dayColor, timeBlend);

        // The fog intensity will be calculated using Perlin Noise.
        float perlinFog = Mathf.PerlinNoise(fogVal, 0);
        fogVal += fogInc * fogIncMult * Time.deltaTime;

        RenderSettings.fogDensity = Unity.Mathematics.math.remap(0f, 1f, 0.001f, 0.02f, perlinFog);
    }
}