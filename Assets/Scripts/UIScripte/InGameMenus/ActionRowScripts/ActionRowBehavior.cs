using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class ActionRowBehavior : MonoBehaviour
{
    private InGameMenuManager gameMenuManager;

    private VisualElement turretBTNContainer;
    private Button pauseMenuButton;
    private Button statsMenuButton;

    public VisualTreeAsset actionRowElementAsset;

    private List<AR_ElementBehavior> turretBtnList = new List<AR_ElementBehavior>();
    private List<TurretBlueprint> turretsInGame = new List<TurretBlueprint>();
    private HashSet<TurretBlueprint> turretsCurrentlyPlaced = new HashSet<TurretBlueprint>();

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        gameMenuManager = FindObjectOfType<InGameMenuManager>();

        turretsInGame = TurretPlacementController.Instance.GetTurretBlueprintList();
        turretsCurrentlyPlaced = TurretPlacementController.Instance.GetInstantiatedTurretList();

        pauseMenuButton = root.Q<Button>("pauseMenuButton");
        pauseMenuButton.SetBinding("text", new LocalizedString("ActionRowTranslationTable", "pauseMenuButton"));
        pauseMenuButton.RegisterCallback<ClickEvent>(PauseBtnClicked);

        statsMenuButton = root.Q<Button>("statsMenuButton");
        statsMenuButton.SetBinding("text", new LocalizedString("ActionRowTranslationTable", "statsMenuButton"));
        statsMenuButton.RegisterCallback<ClickEvent>(StatsBtnClicked);

        FillActionRow();
    }
    private void Update()
    {
        foreach (var btn in turretBtnList)
        {
            btn.ChangeColor();
        }
    }
    void FillActionRow()
    {
        turretBtnList.Clear();

        foreach (var turret in turretsInGame)
        {
            Debug.Log("whyyyy");
            AR_ElementBehavior aR_Element = new AR_ElementBehavior(actionRowElementAsset, turret);
            GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("turrentButtonContainer").Add(aR_Element.turretBorder);
            turretBtnList.Add(aR_Element);
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

    void SetCurrentTurret(int clickedBtn)
    {
        FindObjectOfType<TurretPlacementController>().currentBlueprint = turretsInGame[clickedBtn];
    }
}
