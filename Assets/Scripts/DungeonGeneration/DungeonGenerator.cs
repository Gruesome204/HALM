using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class DungeonGenerator : MonoBehaviour
{
    #region Inspector Variables

    [Header("References")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    [Header("Map Settings")]
    public int mapWidth = 100;
    public int mapHeight = 80;
    public int maxRooms = 10;
    public int minRoomSize = 5;
    public int maxRoomSize = 15;
    public int minRoomDistance = 2;
    public int seed = 0; // 0 = random

    #endregion

    // Internal map representation: 0 = empty, 1 = floor, 2 = wall
    private int[,] map;

    #region Unity Callbacks

    [ContextMenu("Generate Dungeon")]
    private void Start()
    {
        Generate();
    }

    public void GenerateContext()
    {
        Generate();
    }

    #endregion

    #region Dungeon Generation

    private void Generate()
    {
        System.Random rng = seed == 0 ? new System.Random() : new System.Random(seed);
        map = new int[mapWidth, mapHeight];

        // Clear previous tiles
        floorTilemap?.ClearAllTiles();
        wallTilemap?.ClearAllTiles();

        // Step 1: Generate linearly placed rooms
        List<RectInt> rooms = GenerateRoomsLinear(rng);

        // Step 2: Connect rooms strictly one-to-one
        ConnectRoomsLinearly(rooms);

        // Step 3: Surround all floors and corridors with walls
        SurroundFloorsWithWalls();

        // Step 4: Paint the tilemaps
        PaintTilemaps();

        Debug.Log($"Dungeon generated: {rooms.Count} rooms");
    }

    #endregion

    #region Room Generation

    private List<RectInt> GenerateRoomsLinear(System.Random rng)
    {
        List<RectInt> rooms = new List<RectInt>();

        int currentX = 1; // start on the left

        for (int i = 0; i < maxRooms; i++)
        {
            int w = rng.Next(minRoomSize, maxRoomSize + 1);
            int h = rng.Next(minRoomSize, maxRoomSize + 1);

            if (currentX + w >= mapWidth - 1) break; // stop if out of bounds

            // Randomize Y position so rooms are not all on the same line
            int yMin = 1;
            int yMax = mapHeight - h - 1;
            int currentY = rng.Next(yMin, yMax + 1);

            RectInt room = new RectInt(currentX, currentY, w, h);
            rooms.Add(room);
            CarveRoom(room);

            currentX += w + minRoomDistance; // move to next X position
        }

        return rooms;
    }


    private void CarveRoom(RectInt room)
    {
        for (int x = room.xMin; x < room.xMax; x++)
            for (int y = room.yMin; y < room.yMax; y++)
                map[x, y] = 1;
    }

    #endregion

    #region Corridor Generation

    private void ConnectRoomsLinearly(List<RectInt> rooms)
    {
        if (rooms.Count < 2) return;

        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int start = Vector2Int.RoundToInt(rooms[i - 1].center);
            Vector2Int end = Vector2Int.RoundToInt(rooms[i].center);
            CarveCorridor(start, end);
        }
    }

    private void CarveCorridor(Vector2Int a, Vector2Int b)
    {
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
            for (int dy = -1; dy <= 1; dy++) // 3 tiles wide
                if (InBounds(x, y + dy)) map[x, y + dy] = 1;
    }

    private void CarveVertical(int y1, int y2, int x)
    {
        int sy = Math.Min(y1, y2);
        int ey = Math.Max(y1, y2);

        for (int y = sy; y <= ey; y++)
            for (int dx = -1; dx <= 1; dx++) // 3 tiles wide
                if (InBounds(x + dx, y)) map[x + dx, y] = 1;
    }

    #endregion

    #region Wall Placement

    private void SurroundFloorsWithWalls()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y] == 1)
                {
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (InBounds(nx, ny) && map[nx, ny] == 0)
                                map[nx, ny] = 2;
                        }
                }
            }
        }
    }

    #endregion

    #region Tilemap Painting

    private void PaintTilemaps()
    {
        PaintFloor();
        PaintWalls();
    }

    private void PaintFloor()
    {
        if (floorTilemap == null || floorTile == null) return;

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x - mapWidth / 2, y - mapHeight / 2, 0);
                floorTilemap.SetTile(pos, map[x, y] == 1 ? floorTile : null);
            }
    }

    private void PaintWalls()
    {
        if (wallTilemap == null || wallTile == null) return;

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x - mapWidth / 2, y - mapHeight / 2, 0);
                if (map[x, y] == 2)
                    wallTilemap.SetTile(pos, wallTile);
            }
    }

    #endregion

    #region Utilities

    private bool InBounds(int x, int y) => x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;

    #endregion
}
