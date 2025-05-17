using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuBehavior : MonoBehaviour
{
    private MenuManager menuManager;

    private Label mainMenuHeadline;

    private Button playBtn;
    private Button settingsBtn;
    private Button creditsBtn;
    private Button exitBtn;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        menuManager = FindObjectOfType<MenuManager>();

        mainMenuHeadline = root.Q<Label>("mainMenuHeadine");
        mainMenuHeadline.SetBinding("text", new LocalizedString("MenuTranslationaTable", "mainMenuHeadline"));


        playBtn = root.Q<Button>("playBtn");
        playBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "playBtn"));
        playBtn.RegisterCallback<ClickEvent>(OnPlayBtnClicked);

        settingsBtn = root.Q<Button>("settingsBtn");
        settingsBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "settingsBtn"));
        settingsBtn.RegisterCallback<ClickEvent>(OnSettingsBtnClicked);

        creditsBtn = root.Q<Button>("creditsBtn");
        creditsBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "creditsBtn"));
        creditsBtn.RegisterCallback<ClickEvent>(OnCreditsBtnClicked);

        exitBtn = root.Q<Button>("exitBtn");
        exitBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "exitBtn"));
        exitBtn.RegisterCallback<ClickEvent>(OnExitBtnClicked);
    }


    void OnPlayBtnClicked(ClickEvent clicked)
    {
        SceneManager.LoadScene("");
    }
    void OnSettingsBtnClicked(ClickEvent clicked)
    {
        menuManager.EnterSettingsMenu();
    }
    void OnCreditsBtnClicked(ClickEvent clicked)
    {
        menuManager.EnterCreditsMenu();
    }
    void OnExitBtnClicked(ClickEvent clicked)
    {
        Application.Quit();
    }
}
