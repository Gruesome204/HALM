using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 20;
    public int height = 15;
    public float cellSize = 1f;
    private GameObject[,] gridData;

    public static GridManager Instance { get; private set; } // Singleton pattern

    private Vector3 gridOffset; // To store the offset for centering

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gridData = new GameObject[width, height];

            // Calculate the offset to center the grid at the origin (0, 0, 0)
            gridOffset = new Vector3(
                -((float)width * cellSize) / 2f + cellSize / 2f,
                0f,
                -((float)height * cellSize) / 2f + cellSize / 2f
            );
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector3 WorldToGrid(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - gridOffset;
        int gridX = Mathf.FloorToInt(localPosition.x / cellSize);
        int gridZ = Mathf.FloorToInt(localPosition.z / cellSize); // Using Z for the grid's depth
        return new Vector3(gridX, 0f, gridZ);
    }

    public Vector3 GridToWorld(int gridX, int gridZ)
    {
        float worldX = gridX * cellSize + cellSize / 2f + gridOffset.x;
        float worldZ = gridZ * cellSize + cellSize / 2f + gridOffset.z;
        return new Vector3(worldX, 0f, worldZ); // Assuming Y=0 for the ground plane
    }

    public bool IsWithinBounds(int gridX, int gridZ)
    {
        return gridX >= 0 && gridX < width && gridZ >= 0 && gridZ < height;
    }

    public GameObject GetCellContent(int gridX, int gridZ)
    {
        if (IsWithinBounds(gridX, gridZ))
        {
            return gridData[gridX, gridZ];
        }
        return null;
    }

    public void SetCellContent(int gridX, int gridZ, GameObject content)
    {
        if (IsWithinBounds(gridX, gridZ))
        {
            gridData[gridX, gridZ] = content;
        }
    }

    // Show the grid at all times in the editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (Instance == this) // Only draw if this is the active instance
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    Vector3 cellCenter = GridToWorld(x, z);
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, 0.1f, cellSize));
                }
            }
        }
    }
}