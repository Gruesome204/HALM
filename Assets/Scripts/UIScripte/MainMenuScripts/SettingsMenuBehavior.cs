using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SettingsMenuBehavior : MonoBehaviour, IMenu
{
    [SerializeField] GameDataSO gameDataSO;

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
    [SerializeField] AudioSource BGMusicTrack_1;
    [SerializeField] AudioSource BGMusicTrack_2;
    [SerializeField] AudioSource BGMusicTrack_3;
    [SerializeField] AudioSource BGMusicTrack_4;
    [SerializeField] AudioSource BGMusicTrack_5;
    [SerializeField] AudioSource BGMusicTrack_6;
    [SerializeField] AudioSource BGMusicTrack_7;
    [SerializeField] AudioSource BGMusicTrack_8;

    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the SettingsMenu and adds it to openMenu List
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("settingsMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);
        }
        else
        {
            //Closes SettingsMenu and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("settingsMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
        }
    }

    private void OnEnable()
    {
        ConnectingEverything();

        SetAllMusicVolume(gameDataSO.musicVolume * gameDataSO.masterVolume);
        PlayThisMusic(2);
    }

    void ConnectingEverything()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;


        //Connecting the General Stuff
        backBtn = root.Q<Button>("backBtn");
        backBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "backBtnText"));
        backBtn.RegisterCallback<ClickEvent>(OnBackBtnClicked);

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "headline"));


        //Connecting the language Stuff
        subHeadlineLanguages = root.Q<Label>("languageSettingsHeadline");
        subHeadlineLanguages.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "languageHeadline"));

        languageEnglishBtn = root.Q<Button>("englishBtn");
        languageEnglishBtn.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "languageBtnEnglish"));
        languageEnglishBtn.RegisterCallback<ClickEvent>(OnLanguageEnglishBtnClicked);

        languageGermanBtn = root.Q<Button>("germanBtn");
        languageGermanBtn.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "languageBtnDeutsch"));
        languageGermanBtn.RegisterCallback<ClickEvent>(OnLanguageGermanBtnClicked);

        languageDeveloperBtn = root.Q<Button>("developerBtn");
        languageDeveloperBtn.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "languageBtnDeveloper"));
        languageDeveloperBtn.RegisterCallback<ClickEvent>(OnLanguageDeveloperBtnClicked);


        // Connecting the Sound Stuff
        soundSettingsHeadline = root.Q<Label>("soundSettingsHeadline");
        soundSettingsHeadline.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundHeadline"));


        masterVolumeSlider = root.Q<Slider>("masterVolumeSlider");
        masterVolumeSlider.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundSliderMasterVolume"));
        masterVolumeSlider.value = gameDataSO.masterVolume;

        musicVolumeSlider = root.Q<Slider>("musicVolumeSlider");
        musicVolumeSlider.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundSliderMusicVolume"));
        musicVolumeSlider.value = gameDataSO.musicVolume;

        soundVolumeSlider = root.Q<Slider>("soundVolumeSlider");
        soundVolumeSlider.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundSliderSoundVolume"));
        soundVolumeSlider.value = gameDataSO.soundVolume;

        musicTrackSlider = root.Q<SliderInt>("musicTrackSlider");
        musicTrackSlider.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundSliderMusicTrack"));
        musicTrackSlider.value = gameDataSO.musicTrack;
    }

    private void Update()
    {
        SettingVolume();
    }

    void OnBackBtnClicked(ClickEvent clicked)
    {
        InGameMenuManager.Instance.OpenOrCloseOneMenu("SettingsMenuDoc", false);
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

    void SettingVolume()
    {
        if (masterVolumeSlider.value != gameDataSO.masterVolume)
        {
            gameDataSO.masterVolume = masterVolumeSlider.value;
            Debug.Log($"{gameDataSO.masterVolume}");
            SetAllMusicVolume(gameDataSO.musicVolume * gameDataSO.masterVolume);
        }

        if (musicVolumeSlider.value != gameDataSO.musicVolume)
        {
            gameDataSO.musicVolume = musicVolumeSlider.value;
            Debug.Log($"{gameDataSO.musicVolume}");
            SetAllMusicVolume(gameDataSO.musicVolume * gameDataSO.masterVolume);
        }

        if (soundVolumeSlider.value != gameDataSO.soundVolume)
        {
            gameDataSO.soundVolume = soundVolumeSlider.value;
            Debug.Log($"{gameDataSO.soundVolume * gameDataSO.masterVolume}");
        }

        if (musicTrackSlider.value != gameDataSO.musicTrack)
        {
            gameDataSO.musicTrack = musicTrackSlider.value;
            PlayThisMusic(musicTrackSlider.value);
        }
    }

    void StopAllMusic()
    {
        BGMusicTrack_1.Stop();
        BGMusicTrack_2.Stop();
        BGMusicTrack_3.Stop();
        BGMusicTrack_4.Stop();
        BGMusicTrack_5.Stop();
        BGMusicTrack_6.Stop();
        BGMusicTrack_7.Stop();
        BGMusicTrack_8.Stop();
    }

    void PlayThisMusic(int songToPlay)
    {
        StopAllMusic();

        switch (songToPlay)
        {
            case 1:
                BGMusicTrack_1.Play();
                gameDataSO.musicTrack = 1;
                break;

            case 2:
                BGMusicTrack_2.Play();
                gameDataSO.musicTrack = 2;
                break;

            case 3:
                BGMusicTrack_3.Play();
                gameDataSO.musicTrack = 3;
                break;

            case 4:
                BGMusicTrack_4.Play();
                gameDataSO.musicTrack = 4;
                break;

            case 5:
                BGMusicTrack_5.Play();
                gameDataSO.musicTrack = 5;
                break;

            case 6:
                BGMusicTrack_6.Play();
                gameDataSO.musicTrack = 6;
                break;

            case 7:
                BGMusicTrack_7.Play();
                gameDataSO.musicTrack = 7;
                break;

            case 8:
                BGMusicTrack_8.Play();
                gameDataSO.musicTrack = 8;
                break;
        }
    }

    void SetAllMusicVolume(float musicVolume)
    {
        BGMusicTrack_1.volume = musicVolume;
        BGMusicTrack_2.volume = musicVolume;
        BGMusicTrack_3.volume = musicVolume;
        BGMusicTrack_4.volume = musicVolume;
        BGMusicTrack_5.volume = musicVolume;
        BGMusicTrack_6.volume = musicVolume;
        BGMusicTrack_7.volume = musicVolume;
        BGMusicTrack_8.volume = musicVolume;
    }
}
