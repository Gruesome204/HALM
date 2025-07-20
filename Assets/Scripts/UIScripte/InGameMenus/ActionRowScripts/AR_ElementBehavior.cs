using UnityEngine;
using UnityEngine.UIElements;

public class AR_ElementBehavior
{
    public TurretBlueprint representedTurret;

    public VisualElement turretBorder;
    public VisualElement turretIcon;
    public Button turretButton;
    public int placementTurretNumber;

    public AR_ElementBehavior(VisualTreeAsset asset, TurretBlueprint _turret)
    {
        TemplateContainer rowElement = asset.Instantiate();

        turretBorder = rowElement.Q<VisualElement>("border");

        turretIcon = rowElement.Q<VisualElement>("icon");

        turretButton = rowElement.Q<Button>("button");
        turretButton.RegisterCallback<ClickEvent>(OnTurretButtonClicked);

        representedTurret = _turret;
    }

    public void ChangeColor()
    {
        if (TurretPlacementController.Instance.currentSelectedBlueprint == representedTurret)
        {
            turretButton.style.backgroundColor = Color.red;
        }
        else
        {
            turretButton.style.backgroundColor = Color.white;
        }
    }

    void OnTurretButtonClicked(ClickEvent evt)
    {
        TurretPlacementController.Instance.currentSelectedBlueprint = representedTurret;
    }
}
