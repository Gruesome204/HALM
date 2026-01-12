using UnityEngine;
using UnityEngine.UIElements;

public class AR_TowerLimitElementBehavior
{
    public VisualElement border;
    public int assignedNumber;

    public AR_TowerLimitElementBehavior(VisualTreeAsset asset, int _assignedNumber)
    {
        TemplateContainer resourceElemente = asset.Instantiate();
        assignedNumber = _assignedNumber;

        border = resourceElemente.Q<VisualElement>("border");
    }

    public void UpdateColor(int towersPlaced)
    {
        if (assignedNumber <= towersPlaced)
        {
            border.AddToClassList("towerLimitFilled");
            border.RemoveFromClassList("towerLimitEmpty");
        }
        else
        {
            border.AddToClassList("towerLimitEmpty");
            border.RemoveFromClassList("towerLimitFilled");
        }
    }
}
