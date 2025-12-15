using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class WorkshopMenuBehavior : MonoBehaviour, IMenu
{
    private Label headline;
    private VisualElement availableTurrets;
    private VisualElement equippedTurrets;
    private VisualElement towerDetailsContainer;

    public VisualTreeAsset turretDetails;
    public VisualTreeAsset turretButtons;


    public List<TurretBlueprint> allTurretsList = new List<TurretBlueprint>();
    public List<TurretBlueprint> equippedTurretsList = new List<TurretBlueprint>();

    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the SettingsMenu and adds it to openMenu List
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);
            Fill();
        }
        else
        {
            //Closes SettingsMenu and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
            Clear();
        }
    }

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("BuildmasterModifyTranslationTable", "headline"));

        availableTurrets = root.Q<VisualElement>("availableTurrets");
        equippedTurrets = root.Q<VisualElement>("equippedTurrets");
        towerDetailsContainer = root.Q<VisualElement>("towerDetailsContainer");

    }

    private void Fill()
    {
        foreach (var turret in allTurretsList)
        {
            //Create one Button in available Turrets,
            W_AvailableTurretsButtonBejavior availableTurret = new W_AvailableTurretsButtonBejavior(turretButtons, turret);
            availableTurrets.Add(availableTurret.turretBorder);
            Debug.Log("Filling");
        }

        foreach (var turret in equippedTurretsList)
        {
            //Create one Button in equipped Turrets
        }

    }
    private void Clear()
    {
        availableTurrets.Clear();
        equippedTurrets.Clear();
        ClearTurretDetails();
        Debug.Log("Clearing");

    }
    private void ClearTurretDetails()
    {
        towerDetailsContainer.Clear();
    }
}
