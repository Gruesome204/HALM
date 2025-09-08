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
        border.style.backgroundColor = Color.red;
    }

    public void UpdateColor(int towersPlaced)
    {
        if (assignedNumber <= towersPlaced)
        {
            border.style.backgroundColor = Color.red;
            Debug.Log($"{assignedNumber} should be red");
        }
        else
        {
            Debug.Log($"{assignedNumber} should be white");
            border.style.backgroundColor = Color.white;
        }
    }
}
