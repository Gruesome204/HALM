using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; } // Singleton pattern

    [Header("Grid Settings")]
    public float cellSize = 0.5f;
    public int gridWidth = 10;
    public int gridHeight = 10;
    public Vector3 originPosition = Vector3.zero; // World origin of the grid
    public float gridZPosition = 0f;


    [Header("Placement Settings")]
    public LayerMask groundLayer; // Only allow placement on this layer


    private GameObject[,] gridOccupancy;


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
    public Vector3 GetWorldPosition(Vector2Int gridCoords, Vector2Int objectSize)
    {
        // Center the object based on its size
        float x = originPosition.x + gridCoords.x * cellSize + (objectSize.x * cellSize) / 2f;
        float y = originPosition.y + gridCoords.y * cellSize + (objectSize.y * cellSize) / 2f;
        return new Vector3(x, y, gridZPosition);
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




        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                int gx = startCoords.x + x;
                int gy = startCoords.y + y;

                // 1️⃣ Check occupancy
                if (gridOccupancy[gx, gy] != null)
                    return false;
            }
        }

        return true;

    }
    public void PlaceObject(GameObject obj, Vector2Int startCoords, Vector2Int objectSize)
    {
        if (!CanPlaceObject(startCoords, objectSize))
        {
            Debug.LogWarning($"Cannot place object: {obj.name} at {startCoords}");
            return;
        }

        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                gridOccupancy[startCoords.x + x, startCoords.y + y] = obj;
            }
        }

        obj.transform.position = GetWorldPosition(startCoords, objectSize);
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
        if (gridWidth <= 0 || gridHeight <= 0 || cellSize <= 0) return;

        Gizmos.color = Color.grey;

        // Draw grid lines
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = originPosition + new Vector3(x * cellSize, 0, gridZPosition);
            Vector3 end = originPosition + new Vector3(x * cellSize, gridHeight * cellSize, gridZPosition);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = originPosition + new Vector3(0, y * cellSize, gridZPosition);
            Vector3 end = originPosition + new Vector3(gridWidth * cellSize, y * cellSize, gridZPosition);
            Gizmos.DrawLine(start, end);
        }

        if (!Application.isPlaying) return;

        // Draw objects without overlapping
        HashSet<GameObject> drawnObjects = new HashSet<GameObject>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject obj = gridOccupancy[x, y];
                if (obj == null || drawnObjects.Contains(obj)) continue;

                drawnObjects.Add(obj);

                Vector2Int size = Vector2Int.one;

                Vector3 center = GetWorldPosition(new Vector2Int(x, y), size);
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(center, new Vector3(size.x * cellSize, size.y * cellSize, 0.01f));
            }
        }
    }
}