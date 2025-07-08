using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class StatsMenuBehavior : MonoBehaviour
{
    private InGameMenuManager gameMenuManger;
    private UIDocument uIDocument;

    private Label statsMenuHeadline;
    private Button resumeButton;
    private Button charackterButton;
    private Button relicsButton;
    private Button towerButton;

    private VisualElement lowerButtonContainer;
    public VisualTreeAsset listElementAsset;
    public VisualTreeAsset turretDetailsAsset;
    public VisualTreeAsset charackterDetailsAsset;

    private HashSet<TurretBlueprint> turretsCurrentlyPlaced = new HashSet<TurretBlueprint>();
    private List<TurretBlueprint> turretsInGame = new List<TurretBlueprint>();

    void OnEnable()
    {
        uIDocument = GetComponent<UIDocument>();
        gameMenuManger = FindObjectOfType<InGameMenuManager>();

        //Connect and set Stats Menu Headline
        statsMenuHeadline = uIDocument.rootVisualElement.Q<Label>("statsMenuHeadline");
        statsMenuHeadline.SetBinding("text", new LocalizedString("StatsMenuTranslationTable", "statsMenuHeadline"));

        //Filling The List for Turrets curently placed
        turretsInGame = TurretPlacement.Instance.GetTurretBlueprintList();
        turretsCurrentlyPlaced = TurretPlacement.Instance.GetInstantiatedTurretList();

        //Connecting al Buttons and settig their Texts and Functions
        resumeButton = uIDocument.rootVisualElement.Q<Button>("resumeButton");
        resumeButton.SetBinding("text", new LocalizedString("PauseMenuTranslationTable", "resumeButton"));
        resumeButton.RegisterCallback<ClickEvent>(OnResumeBtnClicked);

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
        gameMenuManger.ReturnToGame();
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
        foreach (var turrets in turretsCurrentlyPlaced)
        {
            SM_TowerListElementBehavior towerElement = new SM_TowerListElementBehavior(turrets, listElementAsset, ref turretDetails);
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
