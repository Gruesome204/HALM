using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameWonBehavior : MonoBehaviour, IMenu
{
    private Label headline;

    private Button returnToHubButton;
    private Button returnToMainMenuButton;
    private Button exitButton;

    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the GameWonMenu, pauses the Game and adds it to openMenu List
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("settingsMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);
            GameManager.Instance.ChangeState(GameManager.GameState.Paused);

            //Play a Click sound to give audio feedback to the Player
            SoundManager.Instance.PlayStoneMenuOpen();
        }
        else
        {
            //Closes GameWonMenu and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("settingsMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
        }
        SaveManager.Instance.SaveGame();
    }
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("GameWonTranslationaTable", "headline"));

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

    void OnReturnToHubButtonClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayStoneClick();

        SceneManager.LoadScene("HubScene");
        GameManager.Instance.ChangeState(GameManager.GameState.HubMenu);
    }
    void OnReturnToMainMenuButtonClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayStoneClick();

        SceneManager.LoadScene("MainMenu");
        GameManager.Instance.ChangeState(GameManager.GameState.MainMenu);
    }
    void OnExitButtonClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayStoneClick();

        Application.Quit();
    }
}
