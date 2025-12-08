using UnityEngine;
using UnityEngine.UIElements;

public class AR_ResourceBehavior
{
    public VisualElement border;
    public VisualElement resourceIcon;
    public Label resourceAmount;


    public AR_ResourceBehavior(VisualTreeAsset asset, int itemAmount, string _icon)
    {
        TemplateContainer resourceElemente = asset.Instantiate();

        border = resourceElemente.Q<VisualElement>("border");

        resourceIcon = resourceElemente.Q<VisualElement>("resourceIcon");
        resourceIcon.AddToClassList($"{_icon}");

        resourceAmount = resourceElemente.Q<Label>("resourceAmount");
        resourceAmount.text = $"{itemAmount}";
    }
}
