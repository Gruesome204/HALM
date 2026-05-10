using System.Collections.Generic;
using UnityEngine;

public class GridPathfinding : MonoBehaviour
{
    public static GridPathfinding Instance;

    private void Awake()
    {
        Instance = this;
        Debug.Log("GridPathfinding initialized");
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        HashSet<Vector2Int> closed = new();
        List<Vector2Int> open = new() { start };

        Dictionary<Vector2Int, Vector2Int> cameFrom = new();

        Dictionary<Vector2Int, float> gCost = new();
        gCost[start] = 0;

        while (open.Count > 0)
        {
            Vector2Int current = GetLowestFCost(open, gCost, goal);

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            open.Remove(current);
            closed.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (closed.Contains(neighbor))
                    continue;

                float tentativeG = gCost[current] + 1;

                if (!open.Contains(neighbor))
                    open.Add(neighbor);

                if (!gCost.ContainsKey(neighbor) || tentativeG < gCost[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gCost[neighbor] = tentativeG;
                }
            }
        }

        return new List<Vector2Int>();
    }

    List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new();

        Vector2Int[] dirs =
        {
            new(1,0), new(-1,0),
            new(0,1), new(0,-1)
        };

        foreach (var d in dirs)
        {
            Vector2Int n = pos + d;

            if (IsWalkable(n))
                neighbors.Add(n);
        }

        return neighbors;
    }

    bool IsWalkable(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0 ||
            pos.x >= GridManager.Instance.gridWidth ||
            pos.y >= GridManager.Instance.gridHeight)
            return false;

        return GridManager.Instance.CanPlaceObject(pos, Vector2Int.one);
    }

    List<Vector2Int> ReconstructPath(
        Dictionary<Vector2Int, Vector2Int> cameFrom,
        Vector2Int current)
    {
        List<Vector2Int> path = new() { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }

    Vector2Int GetLowestFCost(
        List<Vector2Int> open,
        Dictionary<Vector2Int, float> gCost,
        Vector2Int goal)
    {
        Vector2Int best = open[0];
        float bestF = float.MaxValue;

        foreach (var node in open)
        {
            float g = gCost[node];
            float h = Vector2Int.Distance(node, goal);
            float f = g + h;

            if (f < bestF)
            {
                bestF = f;
                best = node;
            }
        }

        return best;
    }
}