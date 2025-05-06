using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 20;
    public int height = 15;
    public float cellSize = 1f;
    private GameObject[,] gridData;

    public static GridManager Instance { get; private set; } // Singleton pattern

    private Vector2 gridOffset; // To store the offset for centering

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gridData = new GameObject[width, height];

            // Calculate the offset to center the grid at the origin (0, 0, 0)
            gridOffset = new Vector2(
                -((float)width * cellSize) / 2f + cellSize / 2f,
                -((float)height * cellSize) / 2f + cellSize / 2f
            );
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector2 WorldToGrid(Vector3 worldPosition)
    {
        Vector2 localPosition = worldPosition - (Vector3)gridOffset; // Cast to Vector3 for subtraction
        int gridX = Mathf.FloorToInt(localPosition.x / cellSize);
        int gridY = Mathf.FloorToInt(localPosition.y / cellSize); // Using Y for the grid's height in 2D
        return new Vector2(gridX, gridY);
    }

    public Vector3 GridToWorld(int gridX, int gridY)
    {
        float worldX = gridX * cellSize + cellSize / 2f + gridOffset.x;
        float worldY = gridY * cellSize + cellSize / 2f + gridOffset.y;
        return new Vector3(worldX, worldY, 0f); // Assuming Z=0 for the 2D plane
    }

    public bool IsWithinBounds(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < width && gridY >= 0 && gridY < height;
    }

    public GameObject GetCellContent(int gridX, int gridY)
    {
        if (IsWithinBounds(gridX, gridY))
        {
            return gridData[gridX, gridY];
        }
        return null;
    }

    public void SetCellContent(int gridX, int gridY, GameObject content)
    {
        if (IsWithinBounds(gridX, gridY))
        {
            gridData[gridX, gridY] = content;
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
                for (int y = 0; y < height; y++)
                {
                    Vector3 cellCenter = GridToWorld(x, y);
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, cellSize, 0.1f)); // Draw a square for 2D
                }
            }
        }
    }
}