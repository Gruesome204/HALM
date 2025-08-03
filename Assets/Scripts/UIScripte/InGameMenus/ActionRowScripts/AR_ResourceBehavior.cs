using UnityEngine;
using UnityEngine.UIElements;

public class AR_ResourceBehavior
{
    public VisualElement border;
    public VisualElement resourceIcon;
    public Label resourceAmount;


    public AR_ResourceBehavior(VisualTreeAsset asset, int itemAmount)
    {
        TemplateContainer resourceElemente = asset.Instantiate();

        border = resourceElemente.Q<VisualElement>("border");

        resourceIcon = resourceElemente.Q<VisualElement>("resourceIcon");
        resourceIcon.style.backgroundColor = new Color(Random.Range(1,255), Random.Range(1, 255), Random.Range(1, 255),255);

        resourceAmount = resourceElemente.Q<Label>("resourceAmount");
        resourceAmount.text = $"{itemAmount}";

    }
}
