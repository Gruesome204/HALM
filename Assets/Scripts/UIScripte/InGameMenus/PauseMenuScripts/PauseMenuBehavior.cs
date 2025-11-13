using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuBehavior : MonoBehaviour
{
    private Button statsMenuButton;

    private Label pauseMenuHeadline;
    private Button resumeButton;
    private Button settingsButton;
    private Button cancelRunButton;
    private Button mainMenuButton;
    private Button exitButton;
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
        cancelRunButton.SetBinding("text", new LocalizedString("PauseMenuTranslationTable", "cancelRunButton"));
        cancelRunButton.RegisterCallback<ClickEvent>(OnCancelRunBtnCicked);

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
        InGameMenuManager.Instance.OpenCloseOneMenu("StatsMenuDoc", true);
    }
    void OnSettingsBtnCicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.OpenCloseOneMenu("SettingsMenuDoc", true);
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
