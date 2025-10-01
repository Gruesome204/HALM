using System;
using UnityEditor;
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
    }

    private void Start()
    {
        if (TurretLevelManager.Instance != null)
        {
            TurretLevelManager.Instance.OnMilestoneReached += CreateListEntry;
        }
    }

    private void OnDisable()
    {
        TurretLevelManager.Instance.OnMilestoneReached -= CreateListEntry;
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
