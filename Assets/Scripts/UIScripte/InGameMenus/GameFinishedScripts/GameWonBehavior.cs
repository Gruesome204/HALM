using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameWonBehavior : MonoBehaviour
{
    private Label headline;

    private Button returnToHubButton;
    private Button returnToMainMenuButton;
    private Button exitButton;


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
