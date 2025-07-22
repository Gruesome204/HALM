using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class SettingsMenuBehavior : MonoBehaviour
{
    private MenuManager menuManager;
    [SerializeField] SettingDataSO settingDataSO;

    private Button backBtn;
    private Label headline;

    private Label subHeadlineLanguages;
    private Button languageEnglishBtn;
    private Button languageGermanBtn;
    private Button languageDeveloperBtn;

    private Label soundSettingsHeadline;
    private SliderInt musicTrackSlider;
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



    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        menuManager = FindObjectOfType<MenuManager>();

        var root = GetComponent<UIDocument>().rootVisualElement;


        //Connecting the General Stuff
        backBtn = root.Q<Button>("backBtn");
        backBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "backBtnText"));
        backBtn.RegisterCallback<ClickEvent>(OnBackBtnClicked);

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("MenuTranslationaTable", "settingsHeadline"));


        //Connecting the language Stuff
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


        // Connecting the Sound Stuff
        soundSettingsHeadline = root.Q<Label>("soundSettingsHeadline");
        soundSettingsHeadline.SetBinding("text", new LocalizedString("MenuTranslationaTable", "soundSettingsHeadline"));

        musicTrackSlider = root.Q<SliderInt>("musicTrackSlider");
        musicTrackSlider.SetBinding("text", new LocalizedString("MenuTranslationaTable", "musicTrackSlider"));
        musicTrackSlider.value = settingDataSO.musicTrack;

        musicVolumeSlider = root.Q<Slider>("musicVolumeSlider");
        musicVolumeSlider.SetBinding("text", new LocalizedString("MenuTranslationaTable", "musicVolumeSlider"));
        musicVolumeSlider.value = settingDataSO.musicVolume;

        soundVolumeSlider = root.Q<Slider>("soundVolumeSlider");
        soundVolumeSlider.SetBinding("text", new LocalizedString("MenuTranslationaTable", "soundVolumeSlider"));
        soundVolumeSlider.value = settingDataSO.soundVolume;
    }

    private void Update()
    {
        SettingVolume();
    }

    void OnBackBtnClicked(ClickEvent clicked)
    {
        this.gameObject.SetActive(false);
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
        if (musicVolumeSlider.value != settingDataSO.musicVolume)
        {
            settingDataSO.musicVolume = musicVolumeSlider.value;
            Debug.Log($"{settingDataSO.musicVolume}");
            SetAllMusicVolume(musicVolumeSlider.value);
        }

        if (soundVolumeSlider.value != settingDataSO.soundVolume)
        {
            settingDataSO.soundVolume = soundVolumeSlider.value;
            Debug.Log($"{settingDataSO.soundVolume}");
        }
        if (musicTrackSlider.value != settingDataSO.musicTrack)
        {
            settingDataSO.musicTrack = musicTrackSlider.value;
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
                break;

            case 2:
                BGMusicTrack_2.Play();
                break;

            case 3:
                BGMusicTrack_3.Play();
                break;

            case 4:
                BGMusicTrack_4.Play();
                break;

            case 5:
                BGMusicTrack_5.Play();
                break;

            case 6:
                BGMusicTrack_6.Play();
                break;

            case 7:
                BGMusicTrack_7.Play();
                break;

            case 8:
                BGMusicTrack_8.Play();
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
