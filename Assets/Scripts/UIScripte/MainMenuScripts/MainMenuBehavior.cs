using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuBehavior : MonoBehaviour
{
    private Label mainMenuHeadline;

    private VisualElement btnContainer;
    private VisualElement certainContainer;

    private Button playBtn;
    private Button resetGameBtn;
    private Button settingsBtn;
    private Button creditsBtn;
    private Button exitBtn;

    private Label headline;
    private Button resetButton;
    private Button noResetButton;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        mainMenuHeadline = root.Q<Label>("mainMenuHeadine");
        mainMenuHeadline.SetBinding("text", new LocalizedString("MenuTranslationaTable", "mainMenuHeadline"));

        btnContainer = root.Q<VisualElement>("btnContainer");

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


        certainContainer = root.Q<VisualElement>("certainContainer");
        certainContainer.AddToClassList("vanish");

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("MenuTranslationaTable", "headline"));

        resetButton = root.Q<Button>("resetButton");
        resetButton.SetBinding("text", new LocalizedString("MenuTranslationaTable", "resetButton"));
        resetButton.RegisterCallback<ClickEvent>(ResetGame);

        noResetButton = root.Q<Button>("noResetButton");
        noResetButton.SetBinding("text", new LocalizedString("MenuTranslationaTable", "noResetButton"));
        noResetButton.RegisterCallback<ClickEvent>(NoResetGame);
    }


    void OnPlayBtnClicked(ClickEvent clicked)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("stone");

        SceneManager.LoadScene("HubScene");
    }

    void OnSettingsBtnClicked(ClickEvent clicked)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("stone");

        InGameMenuManager.Instance.OpenOrCloseOneMenu("SettingsMenuDoc", true);
    }
    void OnCreditsBtnClicked(ClickEvent clicked)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("stone");

        InGameMenuManager.Instance.OpenOrCloseOneMenu("CreditsMenuDoc", true);
    }
    void OnExitBtnClicked(ClickEvent clicked)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("stone");

        Application.Quit();
    }

    void OnResetGameBtnClicked(ClickEvent clicked)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("stone");

        btnContainer.AddToClassList("vanish");
        certainContainer.RemoveFromClassList("vanish");
    }

    void ResetGame(ClickEvent clicked)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("stone");

        GameManager.Instance.ResetGame();

        btnContainer.RemoveFromClassList("vanish");
        certainContainer.AddToClassList("vanish");
    }

    void NoResetGame(ClickEvent clicked)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("stone");

        btnContainer.RemoveFromClassList("vanish");
        certainContainer.AddToClassList("vanish");
    }
}
