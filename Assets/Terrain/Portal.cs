using UnityEngine;

public class Portal : MonoBehaviour
{

    [Header("Other Portal")]
    public GameObject otherPortal;

    [Header("Terrain")]
    public Terrain terrain;

    [Header("Portal Data")]
    public bool isCavePortal = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered the portal: " + other.name);

        if (other.name == "Player") {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController.actualPortalCooldownTime > 0f) {
                return; // Cooldown activated -> Do not teleport player.
            }
            else
            {
                playerController.startPortalCoolDown();
            }
        }

        if (otherPortal == null)
        {
            Debug.LogWarning("otherPortal is NULL!");
            return;
        }
        Debug.Log("Teleporting to: " + otherPortal.name);

        Vector3 destination;

        if (isCavePortal)
        {
            // Cave portal → going to surface: snap Y to terrain height.
            Vector3 basePos = otherPortal.transform.position;
            destination = new Vector3(basePos.x, terrain.SampleHeight(new Vector3(basePos.x, 0, basePos.z)), basePos.z);
        }
        else
        {
            // Surface portal → going to cave: raycast down onto the cave mesh to find the actual floor.
            Vector3 rayOrigin = otherPortal.transform.position + Vector3.right;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 50f))
                destination = hit.point + Vector3.up * 1f; // Just above the cave floor.
            else
                destination = otherPortal.transform.position; // Fallback if raycast misses.
        }

        // Disable CharacterController briefly so transform.position assignment isn't overridden.
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        other.transform.position = destination;
        if (cc != null) cc.enabled = true;
    }

    // Setters.
    public void setTerrain(Terrain terrain) { this.terrain = terrain; }
    public void setCavePortal(bool isCavePortal) { this.isCavePortal = isCavePortal; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
