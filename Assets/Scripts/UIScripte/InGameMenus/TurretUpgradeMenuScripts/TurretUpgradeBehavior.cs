using UnityEngine;
using UnityEngine.UIElements;

public class TurretUpgradeBehavior : MonoBehaviour
{
    private VisualElement turretUpgradeChoices;
    public VisualTreeAsset choiceListElement;


    private void OnEnable()
    {
        turretUpgradeChoices = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("turretUpgradeChoices");
        turretUpgradeChoices.Clear();
    }


    public void CreateListEntry(TurretUpgradeChoiceSO _choice)
    {
        TU_ChoiceElementBehavior tu_ChoiceElementBehavior = new TU_ChoiceElementBehavior(choiceListElement, _choice);
        turretUpgradeChoices.Add(tu_ChoiceElementBehavior.border);
    }
}
