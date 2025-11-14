using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameOverBehavior : MonoBehaviour, IMenu
{
    private Label headline;

    private Button retryLevelButton;
    private Button returnToHubButton;
    private Button returnToMainMenuButton;
    private Button exitButton;

    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the GameOverMenu, pauses the Game and adds it to openMenu List
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("settingsMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);
            GameManager.Instance.ChangeState(GameManager.GameState.Paused);
        }
        else
        {
            //Closes GameOverMenu and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("settingsMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
        }
    }
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("GameOverTranslationaTable", "headline"));

        retryLevelButton = root.Q<Button>("retryLevelButton");
        retryLevelButton.SetBinding("text", new LocalizedString("GameOverTranslationaTable", "retryLevelButton"));
        retryLevelButton.RegisterCallback<ClickEvent>(OnRetryLevelButtonClicked);

        returnToHubButton = root.Q<Button>("returnToHubButton");
        returnToHubButton.SetBinding("text", new LocalizedString("GameOverTranslationaTable", "returnToHubButton"));
        returnToHubButton.RegisterCallback<ClickEvent>(OnReturnToHubButtonClicked);

        returnToMainMenuButton = root.Q<Button>("returnToMainMenuButton");
        returnToMainMenuButton.SetBinding("text", new LocalizedString("GameOverTranslationaTable", "returnToMainMenuButton"));
        returnToMainMenuButton.RegisterCallback<ClickEvent>(OnReturnToMainMenuButtonClicked);

        exitButton = root.Q<Button>("exitButton");
        exitButton.SetBinding("text", new LocalizedString("GameOverTranslationaTable", "exitButton"));
        exitButton.RegisterCallback<ClickEvent>(OnExitButtonClicked);

    }

    void OnRetryLevelButtonClicked(ClickEvent evt)
    {
        SceneManager.LoadScene($"{SceneManager.GetActiveScene().name}");
    }
    void OnReturnToHubButtonClicked(ClickEvent evt)
    {
        SceneManager.LoadScene("HubScene");
    }
    void OnReturnToMainMenuButtonClicked(ClickEvent evt)
    {
        SceneManager.LoadScene("MainMenu");
    }
    void OnExitButtonClicked(ClickEvent evt)
    {
        Application.Quit();
    }
}
