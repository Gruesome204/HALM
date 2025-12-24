using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class AR_RessourceAddedBehavior 
{
    public VisualElement container;

    private Label addedNumber;
    private VisualElement icon;

    public AR_RessourceAddedBehavior(VisualTreeAsset _asset, int _addedNumber, ResourceType _type)
    {
        TemplateContainer asset = _asset.Instantiate();

        container = asset.Q<VisualElement>("container");

        addedNumber = asset.Q<Label>("addedNumber");
        addedNumber.text = _addedNumber.ToString();

        icon = asset.Q<VisualElement>("icon");
        icon.AddToClassList($"{_type}Icon");
    }

    private void DestroySlef()
    {
        Debug.Log("Kamikaze");
        container.GetFirstAncestorOfType<VisualElement>().Remove(container);
    }
}
