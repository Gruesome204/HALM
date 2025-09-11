using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TurretUpgradeMenuBehavior : MonoBehaviour
{
    private VisualElement turretUpgradeChoices;
    public VisualTreeAsset choiceListElement;


    private void OnEnable()
    {
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

        TurretLevelManager.Instance.OnMilestoneReached += CreateListEntry;

        TurretLevelManager.Instance.OnMilestoneReached += (type, level) =>
        {
            Debug.Log($"UI Milestone triggered for {type} at level {level}");
        };
    }

    private void OnDisable()
    {
        TurretLevelManager.Instance.OnMilestoneReached -= CreateListEntry;
    }

    public void CreateListEntry(TurretType type, int level)
    {
        Debug.Log("Test");
        turretUpgradeChoices.Clear();
        foreach (var choice in TurretUpgradeChoiceManager.Instance.GetUpgradeChoices(type))
        {
            if (choice.triggerLevel == level)
            {

                Debug.Log($"Test UI");
                TU_ChoiceElementBehavior tu_ChoiceElementBehavior = new TU_ChoiceElementBehavior(choiceListElement, choice);
                turretUpgradeChoices.Add(tu_ChoiceElementBehavior.border);
                break;
            }
        }
    }
}
