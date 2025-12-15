using UnityEngine;
using UnityEngine.UIElements;

public class W_EquippedTurretsButtonBehavior
{
    public TurretBlueprint representedTurret;

    public VisualElement turretBorder;
    public VisualElement turretIcon;
    public VisualElement cooldownCover;
    public Label turretNumbertxt;
    public Button turretButton;

    public W_EquippedTurretsButtonBehavior(VisualTreeAsset asset, TurretBlueprint _turret)
    {
        TemplateContainer rowElement = asset.Instantiate();

        turretBorder = rowElement.Q<VisualElement>("border");
        cooldownCover = rowElement.Q<VisualElement>("cooldownCover");

        turretIcon = rowElement.Q<VisualElement>("icon");
        turretIcon.AddToClassList($"{_turret.turretName}Icon");

        turretNumbertxt = rowElement.Q<Label>("turretNumber");
        turretNumbertxt.text = "";

        turretButton = rowElement.Q<Button>("button");
        turretButton.RegisterCallback<ClickEvent>(OnButtonClicked);


        representedTurret = _turret;
    }

    void OnButtonClicked(ClickEvent evt)
    {

    }
}
