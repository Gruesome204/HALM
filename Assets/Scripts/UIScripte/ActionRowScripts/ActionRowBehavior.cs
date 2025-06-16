using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ActionRow : MonoBehaviour
{

    private VisualElement turretBTNContainer;

    private List<TurretBlueprint> turretsInGame = new List<TurretBlueprint>();
    private HashSet<TurretBlueprint> turretsCurrentlyPlaced = new HashSet<TurretBlueprint>();

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        turretsInGame = TurretPlacement.Instance.GetTurretBlueprintList();
        turretsCurrentlyPlaced = TurretPlacement.Instance.GetInstantiatedTurretList();

        foreach (var TurretBlueprint in turretsInGame)
        {
            var testing = new Button();
            turretBTNContainer.Add(testing);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
