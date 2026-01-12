using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;
using static BuildMasterModifier;

public class BuildmasterModifyBehavior : MonoBehaviour, IMenu
{
    private Label headline;
    private Label subHeadline;
    private Label informationTxt;
    private Label costName;
    private Label cost;

    private VisualElement btnContainer;
    private VisualElement appliedModifierContainer;

    private Button backBtn;
    private Button buySelectButton;

    private BuildMasterModifier openModifierDetails;
    public VisualTreeAsset modifierButtonAsset;


    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the SettingsMenu and adds it to openMenu List
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);
            FillMenu();

            //Play a Click sound to give audio feedback to the Player
            SoundManager.Instance.PlayWoodMenuOpen();

            //This ensures that the picture in the back can grow as large as possible while keeping it's correct scale :D
            //It can only be run when the Menu is actually on screen, otherwise the numbers go hairwire O.o
            // I wanted to run this once in the Enable Method, but due to the above that didn't work :C
            //I'm leaving it like this for now, it works fine so just close your eyes and look elswhere Lukas XD
            var container = root.Q<VisualElement>("possibleModifierList");
            container.style.width = (int)((int)container.resolvedStyle.height / (float)1.368);
        }
        else
        {
            //Closes SettingsMenu and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
            ClearMenu();
            GameManager.Instance.SaveGame();
        }
    }

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("BuildmasterModifyTranslationTable", "headline"));

        subHeadline = root.Q<Label>("subHeadline");
        subHeadline.SetBinding("text", new LocalizedString("BuildmasterModifyTranslationTable", "subHeadline"));

        informationTxt = root.Q<Label>("informationTxt");
        informationTxt.text = "";

        costName = root.Q<Label>("costName");
        costName.text = "";

        cost = root.Q<Label>("cost");
        cost.text = "";

        btnContainer = root.Q<VisualElement>("btnContainer");

        appliedModifierContainer = root.Q<VisualElement>("appliedModifierContainer");

        backBtn = root.Q<Button>("backBtn");
        backBtn.RegisterCallback<ClickEvent>(OnBackBtnClicked);

        buySelectButton = root.Q<Button>("buySelectButton");
    }


    private void FillMenu()
    {
        ClearMenu();
        var uiDocument = this.gameObject.GetComponent<UIDocument>();

        foreach (var modifier in GameManager.Instance.gameDataSO.allBuildMasterModifiers)
        {
            if (GameManager.Instance.gameDataSO.GetSelectedModifiers().Contains<BuildMasterModifier>(modifier))
            {
                //All Selected Modifiers end up here
                var btn = new BM_AppliedModifierBehavior(modifierButtonAsset, modifier, this);
                appliedModifierContainer.Add(btn.mainContainer);
            }
            else if(GameManager.Instance.gameDataSO.GetUnlockedModifiers().Contains<BuildMasterModifier>(modifier))
            {
                //All Unlocked, but not Selected modifiers end here
                var btn = new BM_ModifierButtonBehavior(modifierButtonAsset, modifier, this);
                btn.modifierUnlocked = true;
                uiDocument.rootVisualElement.Q("unity-content-container").Add(btn.mainContainer);
            }
            else
            {
                //All Locked Modifiers end here
                var btn = new BM_ModifierButtonBehavior(modifierButtonAsset, modifier, this);
                btn.cooldownCover.style.height = new Length(100, LengthUnit.Percent);
                uiDocument.rootVisualElement.Q("unity-content-container").Add(btn.mainContainer);
            }       
        }
    }

    private void ClearMenu()
    {
        var uiDocument = this.gameObject.GetComponent<UIDocument>();

        uiDocument.rootVisualElement.Q("unity-content-container").Clear();
        appliedModifierContainer.Clear();

        costName.text = "";
        cost.text = "";
        informationTxt.text = "";

        buySelectButton.UnregisterCallback<ClickEvent>(ModifierLocked);
        buySelectButton.UnregisterCallback<ClickEvent>(ModifierUnlocked);
        buySelectButton.UnregisterCallback<ClickEvent>(ModifierSelected);
        buySelectButton.AddToClassList("vanish");
    }
    public void ClearDetails()
    {
        costName.text = "";
        cost.text = "";
        informationTxt.text = "";

        buySelectButton.UnregisterCallback<ClickEvent>(ModifierLocked);
        buySelectButton.UnregisterCallback<ClickEvent>(ModifierUnlocked);
        buySelectButton.UnregisterCallback<ClickEvent>(ModifierSelected);
        buySelectButton.AddToClassList("vanish");
    }

    public void FillModifierDetails(BuildMasterModifier modifier, bool modifierUnlocked, bool modifierSelected)
    {
        //This ensures that the picture in the back can grow as large as possible while keeping it's correct scale :D
        //It can only be run when the Menu is actually on screen, otherwise the numbers go hairwire O.o
        // I wanted to run this once in the Enable Method, but due to the above that didn't work :C
        //I'm leaving it like this for now, it works fine so just close your eyes and look elswhere Lukas XD
        var root = GetComponent<UIDocument>().rootVisualElement;
        var container = root.Q<VisualElement>("appliedModifierList");
        container.style.width = (int)((int)container.resolvedStyle.height / (float)1.485);


        openModifierDetails = modifier;
        informationTxt.text = $"{openModifierDetails.options.name}";

        buySelectButton.RemoveFromClassList("vanish");

        if (modifierSelected)
        {
            //All Selected Modifiers land here
            buySelectButton.SetBinding("text", new LocalizedString("WorkshopMenuTranslationTable", "selectedButton"));
            buySelectButton.RegisterCallback<ClickEvent>(ModifierSelected);
        }
        else if (modifierUnlocked)
        {
            //All Unlocked, but not selected modifers land here
            buySelectButton.SetBinding("text", new LocalizedString("WorkshopMenuTranslationTable", "unlockedButton"));
            buySelectButton.RegisterCallback<ClickEvent>(ModifierUnlocked);
        }
        else
        {
            // All locked Modifiers land here
            buySelectButton.SetBinding("text", new LocalizedString("WorkshopMenuTranslationTable", "lockedButton"));
            buySelectButton.RegisterCallback<ClickEvent>(ModifierLocked);
            FillCostValue();
        }
    }
    void FillCostValue()
    {
        costName.SetBinding("text", new LocalizedString("TurretTranslationCommon", $"cost"));
        StringTable table = LocalizationSettings.StringDatabase.GetTable("WorkshopMenuTranslationTable");

        string turretCost = new string("");
        foreach (var cost in openModifierDetails.options.costs)
        {
            turretCost = turretCost + $"{table.GetEntry($"{cost.resourceType}").GetLocalizedString()} {cost.amount}<br>";
        }
        cost.text = turretCost;
    }
    private Boolean ModifierCanBeBought()
    {
        var canBeBought = new Boolean();
        canBeBought = true;

        foreach (var cost in openModifierDetails.options.costs)
        {
            if (GameManager.Instance.gameDataSO.HasResource(cost.resourceType, cost.amount))
            {

            }
            else
            {
                canBeBought = false;
                return canBeBought;
            }

        }
        return canBeBought;
    }

    private void ModifierLocked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayWoodClick();

        if (ModifierCanBeBought())
        {
            foreach (var cost in openModifierDetails.options.costs)
            {
                GameManager.Instance.gameDataSO.RemoveResource(cost.resourceType, cost.amount);
            }
            GameManager.Instance.gameDataSO.AddUnlockedModifier(openModifierDetails);
            FillMenu();
            FillModifierDetails(openModifierDetails, true, false);
        }
        else
        {
            informationTxt.SetBinding("text", new LocalizedString("WorkshopMenuTranslationTable", "cantBuyText"));
        }
    }
    private void ModifierUnlocked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayWoodClick();

        if (GameManager.Instance.gameDataSO.SelectModifier(openModifierDetails))
        {
            FillMenu();
            FillModifierDetails(openModifierDetails, true, true);
        }
        else
        {
            informationTxt.SetBinding("text", new LocalizedString("WorkshopMenuTranslationTable", "cantSelectText"));
        }
    }
    private void ModifierSelected(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayWoodClick();

        GameManager.Instance.gameDataSO.DeselectModifier(openModifierDetails);
        FillMenu();
        FillModifierDetails(openModifierDetails, true, false);
    }
    private void OnBackBtnClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayWoodClick();

        InGameMenuManager.Instance.OpenOrCloseOneMenu("BuildmasterModifyDoc", false);
    }
}
