using UnityEngine;
using UnityEngine.UIElements;

public class AR_ResourceBehavior
{
    public VisualElement border;
    public VisualElement resourceIcon;
    public VisualElement addContainer;
    public Label resourceAmount;

    private ResourceType representedRessource;
    private VisualTreeAsset resourceAsset;

    public AR_ResourceBehavior(VisualTreeAsset asset, ResourceType _type, VisualTreeAsset addRessourceAsset)
    {
        representedRessource = _type;
        resourceAsset = addRessourceAsset;

        // Instantiate the main resource UI
        TemplateContainer resourceElemente = asset.Instantiate();

        border = resourceElemente.Q<VisualElement>("border");
        addContainer = resourceElemente.Q<VisualElement>("addContainer");

        resourceIcon = resourceElemente.Q<VisualElement>("resourceIcon");
        resourceIcon.AddToClassList($"{representedRessource}Icon");

        resourceAmount = resourceElemente.Q<Label>("resourceAmount");
        resourceAmount.text = GameManager.Instance.gameDataSO.GetResourceAmount(representedRessource).ToString();

        // Subscribe to resource changes
        GameManager.Instance.gameDataSO.OnResourceChanged += UpdateResourceUI;
    }

    private void UpdateResourceUI(ResourceType type, int newAmount)
    {
        if (type != representedRessource) return;
        resourceAmount.text = GameManager.Instance.gameDataSO.GetResourceAmount(type).ToString();

        CreateAddRessourceElement(resourceAsset, newAmount, type);
    }

    public ResourceType GetRessourceType()
    {
        return representedRessource;
    }

    public void CreateAddRessourceElement(VisualTreeAsset asset, int addedAmount, ResourceType _type)
    {
        if (_type != representedRessource) return;

        TemplateContainer addElement = asset.Instantiate();
        Label addedNumber = addElement.Q<Label>("addedNumber");
        addedNumber.text = $"+{addedAmount}";

        addContainer.Add(addElement);
        // Schedule removal after 1 second
        addElement.schedule
            .Execute(() => addElement.RemoveFromHierarchy())
            .StartingIn(2000); // Delay in milliseconds
    }
}
