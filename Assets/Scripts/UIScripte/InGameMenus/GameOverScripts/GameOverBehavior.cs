using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class GameOverBehavior : MonoBehaviour
{
    private Label headline;

    private Button retryLevelButton;
    private Button returnToHubButton;
    private Button returnToMainMenuButton;
    private Button exitButton;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("GameOverTranslationaTable", "headline"));

        retryLevelButton = root.Q<Button>("retryLevelButton");
        retryLevelButton.SetBinding("text", new LocalizedString("GameOverTranslationaTable", "retryLevelButton"));

        returnToHubButton = root.Q<Button>("returnToHubButton");
        returnToHubButton.SetBinding("text", new LocalizedString("GameOverTranslationaTable", "returnToHubButton"));

        returnToMainMenuButton = root.Q<Button>("returnToMainMenuButton");
        returnToMainMenuButton.SetBinding("text", new LocalizedString("GameOverTranslationaTable", "returnToMainMenuButton"));

        exitButton = root.Q<Button>("exitButton");
        exitButton.SetBinding("text", new LocalizedString("GameOverTranslationaTable", "exitButton"));

        this.gameObject.SetActive(false);
    }
}
