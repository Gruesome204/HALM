using UnityEngine;
using UnityEngine.UIElements;

public class W_EquippedTurretsButtonBehavior
{
    public TurretBlueprint representedTurret;
    private WorkshopMenuBehavior workshop;

    public VisualElement turretBorder;
    public VisualElement cooldownCover;
    public Label turretNumbertxt;
    public Button turretButton;

    public W_EquippedTurretsButtonBehavior(VisualTreeAsset asset, TurretBlueprint _turret, WorkshopMenuBehavior _workshop)
    {
        TemplateContainer rowElement = asset.Instantiate();

        turretBorder = rowElement.Q<VisualElement>("border");
        cooldownCover = rowElement.Q<VisualElement>("cooldownCover");

        turretNumbertxt = rowElement.Q<Label>("turretNumber");
        turretNumbertxt.text = "";

        turretButton = rowElement.Q<Button>("button");
        turretButton.AddToClassList($"{_turret.turretName}Icon");
        turretButton.RegisterCallback<ClickEvent>(OnButtonClicked);


        representedTurret = _turret;
        workshop = _workshop;
    }

    void OnButtonClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayWoodClick();

        workshop.FillTurretDetails(representedTurret, true, true);
    }
}
