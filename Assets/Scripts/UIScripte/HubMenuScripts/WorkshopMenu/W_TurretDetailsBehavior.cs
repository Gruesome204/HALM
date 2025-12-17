using System;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class W_TurretDetailsBehavior
{
    private TurretBlueprint representedTurret;

    public VisualElement mainContainer;
    public VisualElement statsContainer;
    public Label informationTxt;
    public Button buySelectButton;

    public Boolean turretUnlocked = false;
    public Boolean turretSelected = false;

    public W_TurretDetailsBehavior(VisualTreeAsset asset, TurretBlueprint _turret)
    {
        TemplateContainer detailsElement = asset.Instantiate();

        mainContainer = detailsElement.Q<VisualElement>("mainContainer");
        statsContainer = detailsElement.Q<VisualElement>("statsContainer");

        informationTxt = detailsElement.Q<Label>("informationTxt");
        informationTxt.text = "";

        representedTurret = _turret;

        buySelectButton = detailsElement.Q<Button>("buySelectButton");
        if (turretSelected)
        {
            //This Turrets is Selected and can only be removed from the List
            //buySelectButton.SetBinding("text", new LocalizedString("ActionRowTranslationTable", "TurretSelected"));
            buySelectButton.text = "selected";
            buySelectButton.RegisterCallback<ClickEvent>(TurretSelected);
        }
        else if (turretUnlocked)
        {
            //This Turret ist Unlocked, but not selected. If there is open space it can be added to the Selected Turrets
            //buySelectButton.SetBinding("text", new LocalizedString("ActionRowTranslationTable", "TurretUnlocked"));
            buySelectButton.text = "unlocked";
            buySelectButton.RegisterCallback<ClickEvent>(TurretUnlocked);
        }
        else
        {
            //This Turret isn't unlocked. It has to be bought
            //buySelectButton.SetBinding("text", new LocalizedString("ActionRowTranslationTable", "TurretLocked"));
            buySelectButton.text = "locked";
            buySelectButton.RegisterCallback<ClickEvent>(TurretLocked);
        }
    }

    void TurretSelected(ClickEvent evt)
    {

    }
    void TurretUnlocked(ClickEvent evt)
    {

    }
    void TurretLocked(ClickEvent evt)
    {

    }
}
