using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class MarketMenuBehavior : MonoBehaviour, IMenu
{
    private Label headline;
    private VisualElement ressourceButtonContainer;
    private Button backBtn;

    private VisualElement detailsMainContainer;
    private Label subHeadline;
    private VisualElement icon;
    private Label goingRateTxt;
    private Label informationTxt;
    private SliderInt slider;

    private Label sellLabel;
    private Button sellButton;
    private Label buyLabel;
    private Button buyButton;

    public VisualTreeAsset ressourceButton;
    private ResourceType openRessourceDetails;

    private int buyingRate;
    private int sellingRate;

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

            //Play a Click sound to give audio feedback to the Player
            SoundManager.Instance.PlayWoodMenuOpen();

            //This ensures that the picture in the back can grow as large as possible while keeping it's correct scale :D
            //It can only be run when the Menu is actually on screen, otherwise the numbers go hairwire O.o
            // I wanted to run this once in the Enable Method, but due to the above that didn't work :C
            //I'm leaving it like this for now, it works fine so just close your eyes and look elswhere Lukas XD
            var container = root.Q<VisualElement>("optionsContainer");
            container.style.width = (int)((int)container.resolvedStyle.height / (float)1.368);
        }
        else
        {
            //Closes SettingsMenu and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
            Clear();
            GameManager.Instance.SaveGame();
        }
    }
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        //Connect everything to the left
        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("MarketMenuTranslationTable", "headline"));

        ressourceButtonContainer = root.Q<VisualElement>("ressourceButtonContainer");

        backBtn = root.Q<Button>("backBtn");
        backBtn.RegisterCallback<ClickEvent>(OnBackBtnClicked);

        //Connecting everything on the right
        detailsMainContainer = root.Q<VisualElement>("detailsMainContainer");
        detailsMainContainer.AddToClassList("turretChoiceMenuSlideOut");

        subHeadline = root.Q<Label>("subHeadline");

        icon = root.Q<VisualElement>("icon");

        goingRateTxt = root.Q<Label>("goingRateTxt");
        informationTxt = root.Q<Label>("informationTxt");

        slider = root.Q<SliderInt>("howMuchToSellSlider");
        slider.RegisterValueChangedCallback(v =>
        {
            informationTxt.text = $"x{slider.value} "
                                + LocalizationSettings.StringDatabase.GetTable("WorkshopMenuTranslationTable").GetEntry($"{openRessourceDetails}").GetLocalizedString()
                                + LocalizationSettings.StringDatabase.GetTable("MarketMenuTranslationTable").GetEntry($"selectedAmount").GetLocalizedString();

            sellLabel.text = LocalizationSettings.StringDatabase.GetTable("MarketMenuTranslationTable").GetEntry($"sellLabel").GetLocalizedString()
                            + $"{slider.value * sellingRate}x "
                            + LocalizationSettings.StringDatabase.GetTable("WorkshopMenuTranslationTable").GetEntry($"{ResourceType.Currency}").GetLocalizedString();

            buyLabel.text = LocalizationSettings.StringDatabase.GetTable("MarketMenuTranslationTable").GetEntry($"buyLabel").GetLocalizedString()
                            + $"{slider.value * buyingRate}x "
                            + LocalizationSettings.StringDatabase.GetTable("WorkshopMenuTranslationTable").GetEntry($"{ResourceType.Currency}").GetLocalizedString();
        });

        sellLabel = root.Q<Label>("sellLabel");

        sellButton = root.Q<Button>("sellButton");
        sellButton.SetBinding("text", new LocalizedString("MarketMenuTranslationTable", "sellButton"));
        sellButton.RegisterCallback<ClickEvent>(OnSellButtonClicked);

        buyLabel = root.Q<Label>("buyLabel");

        buyButton = root.Q<Button>("buyButton");
        buyButton.SetBinding("text", new LocalizedString("MarketMenuTranslationTable", "buyButton"));
        buyButton.RegisterCallback<ClickEvent>(OnBuyButtonClicked);
    }
    void Fill()
    {
        Clear();
        CreateRessourceButton(ResourceType.Currency);
        CreateRessourceButton(ResourceType.Wood);
        CreateRessourceButton(ResourceType.Stone);
        CreateRessourceButton(ResourceType.Metal);
        CreateRessourceButton(ResourceType.Pulver);
    }
    private void CreateRessourceButton(ResourceType _type)
    {
        MM_RessourceButtonBehaviior _ressourceButton = new MM_RessourceButtonBehaviior(ressourceButton, _type, this);
        ressourceButtonContainer.Add(_ressourceButton.border);
    }

    public void FillRessourceDetails(ResourceType _ressource)
    {
        //This ensures that the picture in the back can grow as large as possible while keeping it's correct scale :D
        //It can only be run when the Menu is actually on screen, otherwise the numbers go hairwire O.o
        // I wanted to run this once in the Enable Method, but due to the above that didn't work :C
        //I'm leaving it like this for now, it works fine so just close your eyes and look elswhere Lukas XD
        var root = GetComponent<UIDocument>().rootVisualElement;
        var container = root.Q<VisualElement>("detailsMainContainer");
        container.style.width = (int)((int)container.resolvedStyle.height / (float)1.485);

        ClearRessourceDetails();
        openRessourceDetails = _ressource;

        detailsMainContainer.RemoveFromClassList("turretChoiceMenuSlideOut");

        string specificSubHeadline = new string(LocalizationSettings.StringDatabase.GetTable("WorkshopMenuTranslationTable").GetEntry($"{openRessourceDetails}").GetLocalizedString());
        string commonSubHeadline = new string(LocalizationSettings.StringDatabase.GetTable("MarketMenuTranslationTable").GetEntry("subHeadline").GetLocalizedString());
        subHeadline.text = specificSubHeadline + " " + commonSubHeadline;

        icon.AddToClassList($"{openRessourceDetails}Icon");

        goingRateTxt.SetBinding("text", new LocalizedString("MarketMenuTranslationTable", $"{openRessourceDetails}GoingRate"));

        switch (openRessourceDetails)
        {
            case ResourceType.Currency:
                buyingRate = 1;
                sellingRate = 1000;
                break;
            case ResourceType.Wood:
                buyingRate = 3;
                sellingRate = 2;
                break;
            case ResourceType.Stone:
                buyingRate = 5;
                sellingRate = 3;
                break;
            case ResourceType.Metal:
                buyingRate = 8;
                sellingRate = 5;
                break;
            case ResourceType.Pulver:
                buyingRate = 12;
                sellingRate = 8;
                break;
        }
    }
    void Clear()
    {
        ressourceButtonContainer.Clear();
        ClearRessourceDetails();
    }

    void ClearRessourceDetails()
    {
        detailsMainContainer.AddToClassList("turretChoiceMenuSlideOut");

        subHeadline.text = "";
        goingRateTxt.text = "";
        informationTxt.text = "";

        sellLabel.text = "";
        buyLabel.text = "";

        slider.value = 2;
        slider.value = 1;
        icon.RemoveFromClassList($"{openRessourceDetails}Icon");
    }

    void OnSellButtonClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayWoodClick();

        if (GameManager.Instance.gameDataSO.HasResource(openRessourceDetails, slider.value))
        {
            //Hat genug zum Verkaufen
            GameManager.Instance.gameDataSO.RemoveResource(openRessourceDetails, slider.value);
            GameManager.Instance.gameDataSO.AddResource(ResourceType.Currency, slider.value * sellingRate);
            Fill();
            FillRessourceDetails(openRessourceDetails);
        }
        else
        {
            // Hat nicht genug zum verkaufen
            informationTxt.text = LocalizationSettings.StringDatabase.GetTable("MarketMenuTranslationTable").GetEntry($"notEnough").GetLocalizedString()
                                + LocalizationSettings.StringDatabase.GetTable("WorkshopMenuTranslationTable").GetEntry($"{openRessourceDetails}").GetLocalizedString();
        }
        GameManager.Instance.SaveGame();
    }

    void OnBuyButtonClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayWoodClick();

        if (GameManager.Instance.gameDataSO.HasResource(ResourceType.Currency, slider.value * buyingRate))
        {
            //Hat genug zum Kaufen
            GameManager.Instance.gameDataSO.RemoveResource(ResourceType.Currency, slider.value * buyingRate);
            GameManager.Instance.gameDataSO.AddResource(openRessourceDetails, slider.value);
            Fill();
            FillRessourceDetails(openRessourceDetails);
        }
        else
        {
            // Hat nicht genug zum Kaufen
            informationTxt.text = LocalizationSettings.StringDatabase.GetTable("MarketMenuTranslationTable").GetEntry($"notEnough").GetLocalizedString()
                                + LocalizationSettings.StringDatabase.GetTable("WorkshopMenuTranslationTable").GetEntry($"{ResourceType.Currency}").GetLocalizedString();
        }
        GameManager.Instance.SaveGame();
    }

    private void OnBackBtnClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayWoodClick();

        InGameMenuManager.Instance.OpenOrCloseOneMenu("MarktMenuDoc", false);
    }
}
