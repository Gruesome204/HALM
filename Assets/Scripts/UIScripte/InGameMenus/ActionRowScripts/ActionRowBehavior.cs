using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ActionRowBehavior : MonoBehaviour, IMenu
{
    [SerializeField] GameDataSO gameDataSO;

    private VisualElement turretButtonContainer;
    private VisualElement resourceContainer;
    private VisualElement towerLimitContainer;
    private Button pauseMenuButton;
    private Button statsMenuButton;

    public VisualTreeAsset actionRowElementAsset;
    public VisualTreeAsset resourceElementAsset;
    public VisualTreeAsset ressourceAddedElement;
    public VisualTreeAsset towerLimitElement;

    private List<AR_ElementBehavior> turretBtnList = new List<AR_ElementBehavior>();
    private List<AR_TowerLimitElementBehavior> towerLimitElementList = new List<AR_TowerLimitElementBehavior>();

    private void Awake()
    {
        gameDataSO = GameManager.Instance.gameDataSO;
    }

    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            FillActionRow();
            //Open the ActionRow and sets Game to playing
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("actionRowSlideOut");
            GameManager.Instance.ChangeState(GameManager.GameState.Playing);
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
            btn.HandleVisualCooldown();
        }
    }


    void FillActionRow()
    {
        turretBtnList.Clear();
        turretButtonContainer.Clear();
        var turretNumber = new int();
        turretNumber = 1;
        foreach (var turret in GameManager.Instance.gameDataSO.GetSelectedBlueprints())
        {
            Debug.Log(GameManager.Instance.gameDataSO.GetSelectedBlueprints().Count);
            AR_ElementBehavior aR_Element = new AR_ElementBehavior(actionRowElementAsset, turret, turretNumber);
            turretButtonContainer.Add(aR_Element.turretBorder);
            turretBtnList.Add(aR_Element);

            turretNumber++;
        }
    }

    void FillResourceRow()
    {
        resourceContainer.Clear();
        CreateRessourceRowElement(ResourceType.Currency);
        CreateRessourceRowElement(ResourceType.Wood);
        CreateRessourceRowElement(ResourceType.Stone);
        CreateRessourceRowElement(ResourceType.Metal);
        CreateRessourceRowElement(ResourceType.Pulver);

    }

    private void CreateRessourceRowElement(ResourceType _type)
    {
        AR_ResourceBehavior aR_Resource = new AR_ResourceBehavior(resourceElementAsset, _type, ressourceAddedElement);
        resourceContainer.Add(aR_Resource.border);
    }

    void PauseBtnClicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.CloseAllMenus();
        InGameMenuManager.Instance.OpenOrCloseOneMenu("PauseMenuDoc", true);

    }

    void StatsBtnClicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.CloseAllMenus();
        InGameMenuManager.Instance.OpenOrCloseOneMenu("StatsMenuDoc", true);
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


    public void TurretButtonContainerColor()
    {
        if (TurretDemolitionController.Instance.IsDestructionModeActive())
        {
            turretButtonContainer.style.unityBackgroundImageTintColor = Color.red;
        }
        else
        {
            turretButtonContainer.style.unityBackgroundImageTintColor = Color.white;
        }
    }
}
