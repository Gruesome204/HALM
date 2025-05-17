using System.Collections;
using UnityEditor.Localization.Editor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class SettingsMenuBehavior : MonoBehaviour
{
    private MenuManager menuManager;


    private Button backBtn;

    private Label headline;

    private Label subHeadlineLanguages;
    private Button languageEnglishBtn;
    private Button languageGermanBtn;
    private Button languageDeveloperBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        menuManager = FindObjectOfType<MenuManager>();

        var root = GetComponent<UIDocument>().rootVisualElement;

        backBtn = root.Q<Button>("backBtn");
        backBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "backBtnText"));
        backBtn.RegisterCallback<ClickEvent>(OnBackBtnClicked);

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("MenuTranslationaTable", "settingsHeadline"));


        subHeadlineLanguages = root.Q<Label>("languageSettingsHeadline");
        subHeadlineLanguages.SetBinding("text", new LocalizedString("MenuTranslationaTable", "settingsLanguageHeadline"));

        languageEnglishBtn = root.Q<Button>("englishBtn");
        languageEnglishBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "languageSettimgsEnglischBtn"));
        languageEnglishBtn.RegisterCallback<ClickEvent>(OnLanguageEnglishBtnClicked);

        languageGermanBtn = root.Q<Button>("germanBtn");
        languageGermanBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "languageSettingsDeutschBtn"));
        languageGermanBtn.RegisterCallback<ClickEvent>(OnLanguageGermanBtnClicked);

        languageDeveloperBtn = root.Q<Button>("developerBtn");
        languageDeveloperBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "languageSettingsDeveloperBtn"));
        languageDeveloperBtn.RegisterCallback<ClickEvent>(OnLanguageDeveloperBtnClicked);

    }

    void OnBackBtnClicked(ClickEvent clicked)
    {
        menuManager.EnterMainMenu();
    }

    void OnLanguageEnglishBtnClicked(ClickEvent clicked)
    {
        SetNewLocale("en");
    }
    void OnLanguageGermanBtnClicked(ClickEvent clicked)
    {
        SetNewLocale("de");
    }
    void OnLanguageDeveloperBtnClicked(ClickEvent clicked)
    {
        SetNewLocale("dev");
    }

    void SetNewLocale(string desiredLocale)
    {
        switch (desiredLocale)
        {
            case "en":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
                break;

            case "de":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
                break;

            case "dev":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[2];
                break;
        }
            
    }
}
