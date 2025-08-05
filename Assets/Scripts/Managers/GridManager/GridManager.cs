using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; } // Singleton pattern

    [Header("Grid Settings")]
    public float cellSize = 1f;
    public int gridWidth = 10;
    public int gridHeight = 10;
    public Vector3 originPosition = Vector3.zero; // World origin of the grid
    public float gridZPosition = 0f;

    private GameObject[,] gridOccupancy;

    private Vector2 gridOffset; // To store the offset for centering

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        gridOccupancy = new GameObject[gridWidth, gridHeight];
    }

    // Converts world position (e.g., from mouse click) to grid coordinates
    public Vector2Int GetGridCoordinates(Vector3 worldPosition)
    {
        // For 2D (XY plane), we care about X and Y.
        // We subtract originPosition.x/y to get local coordinates relative to the grid's start.
        int x = Mathf.FloorToInt((worldPosition.x - originPosition.x) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.y - originPosition.y) / cellSize); // Changed from .z to .y

        return new Vector2Int(x, y);
    }

    // Converts grid coordinates to world position (center of the cell)
    public Vector3 GetWorldPosition(Vector2Int gridCoords)
    {
        // For 2D (XY plane), we position based on X and Y, and use the fixed gridZPosition for Z.
        float x = originPosition.x + gridCoords.x * cellSize + cellSize / 2f;
        float y = originPosition.y + gridCoords.y * cellSize + cellSize / 2f; // Changed from .z to .y for Z-axis in 3D to .y for Y-axis in 2D

        return new Vector3(x, y, gridZPosition); // Use fixed gridZPosition
    }

    // Checks if a placable object of a given size can be placed at a specific grid coordinate
    public bool CanPlaceObject(Vector2Int startCoords, Vector2Int objectSize)
    {
        // Check bounds
        if (startCoords.x < 0 || startCoords.y < 0 ||
            startCoords.x + objectSize.x > gridWidth ||
            startCoords.y + objectSize.y > gridHeight)
        {
            return false; // Out of bounds
        }

        // Check for existing objects in the required cells
        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                // Ensure array access is within bounds after calculating grid coordinates
                if (startCoords.x + x >= 0 && startCoords.x + x < gridWidth &&
                    startCoords.y + y >= 0 && startCoords.y + y < gridHeight)
                {
                    if (gridOccupancy[startCoords.x + x, startCoords.y + y] != null)
                    {
                        return false; // Cell is already occupied
                    }
                }
                else
                {
                    // This case should ideally be caught by the initial bounds check,
                    // but it's a good defensive programming check.
                    return false;
                }
            }
        }
        return true;
    }

    // Places an object onto the grid
    public void PlaceObject(GameObject obj, Vector2Int startCoords, Vector2Int objectSize)
    {
        if (!CanPlaceObject(startCoords, objectSize))
        {
            Debug.LogWarning("Cannot place object: " + obj.name + " at " + startCoords);
            return;
        }

        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                gridOccupancy[startCoords.x + x, startCoords.y + y] = obj;
            }
        }
        // Position object at the center of its bottom-left cell, at the fixed Z-position
        obj.transform.position = GetWorldPosition(startCoords);
    }

    // Removes an object from the grid
    public void RemoveObject(GameObject obj, Vector2Int startCoords, Vector2Int objectSize)
    {
        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                // Only clear if it's the specific object being removed and within bounds
                if (startCoords.x + x >= 0 && startCoords.x + x < gridWidth &&
                    startCoords.y + y >= 0 && startCoords.y + y < gridHeight &&
                    gridOccupancy[startCoords.x + x, startCoords.y + y] == obj)
                {
                    gridOccupancy[startCoords.x + x, startCoords.y + y] = null;
                }
            }
        }
    }

    // Show the grid at all times in the editor
    // For debugging: Draw grid lines in the editor
    private void OnDrawGizmos()
    {
        // Ensure grid settings are valid to prevent errors
        if (gridWidth <= 0 || gridHeight <= 0 || cellSize <= 0) return;

        // Gizmos are drawn in the editor and at runtime
        Gizmos.color = Color.grey; // Default color for grid lines

        // Draw basic grid lines
        for (int x = 0; x <= gridWidth; x++)
        {
            // Lines along the Y-axis (vertical lines)
            Vector3 start = originPosition + new Vector3(x * cellSize, 0, gridZPosition); // Start at Y=0, fixed Z
            Vector3 end = originPosition + new Vector3(x * cellSize, gridHeight * cellSize, gridZPosition); // End at max Y, fixed Z
            Gizmos.DrawLine(start, end);
        }
        for (int y = 0; y <= gridHeight; y++)
        {
            // Lines along the X-axis (horizontal lines)
            Vector3 start = originPosition + new Vector3(0, y * cellSize, gridZPosition); // Start at X=0, fixed Z
            Vector3 end = originPosition + new Vector3(gridWidth * cellSize, y * cellSize, gridZPosition); // End at max X, fixed Z
            Gizmos.DrawLine(start, end);
        }

        // Draw occupancy visualization when playing
        if (Application.isPlaying)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 cellCenter = GetWorldPosition(new Vector2Int(x, y));

                    // Draw a small cube/wire cube to represent the cell
                    if (gridOccupancy[x, y] != null)
                    {
                        Gizmos.color = Color.red; // Occupied cells are red
                        // Draw a cube representing the cell, slightly raised (0.01f) to be visible if on floor
                        Gizmos.DrawCube(cellCenter, new Vector3(cellSize, cellSize, 0.01f));
                    }
                    else
                    {
                        Gizmos.color = Color.blue; // Empty cells are blue
                        Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, cellSize, 0.01f));
                    }
                }
            }
        }
    }
}