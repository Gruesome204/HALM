using UnityEngine;
using UnityEngine.UIElements;

public class MM_RessourceButtonBehaviior
{
    public VisualElement border;
    private Button button;
    private VisualElement icon;

    private Label ressourceAmount;

    private ResourceType represendetRessource;
    private MarketMenuBehavior marketMenu;
    public MM_RessourceButtonBehaviior(VisualTreeAsset _asset, ResourceType _type, MarketMenuBehavior _marketMenu)
    {
        TemplateContainer buttonElement = _asset.Instantiate();

        represendetRessource = _type;
        marketMenu = _marketMenu;

        border = buttonElement.Q<VisualElement>("border");

        button = buttonElement.Q<Button>("button");
        button.RegisterCallback<ClickEvent>(OnButtonClicked);

        icon = buttonElement.Q<VisualElement>("icon");
        icon.AddToClassList($"{represendetRessource}Icon");

        ressourceAmount = buttonElement.Q<Label>("turretNumber");
        ressourceAmount.text = GameManager.Instance.gameDataSO.GetResourceAmount(_type).ToString();
        this.marketMenu = marketMenu;
    }

    void OnButtonClicked(ClickEvent evt)
    {
        marketMenu.FillRessourceDetails(represendetRessource);
    }
}
