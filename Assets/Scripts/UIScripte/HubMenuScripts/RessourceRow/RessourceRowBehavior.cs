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

    private List<AR_ResourceBehavior> ressourceElementList = new List<AR_ResourceBehavior>();


    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the ActionRow and sets Game to playing
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("actionRowSlideOut");
            GameManager.Instance.ChangeState(GameManager.GameState.HubMenu);
            FillResourceRow();
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

        GameManager.Instance.OnRessourceChanged += RessourceAdded;
    }

    public void RessourceAdded(ResourceType _type, int change, int currentAmount)
    {
        foreach (var ressource in ressourceElementList)
        {
            if (ressource.GetRessourceType() == _type)
            {
                //do shit i guess
                ressource.CreateAddRessourceElement(ressourceAddedElement, change, _type);
                return;
            }
        }
    }
    void PauseBtnClicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.CloseAllMenus();
        InGameMenuManager.Instance.OpenOrCloseOneMenu("PauseMenuDoc", true);
    }

    void FillResourceRow()
    {
        resourceContainer.Clear();
        ressourceElementList.Clear();
        CreateRessourceRowElement(ResourceType.Currency);
        CreateRessourceRowElement(ResourceType.Wood);
        CreateRessourceRowElement(ResourceType.Stone);
        CreateRessourceRowElement(ResourceType.Metal);
        CreateRessourceRowElement(ResourceType.Pulver);

    }

    private void CreateRessourceRowElement(ResourceType _type)
    {
        AR_ResourceBehavior aR_Resource = new AR_ResourceBehavior(resourceElementAsset, _type);
        ressourceElementList.Add(aR_Resource);
        resourceContainer.Add(aR_Resource.border);
    }
}
