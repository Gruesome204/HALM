using UnityEngine;

public class PlacableObject : MonoBehaviour
{
    public Vector2Int currentGridCoordinates; // Stores the bottom-left grid coordinate when placed

    // Optional: Visual feedback for placement (e.g., green/red outline)
    // You'd handle this in a separate "Placement Preview" script that instantiates
    // a ghost object and checks with GridManager.CanPlaceObject
}