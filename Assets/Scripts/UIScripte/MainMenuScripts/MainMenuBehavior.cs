using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuBehavior : MonoBehaviour
{
    private Label mainMenuHeadline;

    private Button playBtn;
    private Button resetGameBtn;
    private Button settingsBtn;
    private Button creditsBtn;
    private Button exitBtn;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        mainMenuHeadline = root.Q<Label>("mainMenuHeadine");
        mainMenuHeadline.SetBinding("text", new LocalizedString("MenuTranslationaTable", "mainMenuHeadline"));


        playBtn = root.Q<Button>("playBtn");
        playBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "playBtn"));
        playBtn.RegisterCallback<ClickEvent>(OnPlayBtnClicked);

        resetGameBtn = root.Q<Button>("resetGameBtn");
        resetGameBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "resetGameBtn"));
        resetGameBtn.RegisterCallback<ClickEvent>(OnResetGameBtnClicked);

        settingsBtn = root.Q<Button>("settingsBtn");
        settingsBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "settingsButton"));
        settingsBtn.RegisterCallback<ClickEvent>(OnSettingsBtnClicked);

        creditsBtn = root.Q<Button>("creditsBtn");
        creditsBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "creditsBtn"));
        creditsBtn.RegisterCallback<ClickEvent>(OnCreditsBtnClicked);

        exitBtn = root.Q<Button>("exitBtn");
        exitBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "exitButton"));
        exitBtn.RegisterCallback<ClickEvent>(OnExitBtnClicked);
    }


    void OnPlayBtnClicked(ClickEvent clicked)
    {
        SceneManager.LoadScene("HubScene");
    }
    void OnResetGameBtnClicked(ClickEvent clicked)
    {
        GameManager.Instance.ResetGame();
    }
    void OnSettingsBtnClicked(ClickEvent clicked)
    {
        InGameMenuManager.Instance.OpenOrCloseOneMenu("SettingsMenuDoc", true);
    }
    void OnCreditsBtnClicked(ClickEvent clicked)
    {
        InGameMenuManager.Instance.OpenOrCloseOneMenu("CreditsMenuDoc", true);
    }
    void OnExitBtnClicked(ClickEvent clicked)
    {
        Application.Quit();
    }
}
