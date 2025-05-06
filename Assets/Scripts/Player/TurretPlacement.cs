using System.Collections.Generic;
using UnityEngine;

public class TurretPlacement : MonoBehaviour
{
    public List<TurretBlueprint> TurretBlueprintList = new List<TurretBlueprint>();
    public LayerMask groundLayer; // Assign the layer of your ground in the Inspector

    public TurretBlueprint currentBlueprint;
    public GameObject placementPreviewPrefab; // Optional: Prefab for placement preview
    private GameObject currentPreview;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && currentBlueprint != null) // Left mouse button click
        {
            Debug.Log("Test Click");
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, groundLayer);
            Vector2 placementPosition = hit.point;

            GameObject newTurret = Instantiate(currentBlueprint.turretPrefab, worldPosition, Quaternion.identity);

                TurretAttack turretAttack = newTurret.GetComponent<TurretAttack>();
                if (turretAttack != null)
                {
                    turretAttack.turretBlueprint = currentBlueprint;
                    turretAttack.InitializeFromBlueprint(); // Optional initialization method
                    
                }
        }
    }
}