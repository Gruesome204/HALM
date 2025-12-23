using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class MarketMenuBehavior : MonoBehaviour, IMenu
{
    private Label headline;
    private VisualElement ressourceButtonContainer;

    private VisualElement detailsMainContainer;
    private Label subHeadline;
    private VisualElement icon;
    private Label goingRateTxt;
    private Label informationTxt;
    private SliderInt slider;
    private Button sellButton;
    private Button buyButton;

    public VisualTreeAsset ressourceButton;
    private ResourceType openRessourceDetails;

    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the SettingsMenu and adds it to openMenu List
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);
            Fill();
        }
        else
        {
            //Closes SettingsMenu and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
            Clear();
        }
    }
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        //Connect everything to the left
        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("WorkshopMenuTranslationTable", "headline"));

        ressourceButtonContainer = root.Q<VisualElement>("ressourceButtonContainer");

        //Connecting everything on the right
        detailsMainContainer = root.Q<VisualElement>("detailsMainContainer");

        subHeadline = root.Q<Label>("subHeadline");

        icon = root.Q<VisualElement>("icon");

        goingRateTxt = root.Q<Label>("goingRateTxt");
        informationTxt = root.Q<Label>("informationTxt");

        slider = root.Q<SliderInt>("howMuchToSellSlider");

        sellButton = root.Q<Button>("sellButton");
        sellButton.SetBinding("text", new LocalizedString("", "sellButton"));
        sellButton.RegisterCallback<ClickEvent>(OnSellButtonClicked);

        buyButton = root.Q<Button>("buyButton");
        buyButton.SetBinding("text", new LocalizedString("", "buyButton"));
        buyButton.RegisterCallback<ClickEvent>(OnBuyButtonClicked);
    }
    void Fill()
    {
        CreateRessourceButton(ResourceType.Wood, "WoodIcon");
        CreateRessourceButton(ResourceType.Stone, "StoneIcon");
        CreateRessourceButton(ResourceType.Metal, "MetallIcon");
        CreateRessourceButton(ResourceType.Pulver, "BlackpowderIcon");


    }
    private void CreateRessourceButton(ResourceType _type, string _iconClass)
    {
        MM_RessourceButtonBehaviior _ressourceButton = new MM_RessourceButtonBehaviior(ressourceButton, _type, _iconClass, this);
        ressourceButtonContainer.Add(_ressourceButton.border);
    }

    public void FillRessourceDetails(ResourceType _ressource)
    {
        openRessourceDetails = _ressource;

        detailsMainContainer.RemoveFromClassList("");

        subHeadline.SetBinding("text", new LocalizedString("",""));

        icon.AddToClassList("");

        goingRateTxt.SetBinding("text", new LocalizedString("", ""));
    }
    void Clear()
    {

    }

    void ClearRessourceDetails()
    {
        subHeadline.text = "";
        goingRateTxt.text = "";
        informationTxt.text = "";

        slider.value = 1;
    }

    void OnSellButtonClicked(ClickEvent evt)
    {
        if (GameManager.Instance.gameDataSO.HasResource(openRessourceDetails, slider.value))
        {
            //Hat genug zum Verkaufen
        }
        else
        {
            // Hat nicht genug zum verkaufen
            informationTxt.SetBinding("text", new LocalizedString("",""));
        }
    }

    void OnBuyButtonClicked(ClickEvent evt)
    {
        if (GameManager.Instance.gameDataSO.HasResource(ResourceType.Currency, slider.value))
        {
            //Hat genug zum Kaufen
        }
        else
        {
            // Hat nicht genug zum Kaufen
            informationTxt.SetBinding("text", new LocalizedString("", ""));
        }
    }
}
