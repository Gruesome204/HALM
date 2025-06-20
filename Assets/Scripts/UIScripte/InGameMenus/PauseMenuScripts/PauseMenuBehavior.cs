using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuBehavior : MonoBehaviour
{
    private Button backButton;

    private Button resumeButton;
    private Button settingsButton;
    private Button cancelRunButton;
    private Button mainMenuButton;
    private Button exitButton;

    private InGameMenuManager gameMenuManager;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        gameMenuManager = FindObjectOfType<InGameMenuManager>();

        backButton = root.Q<Button>("backButton");
        backButton.SetBinding("text", new LocalizedString("MenuTranslationaTable", "backBtnText"));
        backButton.RegisterCallback<ClickEvent>(OnBackBtnCicked);

        resumeButton = root.Q<Button>("resumeButton");
        resumeButton.SetBinding("text", new LocalizedString("PauseMenuTranslationTable", "resumeButton"));
        resumeButton.RegisterCallback<ClickEvent>(OnResumeBtnCicked);

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

    void OnBackBtnCicked(ClickEvent evt)
    {
        gameMenuManager.ReturnToGame();
    }
    void OnResumeBtnCicked(ClickEvent evt)
    {
        gameMenuManager.ReturnToGame();
    }
    void OnSettingsBtnCicked(ClickEvent evt)
    {
        gameMenuManager.OpenOneInGameMenu(4);
    }
    void OnCancelRunBtnCicked(ClickEvent evt)
    {
        //SceneManager.LoadScene("GameScene");
        Debug.Log("Gibbet noch net");
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
