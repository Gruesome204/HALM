using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class RessourceRowBehavior : MonoBehaviour,IMenu
{
    private VisualElement resourceContainer;
    private Button pauseMenuButton;

    public VisualTreeAsset resourceElementAsset;

    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the ActionRow and sets Game to playing
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("actionRowSlideOut");
            GameManager.Instance.ChangeState(GameManager.GameState.HubMenu);
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

        resourceContainer = root.Q<VisualElement>("resourceContainer");

        pauseMenuButton = root.Q<Button>("pauseMenuButton");
        pauseMenuButton.SetBinding("text", new LocalizedString("ActionRowTranslationTable", "pauseMenuButton"));
        pauseMenuButton.RegisterCallback<ClickEvent>(PauseBtnClicked);

        FillResourceRow();
    }

    void PauseBtnClicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.CloseAllMenus();
        InGameMenuManager.Instance.OpenOrCloseOneMenu("PauseMenuDoc", true);
    }

    void FillResourceRow()
    {
        CreateRessourceRowElement(GameManager.Instance.gameDataSO.gameCurrency, "CurrencyIcon");
        CreateRessourceRowElement(GameManager.Instance.gameDataSO.woodResource, "WoodIcon");
        CreateRessourceRowElement(GameManager.Instance.gameDataSO.steinResource, "StoneIcon");
        CreateRessourceRowElement(GameManager.Instance.gameDataSO.metallResource, "MetallIcon");
        CreateRessourceRowElement(GameManager.Instance.gameDataSO.pulverResource, "BlackpowderIcon");
    }

    private void CreateRessourceRowElement(int amountOfRessource, string ressourceIcon)
    {
        AR_ResourceBehavior aR_Resource = new AR_ResourceBehavior(resourceElementAsset, amountOfRessource, ressourceIcon);
        resourceContainer.Add(aR_Resource.border);

        Debug.Log("Kill me jow");
    }
}
