using UnityEngine;
using UnityEngine.UIElements;

public class AR_ResourceBehavior
{
    public VisualElement border;
    public VisualElement resourceIcon;
    public VisualElement addContainer;
    public Label resourceAmount;

    private ResourceType representedRessource;
    public AR_ResourceBehavior(VisualTreeAsset asset, ResourceType _type)
    {
        TemplateContainer resourceElemente = asset.Instantiate();

        representedRessource = _type;

        border = resourceElemente.Q<VisualElement>("border");

        resourceIcon = resourceElemente.Q<VisualElement>("resourceIcon");
        resourceIcon.AddToClassList($"{representedRessource}Icon");

        resourceAmount = resourceElemente.Q<Label>("resourceAmount");
        resourceAmount.text = $"{GameManager.Instance.gameDataSO.GetResourceAmount(representedRessource)}";
    }

    public ResourceType GetRessourceType()
    {
        return representedRessource;
    }

    public void CreateAddRessourceElement(VisualTreeAsset asset, int addedAmount, ResourceType _type)
    {
        //AR_RessourceAddedBehavior test = new AR_RessourceAddedBehavior(asset, addedAmount, _type);
        //addContainer.Add(test.container);
    }
}
