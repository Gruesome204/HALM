using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuBehavior : MonoBehaviour, IMenu
{
    private Button statsMenuButton;

    private Label pauseMenuHeadline;
    private Button resumeButton;
    private Button settingsButton;
    private Button cancelRunButton;
    private Button mainMenuButton;
    private Button exitButton;

    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            InGameMenuManager.Instance.CloseAllMenus();
            //Open the Pause Menu, adds to openMenu List and sets Game to paused
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("pauseMenuSlideOut");
            GameManager.Instance.ChangeState(GameManager.GameState.Paused);
        }
        else
        {
            //Closes Pause Menu and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("pauseMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
        }
    }
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        //Connecting and Setting Pause Menu Headline
        pauseMenuHeadline = root.Q<Label>("pauseMenuHeadline");
        pauseMenuHeadline.SetBinding("text", new LocalizedString("PauseMenuTranslationTable", "pauseMenuHeadline"));

        //Connecting all Buttons, Settings their texts and their functions

        resumeButton = root.Q<Button>("resumeButton");
        resumeButton.SetBinding("text", new LocalizedString("PauseMenuTranslationTable", "resumeButton"));
        resumeButton.RegisterCallback<ClickEvent>(OnResumeBtnCicked);

        statsMenuButton = root.Q<Button>("statsMenuButton");
        statsMenuButton.SetBinding("text", new LocalizedString("StatsMenuTranslationTable", "statsMenuHeadline"));
        statsMenuButton.RegisterCallback<ClickEvent>(OnStatsMenuButtonClicked);

        settingsButton = root.Q<Button>("settingsButton");
        settingsButton.SetBinding("text", new LocalizedString("MenuTranslationaTable", "settingsButton"));
        settingsButton.RegisterCallback<ClickEvent>(OnSettingsBtnCicked);

        cancelRunButton = root.Q<Button>("cancelRunButton");
        if (SceneManager.GetActiveScene().name != "HubScene")
        {
            cancelRunButton.SetBinding("text", new LocalizedString("PauseMenuTranslationTable", "cancelRunButton"));
            cancelRunButton.RegisterCallback<ClickEvent>(OnCancelRunBtnCicked);
        }
        else
        {
            root.Q<VisualElement>("buttonContainer").Remove(cancelRunButton);
        }

        mainMenuButton = root.Q<Button>("mainMenuButton");
        mainMenuButton.SetBinding("text", new LocalizedString("PauseMenuTranslationTable", "mainMenuButton"));
        mainMenuButton.RegisterCallback<ClickEvent>(OnMainMenuBtnCicked);

        exitButton = root.Q<Button>("exitButton");
        exitButton.SetBinding("text", new LocalizedString("MenuTranslationaTable", "exitButton"));
        exitButton.RegisterCallback<ClickEvent>(OnExitBtnCicked);        
    }

    void OnResumeBtnCicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.ReturnToGame();
    }
    void OnStatsMenuButtonClicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.CloseAllMenus();
        InGameMenuManager.Instance.OpenOrCloseOneMenu("StatsMenuDoc", true);
    }
    void OnSettingsBtnCicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.OpenOrCloseOneMenu("SettingsMenuDoc", true);
    }
    void OnCancelRunBtnCicked(ClickEvent evt)
    {
        SceneManager.LoadScene("HubScene");
    }
    void OnMainMenuBtnCicked(ClickEvent evt)
    {
        SceneManager.LoadScene("MainMenu");
    }
    void OnExitBtnCicked(ClickEvent evt)
    {
        Application.Quit();
    }
}
