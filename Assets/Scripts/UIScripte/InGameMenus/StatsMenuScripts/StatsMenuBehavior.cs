using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class StatsMenuBehavior : MonoBehaviour, IMenu
{
    private UIDocument uIDocument;

    private Label statsMenuHeadline;

    private Button resumeButton;
    private Button pauseMenuButton;
    private Button charackterButton;
    private Button relicsButton;
    private Button towerButton;

    public VisualTreeAsset listElementAsset;
    public VisualTreeAsset turretDetailsAsset;
    public VisualTreeAsset charackterDetailsAsset;

    private List<GameObject> turretsCurrentlyPlaced = new List<GameObject>();
    private List<TurretBlueprint> turretsInGame = new List<TurretBlueprint>();


    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the StatsMenu, adds to openMenu List and sets Game to paused
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("pauseMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);
            GameManager.Instance.ChangeState(GameManager.GameState.Paused);
        }
        else
        {
            //Closes StatsMenu, removes from openMenu List, and clears all lists
            root.Q<VisualElement>("mainContainer").AddToClassList("pauseMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
            ClearList();
        }
    }
    void OnEnable()
    {
        uIDocument = GetComponent<UIDocument>();

        //Connect and set Stats Menu Headline
        statsMenuHeadline = uIDocument.rootVisualElement.Q<Label>("statsMenuHeadline");
        statsMenuHeadline.SetBinding("text", new LocalizedString("StatsMenuTranslationTable", "statsMenuHeadline"));

        //Filling The List for Turrets curently placed
        turretsInGame = TurretPlacementController.Instance.GetTurretBlueprintList();
        turretsCurrentlyPlaced = TurretPlacementController.Instance.GetActiveTurrets();

        //Connecting al Buttons and settig their Texts and Functions
        resumeButton = uIDocument.rootVisualElement.Q<Button>("resumeButton");
        resumeButton.SetBinding("text", new LocalizedString("PauseMenuTranslationTable", "resumeButton"));
        resumeButton.RegisterCallback<ClickEvent>(OnResumeBtnClicked);

        pauseMenuButton = uIDocument.rootVisualElement.Q<Button>("pauseMenuButton");
        pauseMenuButton.SetBinding("text", new LocalizedString("PauseMenuTranslationTable", "pauseMenuHeadline"));
        pauseMenuButton.RegisterCallback<ClickEvent>(OnPauseBtnClicked);

        charackterButton = uIDocument.rootVisualElement.Q<Button>("charackterButton");
        charackterButton.SetBinding("text", new LocalizedString("StatsMenuTranslationTable", "charackterButton"));
        charackterButton.RegisterCallback<ClickEvent>(OnCharackterBtnClicked);

        relicsButton = uIDocument.rootVisualElement.Q<Button>("relicsButton");
        relicsButton.SetBinding("text", new LocalizedString("StatsMenuTranslationTable", "relicsButton"));
        relicsButton.RegisterCallback<ClickEvent>(OnRelicsBtnClicked);

        towerButton = uIDocument.rootVisualElement.Q<Button>("towerButton");
        towerButton.SetBinding("text", new LocalizedString("StatsMenuTranslationTable", "towerButton"));
        towerButton.RegisterCallback<ClickEvent>(OnTowerBtnClicked);
    }


    void OnResumeBtnClicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.ReturnToGame();
    }
    void OnPauseBtnClicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.CloseAllMenus();
        InGameMenuManager.Instance.OpenOrCloseOneMenu("PauseMenuDoc", true);
    }
    void OnCharackterBtnClicked(ClickEvent evt)
    {
        ClearList();

        TemplateContainer charackterDetails = charackterDetailsAsset.Instantiate();
        charackterDetails.style.height = new Length(100, LengthUnit.Percent);
        uIDocument.rootVisualElement.Q("statsContainer").Add(charackterDetails);

        charackterDetails.Q<Label>($"name").SetBinding("text", new LocalizedString("ClassTranslationCommon", $"Mechanic"));

        charackterDetails.Q<Label>($"relicsName").SetBinding("text", new LocalizedString("ClassTranslationCommon", $"relics"));
        charackterDetails.Q<Label>($"relics").text = $"0";

        charackterDetails.Q<Label>($"turretsName").SetBinding("text", new LocalizedString("ClassTranslationCommon", $"turrets"));
        charackterDetails.Q<Label>($"turrets").text = $"{turretsInGame.Count}";
    }
    void OnRelicsBtnClicked(ClickEvent evt)
    {
        ClearList();
    }
    void OnTowerBtnClicked(ClickEvent evt)
    {
        ClearList();

        //Creating the Turret Detail Menu, in case it wasn't there before
        TemplateContainer turretDetails = turretDetailsAsset.Instantiate();
        turretDetails.style.height = new Length(100, LengthUnit.Percent);
        uIDocument.rootVisualElement.Q("statsContainer").Add(turretDetails);

        //Creating List Elements for each Turretkind currently placed, giving them the turret they represent and a reference to the turret Detail Menu,
        //and adding the created Button to the scroll List
        foreach (var turret in TurretPlacementController.Instance.GetActiveTurrets())
        {
            SM_TowerListElementBehavior towerElement = new SM_TowerListElementBehavior(turret.GetComponentInChildren<TurretBehaviour>().turretBlueprint, listElementAsset, ref turretDetails);
            uIDocument.rootVisualElement.Q("unity-content-container").Add(towerElement.listButton);
        }
    }

    void ClearList()
    {
        //Ensuring that the Stats Container and scrollBox are empty, so that tower, charakter and relic details won't start overlapping
        uIDocument.rootVisualElement.Q("statsContainer").Clear();
        uIDocument.rootVisualElement.Q("unity-content-container").Clear();
    }
}
