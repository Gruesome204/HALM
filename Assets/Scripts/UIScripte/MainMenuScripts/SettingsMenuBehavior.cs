using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class SettingsMenuBehavior : MonoBehaviour, IMenu
{
    [SerializeField] private GameDataSO gameDataSO;

    private Button backBtn;
    private Label headline;

    private Label subHeadlineLanguages;
    private Button languageEnglishBtn;
    private Button languageGermanBtn;
    private Button languageDeveloperBtn;

    private Label soundSettingsHeadline;
    private SliderInt musicTrackSlider;
    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Slider soundVolumeSlider;

    private Label controlHeadline;
    private Label controlText;

    [Header("Audio Tracks")]
    public List<AudioSource> AllMusicTracks = new List<AudioSource>();

    #region Menu Open/Close
    public void OpenOrClose(bool open)
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("settingsMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);

            //Play a Click sound to give audio feedback to the Player
            SoundManager.Instance.PlayStoneMenuOpen();
        }
        else
        {
            root.Q<VisualElement>("mainContainer").AddToClassList("settingsMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
            GameManager.Instance.SaveGame();
        }
    }
    #endregion

    private void OnEnable()
    {
       StartCoroutine(WaitForSaveLoad());
    }
    private IEnumerator WaitForSaveLoad()
    {
        // Wait until GameManager has loaded the save
        yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.IsSaveLoaded);
        {
            gameDataSO = GameManager.Instance.gameDataSO;
            ConnectUI();
            ApplySavedSettings();
            RegisterSliderCallbacks();

        }
    }

    #region UI Setup
    private void ConnectUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // General
        backBtn = root.Q<Button>("backBtn");
        backBtn.RegisterCallback<ClickEvent>(OnBackBtnClicked);

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "headline"));

        // Language
        subHeadlineLanguages = root.Q<Label>("languageSettingsHeadline");
        subHeadlineLanguages.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "languageHeadline"));

        languageEnglishBtn = root.Q<Button>("englishBtn");
        languageEnglishBtn.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "languageBtnEnglish"));
        languageEnglishBtn.RegisterCallback<ClickEvent>(evt => SetNewLocale("en"));

        languageGermanBtn = root.Q<Button>("germanBtn");
        languageGermanBtn.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "languageBtnDeutsch"));
        languageGermanBtn.RegisterCallback<ClickEvent>(evt => SetNewLocale("de"));

        languageDeveloperBtn = root.Q<Button>("developerBtn");
        languageDeveloperBtn.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "languageBtnDeveloper"));
        languageDeveloperBtn.RegisterCallback<ClickEvent>(evt => SetNewLocale("dev"));

        // Sound
        soundSettingsHeadline = root.Q<Label>("soundSettingsHeadline");
        soundSettingsHeadline.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundHeadline"));

        masterVolumeSlider = root.Q<Slider>("masterVolumeSlider");
        masterVolumeSlider.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundSliderMasterVolume"));

        musicVolumeSlider = root.Q<Slider>("musicVolumeSlider");
        musicVolumeSlider.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundSliderMusicVolume"));

        soundVolumeSlider = root.Q<Slider>("soundVolumeSlider");
        soundVolumeSlider.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundSliderSoundVolume"));

        musicTrackSlider = root.Q<SliderInt>("musicTrackSlider");
        musicTrackSlider.AddToClassList("vanish");
        //musicTrackSlider.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundSliderMusicTrack"));
        //musicTrackSlider.highValue = AllMusicTracks.Count - 1;

        //Control
        controlHeadline = root.Q<Label>("controlHeadline");
        controlHeadline.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "controlHeadline"));

        controlText = root.Q<Label>("controlText");
        controlText.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "controlText"));
    }
    #endregion

    #region Apply Saved Settings
    private void ApplySavedSettings()
    {
        masterVolumeSlider.value = gameDataSO.masterVolume;
        musicVolumeSlider.value = gameDataSO.musicVolume;
        soundVolumeSlider.value = gameDataSO.soundVolume;
        //musicTrackSlider.value = gameDataSO.musicTrack;

        SetAllMusicVolume(gameDataSO.masterVolume * gameDataSO.musicVolume);
        PlayThisMusic(0);
    }
    #endregion

    #region Slider Callbacks
    private void RegisterSliderCallbacks()
    {
        masterVolumeSlider.RegisterValueChangedCallback(evt =>
        {
            gameDataSO.masterVolume = evt.newValue;
            SetAllMusicVolume(gameDataSO.masterVolume * gameDataSO.musicVolume);
        });

        musicVolumeSlider.RegisterValueChangedCallback(evt =>
        {
            gameDataSO.musicVolume = evt.newValue;
            SetAllMusicVolume(gameDataSO.masterVolume * gameDataSO.musicVolume);
        });

        soundVolumeSlider.RegisterValueChangedCallback(evt =>
        {
            gameDataSO.soundVolume = evt.newValue;
        });

        //musicTrackSlider.RegisterValueChangedCallback(evt =>
        //{
        //    gameDataSO.musicTrack = evt.newValue;
        //    PlayThisMusic(evt.newValue);
        //});
    }
    #endregion

    #region Audio Control
    private void SetAllMusicVolume(float volume)
    {
        foreach (var track in AllMusicTracks)
            track.volume = volume;
    }

    private void PlayThisMusic(int index)
    {
        if (index < 0 || index >= AllMusicTracks.Count)
            index = 0;

        foreach (var track in AllMusicTracks)
            track.Stop();

        AllMusicTracks[index]?.Play();
    }
    #endregion

    #region Language
    private void SetNewLocale(string code)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayStoneClick();

        switch (code)
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
    #endregion

    private void OnBackBtnClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayStoneClick();

        InGameMenuManager.Instance.OpenOrCloseOneMenu("SettingsMenuDoc", false);
    }
}
