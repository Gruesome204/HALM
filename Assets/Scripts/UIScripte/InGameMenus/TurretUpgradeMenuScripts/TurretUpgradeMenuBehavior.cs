using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TurretUpgradeMenuBehavior : MonoBehaviour, IMenu
{
    private VisualElement turretUpgradeChoices;
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

        turretUpgradeChoices = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("turretUpgradeChoices");

        if (turretUpgradeChoices == null)
        {
            Debug.LogError("turretUpgradeChoices element not found in UXML!");
            return;
        }
        turretUpgradeChoices.Clear();

        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("No UIDocument found!");
            return;
        }
    }


    public void CreateListEntry(TurretType type, int level)
    {
        Debug.Log("Test");
        turretUpgradeChoices.Clear();

        var options = TurretUpgradeChoiceManager.Instance.GetAllOptionsForLevel(type, level);

        foreach (var option in options)
        {
            TU_ChoiceElementBehavior tu_ChoiceElementBehavior =
                new TU_ChoiceElementBehavior(choiceListElement, option, type, level);

            turretUpgradeChoices.Add(tu_ChoiceElementBehavior.border);
        }

    }
   }
