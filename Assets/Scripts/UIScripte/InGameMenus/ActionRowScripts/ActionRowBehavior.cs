using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class ActionRow : MonoBehaviour
{
    private InGameMenuManager gameMenuManager;

    private VisualElement turretBTNContainer;
    private Button pauseMenuButton;
    private Button statsMenuButton;

    private List<TurretBlueprint> turretsInGame = new List<TurretBlueprint>();
    private HashSet<TurretBlueprint> turretsCurrentlyPlaced = new HashSet<TurretBlueprint>();

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        gameMenuManager = FindObjectOfType<InGameMenuManager>();

        turretsInGame = TurretPlacement.Instance.GetTurretBlueprintList();
        turretsCurrentlyPlaced = TurretPlacement.Instance.GetInstantiatedTurretList();

        pauseMenuButton = root.Q<Button>("pauseMenuButton");
        pauseMenuButton.SetBinding("text", new LocalizedString("ActionRowTranslationTable", "pauseMenuButton"));
        pauseMenuButton.RegisterCallback<ClickEvent>(PauseBtnClicked);

        statsMenuButton = root.Q<Button>("statsMenuButton");
        statsMenuButton.SetBinding("text", new LocalizedString("ActionRowTranslationTable", "statsMenuButton"));
        statsMenuButton.RegisterCallback<ClickEvent>(StatsBtnClicked);

        FillActionRow();
    }

    void FillActionRow()
    {

        foreach (var turret in turretsInGame)
        {
            Debug.Log($"{turret.turretName}");
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
}
