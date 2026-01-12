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
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);

            //This ensures that the picture in the back can grow as large as possible while keeping it's correct scale :D
            //It can only be run when the Menu is actually on screen, otherwise the numbers go hairwire O.o
            // I wanted to run this once in the Enable Method, but due to the above that didn't work :C
            //I'm leaving it like this for now, it works fine so just close your eyes and look elswhere Lukas XD
            var container = root.Q<VisualElement>("mainContainer");
            container.style.width = (int)((int)container.resolvedStyle.height / (float)1.31);
        }
        else
        {
            //Closes Pause Menu and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("pauseMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
        }
        GameManager.Instance.SaveGame();
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

        if (SceneManager.GetActiveScene().name != "HubScene")
        {
            statsMenuButton.SetBinding("text", new LocalizedString("StatsMenuTranslationTable", "statsMenuHeadline"));
            statsMenuButton.RegisterCallback<ClickEvent>(OnStatsMenuButtonClicked);
        }
        else
        {
            statsMenuButton.AddToClassList("vanish");
        }

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
            cancelRunButton.AddToClassList("vanish");
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
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("paper");

        InGameMenuManager.Instance.ReturnToGame();
    }
    void OnStatsMenuButtonClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("paper");

        InGameMenuManager.Instance.CloseAllMenus();
        InGameMenuManager.Instance.OpenOrCloseOneMenu("StatsMenuDoc", true);
    }
    void OnSettingsBtnCicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("paper");

        InGameMenuManager.Instance.OpenOrCloseOneMenu("SettingsMenuDoc", true);
    }
    void OnCancelRunBtnCicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("paper");

        SceneManager.LoadScene("HubScene");
    }
    void OnMainMenuBtnCicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("paper");

        SceneManager.LoadScene("MainMenu");
    }
    void OnExitBtnCicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("paper");

        Application.Quit();
    }
}
