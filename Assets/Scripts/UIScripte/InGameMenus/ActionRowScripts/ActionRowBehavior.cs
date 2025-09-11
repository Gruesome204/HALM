using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class ActionRowBehavior : MonoBehaviour
{
    private InGameMenuManager gameMenuManager;

    private VisualElement turrentButtonContainer;
    private VisualElement resourceContainer;
    private VisualElement towerLimitContainer;
    private Button pauseMenuButton;
    private Button statsMenuButton;

    public VisualTreeAsset actionRowElementAsset;
    public VisualTreeAsset resourceElementAsset;
    public VisualTreeAsset towerLimitElement;

    private List<AR_ElementBehavior> turretBtnList = new List<AR_ElementBehavior>();
    private List<TurretBlueprint> turretsInGame = new List<TurretBlueprint>();
    private List<GameObject> turretsCurrentlyPlaced = new List<GameObject>();
    private List<AR_TowerLimitElementBehavior> towerLimitElementList = new List<AR_TowerLimitElementBehavior>();

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        gameMenuManager = FindObjectOfType<InGameMenuManager>();

        turretsInGame = TurretPlacementController.Instance.GetTurretBlueprintList();
        turretsCurrentlyPlaced = TurretPlacementController.Instance.GetActiveTurrets();

        turrentButtonContainer = root.Q<VisualElement>("turrentButtonContainer");

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
        FillTowerLimitBar(7);
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
        foreach (var turret in turretsInGame)
        {
            AR_ElementBehavior aR_Element = new AR_ElementBehavior(actionRowElementAsset, turret, turretNumber);
            turrentButtonContainer.Add(aR_Element.turretBorder);
            turretBtnList.Add(aR_Element);

            turretNumber++;
        }
    }

    void FillResourceRow()
    {
        for (int i = 0; i < Random.Range(1,6); i++)
        {
            AR_ResourceBehavior aR_Resource = new AR_ResourceBehavior(resourceElementAsset, Random.Range(5,50));
            resourceContainer.Add(aR_Resource.border);
        }
    }

    void PauseBtnClicked(ClickEvent evt)
    {
        gameMenuManager.CloseAllMenus();
        gameMenuManager.OpenOneInGameMenu(1);

    }

    void StatsBtnClicked(ClickEvent evt)
    {
        gameMenuManager.CloseAllMenus();
        gameMenuManager.OpenOneInGameMenu(2);
    }

    void FillTowerLimitBar(int towerLimit)
    {
        for (int i = 0; i < towerLimit; i++)
        {
            AR_TowerLimitElementBehavior aR_TowerLimitElement = new AR_TowerLimitElementBehavior(towerLimitElement, i);
            towerLimitContainer.Add(aR_TowerLimitElement.border);
            towerLimitElementList.Add(aR_TowerLimitElement);

            UpdateTowerLimitBar(TurretPlacementController.Instance.GetNumActiveTurrets());
        }
    }

    public void UpdateTowerLimitBar(int _towersPlaced)
    {
        foreach ( var limitElement in towerLimitElementList)
        {
            limitElement.UpdateColor(_towersPlaced);
        }
    }
    void SetCurrentTurret(int clickedBtn)
    {
        FindObjectOfType<TurretPlacementController>().currentSelectedBlueprint = turretsInGame[clickedBtn];
    }
}
