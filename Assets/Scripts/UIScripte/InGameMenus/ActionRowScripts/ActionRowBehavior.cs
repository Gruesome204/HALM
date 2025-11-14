using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class ActionRowBehavior : MonoBehaviour, IMenu
{
    private InGameMenuManager gameMenuManager;

    private VisualElement turretButtonContainer;
    private VisualElement resourceContainer;
    private VisualElement towerLimitContainer;
    private Button pauseMenuButton;
    private Button statsMenuButton;

    public VisualTreeAsset actionRowElementAsset;
    public VisualTreeAsset resourceElementAsset;
    public VisualTreeAsset towerLimitElement;

    private List<AR_ElementBehavior> turretBtnList = new List<AR_ElementBehavior>();
    private List<AR_TowerLimitElementBehavior> towerLimitElementList = new List<AR_TowerLimitElementBehavior>();


    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the ActionRow and sets Game to playing
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("actionRowSlideOut");
            GameManager.Instance.ChangeState(GameManager.GameState.Playing);
            Debug.Log("Open ActionRow");
        }
        else
        {
            //Closes ActionRow
            root.Q<VisualElement>("mainContainer").AddToClassList("actionRowSlideOut");
        }
    }
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        gameMenuManager = FindObjectOfType<InGameMenuManager>();


        turretButtonContainer = root.Q<VisualElement>("turrentButtonContainer");
        TurretDemolitionController.Instance.OnDemolitionModeChange += TurretButtonContainerColor;
        TurretButtonContainerColor();

        resourceContainer = root.Q<VisualElement>("resourceContainer");

        towerLimitContainer = root.Q<VisualElement>("towerLimitContainer");

        pauseMenuButton = root.Q<Button>("pauseMenuButton");
        pauseMenuButton.SetBinding("text", new LocalizedString("ActionRowTranslationTable", "pauseMenuButton"));
        pauseMenuButton.RegisterCallback<ClickEvent>(PauseBtnClicked);

        statsMenuButton = root.Q<Button>("statsMenuButton");
        statsMenuButton.SetBinding("text", new LocalizedString("ActionRowTranslationTable", "statsMenuButton"));
        statsMenuButton.RegisterCallback<ClickEvent>(StatsBtnClicked);

        FillActionRow();
        FillResourceRow();
        FillTowerLimitBar(TurretPlacementController.Instance.maxTurretCapacity);

        TurretPlacementController.Instance.OnTurretsChanged += UpdateTowerLimitBar;
    }

    void OnDisable()
    {
        if (TurretPlacementController.Instance != null)
            TurretPlacementController.Instance.OnTurretsChanged -= UpdateTowerLimitBar;

        TurretDemolitionController.Instance.OnDemolitionModeChange -= TurretButtonContainerColor;
    }

    private void FixedUpdate()
    {
        foreach (var btn in turretBtnList)
        {
            btn.ChangeColor();
        }
    }

    void FillActionRow()
    {
        turretBtnList.Clear();
        var turretNumber = new int();
        turretNumber = 1;
        foreach (var turret in TurretPlacementController.Instance.GetTurretBlueprintList())
        {
            AR_ElementBehavior aR_Element = new AR_ElementBehavior(actionRowElementAsset, turret, turretNumber);
            turretButtonContainer.Add(aR_Element.turretBorder);
            turretBtnList.Add(aR_Element);

            turretNumber++;
        }
    }

    void FillResourceRow()
    {
        for (int i = 0; i < UnityEngine.Random.Range(1,6); i++)
        {
            AR_ResourceBehavior aR_Resource = new AR_ResourceBehavior(resourceElementAsset, UnityEngine.Random.Range(5,50));
            resourceContainer.Add(aR_Resource.border);
        }
    }

    void PauseBtnClicked(ClickEvent evt)
    {
        gameMenuManager.CloseAllMenus();
        gameMenuManager.OpenOrCloseOneMenu("PauseMenuDoc", true);

    }

    void StatsBtnClicked(ClickEvent evt)
    {
        gameMenuManager.CloseAllMenus();
        gameMenuManager.OpenOrCloseOneMenu("StatsMenuDoc", true);
    }

    void FillTowerLimitBar(int towerLimit)
    {
        for (int i = 1; i < towerLimit +1; i++)
        {
            AR_TowerLimitElementBehavior aR_TowerLimitElement = new AR_TowerLimitElementBehavior(towerLimitElement, i);
            towerLimitContainer.Add(aR_TowerLimitElement.border);
            towerLimitElementList.Add(aR_TowerLimitElement);

            UpdateTowerLimitBar();
        }
    }

    public void UpdateTowerLimitBar()
    {
       int t = TurretPlacementController.Instance.GetUsedCapacity();
        foreach ( var limitElement in towerLimitElementList)
        {
            limitElement.UpdateColor(t);
        }
    }
    void SetCurrentTurret(int clickedBtn)
    {
        FindObjectOfType<TurretPlacementController>().currentSelectedBlueprint = TurretPlacementController.Instance.GetTurretBlueprintList()[clickedBtn];
    }

    public void TurretButtonContainerColor()
    {
        if (TurretDemolitionController.Instance.IsDestructionModeActive())
        {
            turretButtonContainer.style.backgroundColor = Color.red;
        }
        else
        {
            turretButtonContainer.style.backgroundColor = Color.grey;
        }
    }
}
