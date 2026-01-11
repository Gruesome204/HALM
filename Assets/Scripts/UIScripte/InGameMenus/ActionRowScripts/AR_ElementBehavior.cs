using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AR_ElementBehavior
{
    public TurretBlueprint representedTurret;
    public Boolean turretOnCooldown = false;
    private int turretNumber;

    public VisualElement turretBorder;
    public VisualElement cooldownCover;
    public Label turretNumbertxt;
    public Button turretButton;


    public AR_ElementBehavior(VisualTreeAsset asset, TurretBlueprint _turret, int _turretNumber)
    {
        TemplateContainer rowElement = asset.Instantiate();

        turretBorder = rowElement.Q<VisualElement>("border");
        cooldownCover = rowElement.Q<VisualElement>("cooldownCover");

        turretNumber = _turretNumber;
        turretNumbertxt = rowElement.Q<Label>("turretNumber");
        turretNumbertxt.text = $"{_turretNumber}";

        turretButton = rowElement.Q<Button>("button");
        turretButton.AddToClassList($"{_turret.turretName}Icon");
        turretButton.RegisterCallback<ClickEvent>(OnTurretButtonClicked);


        representedTurret = _turret;

        TurretPlacementController.Instance.OnPlacementCooldownStateChanged += HandleCooldown;
    }
    void OnTurretButtonClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("wood");

        TurretPlacementController.Instance.SelectTurretBlueprint(representedTurret);
    }

    public void ChangeColor()
    {
        if (TurretPlacementController.Instance.currentSelectedBlueprint == representedTurret)
        {
            turretButton.style.unityBackgroundImageTintColor = Color.red;
        }
        else
        {
            turretButton.style.unityBackgroundImageTintColor = new Color(202, 201, 201);
        }
    }

    void HandleCooldown(TurretBlueprint _turret, Boolean _active)
    {
        if (_turret == representedTurret)
        {
            turretOnCooldown = _active;
            if (_active)
            {

            }
            else
            {
                cooldownCover.style.height = new Length(0, LengthUnit.Percent);
                turretNumbertxt.text = $"{turretNumber}";
            }
        }
    }

    public void HandleVisualCooldown()
    {
        if (turretOnCooldown)
        {
            float remainingCooldownPercentage = (TurretPlacementController.Instance.GetCooldownRemaining(representedTurret)/ representedTurret.placementCooldown) * 100;
            cooldownCover.style.height = new Length(remainingCooldownPercentage, LengthUnit.Percent);

            turretNumbertxt.text = $"{TurretPlacementController.Instance.GetCooldownRemaining(representedTurret).ToString("F1")}";
        }
    }
}
