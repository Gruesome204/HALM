using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class RessourceRowBehavior : MonoBehaviour,IMenu
{
    private VisualElement resourceContainer;
    private Button pauseMenuButton;

    public VisualTreeAsset resourceElementAsset;
    public VisualTreeAsset ressourceAddedElement;


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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            GameManager.Instance.gameDataSO.AddResource(ResourceType.Currency, 10);
            GameManager.Instance.gameDataSO.AddResource(ResourceType.Wood, 10);
            GameManager.Instance.gameDataSO.AddResource(ResourceType.Stone, 10);
            GameManager.Instance.gameDataSO.AddResource(ResourceType.Metal, 10);
            GameManager.Instance.gameDataSO.AddResource(ResourceType.Pulver, 10);
        }
    }

    void PauseBtnClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayWoodClick();

        InGameMenuManager.Instance.CloseAllMenus();
        InGameMenuManager.Instance.OpenOrCloseOneMenu("PauseMenuDoc", true);
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
}
