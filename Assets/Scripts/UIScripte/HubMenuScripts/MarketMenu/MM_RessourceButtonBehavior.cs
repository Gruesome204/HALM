using UnityEngine;
using UnityEngine.UIElements;

public class MM_RessourceButtonBehaviior
{
    public VisualElement border;
    private Button button;

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
        button.AddToClassList($"{represendetRessource}Icon");
        button.RegisterCallback<ClickEvent>(OnButtonClicked);

        ressourceAmount = buttonElement.Q<Label>("turretNumber");
        ressourceAmount.text = GameManager.Instance.gameDataSO.GetResourceAmount(_type).ToString();
    }

    void OnButtonClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayWoodClick();

        marketMenu.FillRessourceDetails(represendetRessource);

        marketMenu.slider.value = 2;
        marketMenu.slider.value = 1;
    }
}
