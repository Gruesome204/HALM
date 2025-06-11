using System.Collections.Generic;
using UnityEngine;

public class TurretPlacement : MonoBehaviour
{
    public List<TurretBlueprint> turretBlueprintList = new List<TurretBlueprint>();
    public LayerMask groundLayer; // Assign the layer of your ground in the Inspector
    public LayerMask turretLayer;
    public TurretBlueprint currentBlueprint;
    public GameObject placementPreviewPrefab; // Optional: Prefab for placement preview
    private GameObject currentPreview;

    [Header("Placement Cooldown")] // Organize variables in the Inspector
    public float placementCooldown = 1.0f; // Cooldown duration in seconds
    private float lastPlacementTime; // Time when the last turret was placed

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentBlueprint = turretBlueprintList[0];

        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentBlueprint = turretBlueprintList[1];

        }

        if (Input.GetMouseButtonDown(0) && currentBlueprint != null) // Left mouse button click
        {
            if (Time.time >= lastPlacementTime + placementCooldown)
            {
                Debug.Log("Test Click");
                Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, groundLayer);

                GameObject newTurret = Instantiate(currentBlueprint.turretPrefab, worldPosition, Quaternion.identity);
                TurretAttack turretAttack = newTurret.GetComponent<TurretAttack>();
                if (turretAttack != null)
                {
                    turretAttack.turretBlueprint = currentBlueprint;
                    turretAttack.InitializeFromBlueprint(); // Optional initialization method

                }
                // Update the last placement time
                lastPlacementTime = Time.time;
                Debug.Log("Turret placed! Cooldown started.");
            }
            else
            {
                Debug.Log("Turret placement on cooldown. Remaining: " + (lastPlacementTime + placementCooldown - Time.time).ToString("F2") + " seconds.");
            }
        }
    }
}