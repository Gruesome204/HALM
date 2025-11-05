using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuBehavior : MonoBehaviour
{
    private Button backButton;

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

        GameManager.Instance.ChangeState(GameManager.GameState.Paused);
        root.Q<VisualElement>("mainContainer").RemoveFromClassList("pauseMenuSlideOut");
    }

    private void OnDisable()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);
        InGameMenuManager.Instance.ReturnToGame();
    }
    void OnBackBtnCicked(ClickEvent evt)
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<VisualElement>("mainContainer").AddToClassList("pauseMenuSlideOut");

        this.gameObject.SetActive(false);
    }
    void OnResumeBtnCicked(ClickEvent evt)
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<VisualElement>("mainContainer").AddToClassList("pauseMenuSlideOut");

        this.gameObject.SetActive(false);
    }
    void OnSettingsBtnCicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.OpenOneInGameMenu(4);
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
