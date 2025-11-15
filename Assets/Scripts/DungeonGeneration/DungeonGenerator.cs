using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class DungeonGenerator : MonoBehaviour
{
    [Header("References")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    [Header("Map Settings")]
    public int mapWidth = 100;
    public int mapHeight = 80;
    public int maxRooms = 20;
    public int minRoomSize = 5;
    public int maxRoomSize = 15;
    public int maxPlacementAttempts = 200;
    public int seed = 0; // 0 = random

    [Header("Walls")]
    public bool generateWalls = true;

    // Internal map: 0 = empty, 1 = floor
    private int[,] map;

    [ContextMenu("Generate Dungeon")]

    private void Start()
    {
        Generate();
    }
    public void GenerateContext()
    {
        Generate();
    }

    public void Generate()
    {
        Debug.Log("Start Generating");
        System.Random rng = seed == 0 ? new System.Random() : new System.Random(seed);
        map = new int[mapWidth, mapHeight];
        List<RectInt> rooms = new List<RectInt>();

        // Clear tilemaps
        if (floorTilemap) floorTilemap.ClearAllTiles();
        if (wallTilemap) wallTilemap.ClearAllTiles();

        int attempts = 0;
        while (rooms.Count < maxRooms && attempts < maxPlacementAttempts)
        {
            attempts++;
            int w = rng.Next(minRoomSize, maxRoomSize + 1);
            int h = rng.Next(minRoomSize, maxRoomSize + 1);
            int x = rng.Next(1, mapWidth - w - 1);
            int y = rng.Next(1, mapHeight - h - 1);
            RectInt newRoom = new RectInt(x, y, w, h);

            bool overlaps = false;
            foreach (var r in rooms)
            {
                // Expand r by 1 to create a separation buffer
                RectInt expanded = new RectInt(r.xMin - 1, r.yMin - 1, r.width + 2, r.height + 2);
                if (expanded.Overlaps(newRoom))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                rooms.Add(newRoom);
                CarveRoom(newRoom);
            }
        }

        // Connect rooms - simple MST-like by connecting room centers in order
        if (rooms.Count > 1)
        {
            // Sort by x to make connections more stable, or shuffle for variety
            rooms.Sort((a, b) => a.center.x.CompareTo(b.center.x));
            for (int i = 1; i < rooms.Count; i++)
            {
                Vector2Int prev = Vector2Int.RoundToInt(rooms[i - 1].center);
                Vector2Int cur = Vector2Int.RoundToInt(rooms[i].center);
                CarveCorridor(prev, cur);
            }
        }

        // Paint tiles
        PaintTiles();

        // Paint walls (simple: any empty tile adjacent to floor becomes wall)
        if (generateWalls && wallTilemap != null && wallTile != null)
        {
            PaintWalls();
        }

        Debug.Log($"Dungeon generated: rooms={rooms.Count}, attempts={attempts}");
    }

    private void CarveRoom(RectInt r)
    {
        for (int x = r.xMin; x < r.xMax; x++)
            for (int y = r.yMin; y < r.yMax; y++)
                map[x, y] = 1;
    }

    private void CarveCorridor(Vector2Int a, Vector2Int b)
    {
        // L-shaped corridor: horizontal then vertical (randomize order sometimes)
        bool horizontalFirst = UnityEngine.Random.value > 0.5f;
        if (horizontalFirst)
        {
            CarveHorizontal(a.x, b.x, a.y);
            CarveVertical(a.y, b.y, b.x);
        }
        else
        {
            CarveVertical(a.y, b.y, a.x);
            CarveHorizontal(a.x, b.x, b.y);
        }
    }

    private void CarveHorizontal(int x1, int x2, int y)
    {
        int sx = Math.Min(x1, x2);
        int ex = Math.Max(x1, x2);
        for (int x = sx; x <= ex; x++)
        {
            if (InBounds(x, y)) map[x, y] = 1;
            // widen corridor optionally:
            if (InBounds(x, y + 1)) map[x, y + 1] = 1;
            if (InBounds(x, y - 1)) map[x, y - 1] = 1;
        }
    }

    private void CarveVertical(int y1, int y2, int x)
    {
        int sy = Math.Min(y1, y2);
        int ey = Math.Max(y1, y2);
        for (int y = sy; y <= ey; y++)
        {
            if (InBounds(x, y)) map[x, y] = 1;
            if (InBounds(x + 1, y)) map[x + 1, y] = 1;
            if (InBounds(x - 1, y)) map[x - 1, y] = 1;
        }
    }

    private bool InBounds(int x, int y) => x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;

    private void PaintTiles()
    {
        if (floorTilemap == null || floorTile == null) return;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x - mapWidth / 2, y - mapHeight / 2, 0); // center the dungeon
                if (map[x, y] == 1)
                    floorTilemap.SetTile(pos, floorTile);
                else
                    floorTilemap.SetTile(pos, null);
            }
        }
    }

    private void PaintWalls()
    {
        if (wallTilemap == null || wallTile == null) return;
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y] == 0)
                {
                    bool adjacentToFloor = false;
                    for (int dx = -1; dx <= 1 && !adjacentToFloor; dx++)
                        for (int dy = -1; dy <= 1 && !adjacentToFloor; dy++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
                                if (map[nx, ny] == 1) adjacentToFloor = true;
                        }

                    if (adjacentToFloor)
                    {
                        Vector3Int pos = new Vector3Int(x - mapWidth / 2, y - mapHeight / 2, 0);
                        wallTilemap.SetTile(pos, wallTile);
                    }
                }
            }
        }
    }
}
