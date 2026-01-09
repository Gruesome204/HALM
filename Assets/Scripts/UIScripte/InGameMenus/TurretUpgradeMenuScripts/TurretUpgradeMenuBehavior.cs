using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class TurretUpgradeMenuBehavior : MonoBehaviour, IMenu
{

    private Label headline;
    private VisualElement icon;
    private Label towerName;
    private ScrollView turretUpgradeChoices;

    public VisualTreeAsset choiceListElement;

    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the TurretUpgrade Menu, adds to openMenu List and sets Game to paused
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);
            GameManager.Instance.ChangeState(GameManager.GameState.Paused);
        }
        else
        {
            //Closes TurretUpgrade and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
        }
    }
    private void OnEnable()
    {
        TurretLevelManager.Instance.OnMilestoneReached += CreateListEntry;
        var root = GetComponent<UIDocument>().rootVisualElement;

        headline = root.Q<Label>("headline");
        icon = root.Q<VisualElement>("icon");
        towerName = root.Q<Label>("towerName");

        turretUpgradeChoices = root.Q<ScrollView>("turretUpgradeChoices");
    }

    void Fill(TurretType _type)
    {
        headline.SetBinding("text", new LocalizedString("TurretUpgradeTranslationTable", "headline"));

        towerName.SetBinding("text", new LocalizedString($"TurretTranslation{_type}","name"));

        icon.ClearClassList();
        icon.AddToClassList($"{_type}Icon");
    }

    public void CreateListEntry(TurretType type, int level)
    {
        InGameMenuManager.Instance.OpenOrCloseOneMenu("TurretUpgradeChoiceDoc", true);
        Fill(type);

        turretUpgradeChoices.Clear();

        var options = TurretUpgradeChoiceManager.Instance.GetAllOptionsForLevel(type, level);

        foreach (var option in options)
        {
            
            TU_ChoiceElementBehavior tu_ChoiceElementBehavior = new TU_ChoiceElementBehavior(choiceListElement, option);

            turretUpgradeChoices.Add(tu_ChoiceElementBehavior.border);
        }
    }
   }
