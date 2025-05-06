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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                Vector3 placementPosition = hit.point;
                Vector3 gridPosition = GridManager.Instance.WorldToGrid(placementPosition);
                int gridX = Mathf.RoundToInt(gridPosition.x);
                int gridZ = Mathf.RoundToInt(gridPosition.z);

                if (GridManager.Instance.IsWithinBounds(gridX, gridZ) &&
                    GridManager.Instance.GetCellContent(gridX, gridZ) == null)
                {
                    Vector3 worldPosition = GridManager.Instance.GridToWorld(gridX, gridZ);
                    GameObject newTurret = Instantiate(currentBlueprint.turretPrefab, worldPosition, Quaternion.identity);

                    TurretAttack turretAttack = newTurret.GetComponent<TurretAttack>();
                    if (turretAttack != null)
                    {
                        turretAttack.turretBlueprint = currentBlueprint;
                        turretAttack.InitializeFromBlueprint(); // Optional initialization method
                    }
                    GridManager.Instance.SetCellContent(gridX, gridZ, newTurret);
                    //selectedTurretBlueprint = null; // Deselect after placing
                    Debug.Log("Turret placed at grid: " + new Vector3Int(gridX, 0, gridZ));
                }
                else
                {
                    Debug.Log("Cannot place turret here.");
                }
            }
        }
    }
}