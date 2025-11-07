using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
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



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menuManager = FindObjectOfType<MenuManager>();
        ConnectingEverything();

        SetAllMusicVolume(settingDataSO.musicVolume * settingDataSO.masterVolume);
        PlayThisMusic(2);
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            this.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        ConnectingEverything();
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
        masterVolumeSlider.value = settingDataSO.masterVolume;

        musicVolumeSlider = root.Q<Slider>("musicVolumeSlider");
        musicVolumeSlider.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundSliderMusicVolume"));
        musicVolumeSlider.value = settingDataSO.musicVolume;

        soundVolumeSlider = root.Q<Slider>("soundVolumeSlider");
        soundVolumeSlider.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundSliderSoundVolume"));
        soundVolumeSlider.value = settingDataSO.soundVolume;

        musicTrackSlider = root.Q<SliderInt>("musicTrackSlider");
        musicTrackSlider.SetBinding("text", new LocalizedString("SettingsMenuTranslationTable", "soundSliderMusicTrack"));
        musicTrackSlider.value = settingDataSO.musicTrack;
    }

    private void Update()
    {
        SettingVolume();
    }
  
 


    void OnBackBtnClicked(ClickEvent clicked)
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            InGameMenuManager.Instance.OpenCloseOneMenu("SettingsMenuDoc", false);
        }
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
        if (masterVolumeSlider.value != settingDataSO.masterVolume)
        {
            settingDataSO.masterVolume = masterVolumeSlider.value;
            Debug.Log($"{settingDataSO.masterVolume}");
            SetAllMusicVolume(settingDataSO.musicVolume * settingDataSO.masterVolume);
        }

        if (musicVolumeSlider.value != settingDataSO.musicVolume)
        {
            settingDataSO.musicVolume = musicVolumeSlider.value;
            Debug.Log($"{settingDataSO.musicVolume}");
            SetAllMusicVolume(settingDataSO.musicVolume * settingDataSO.masterVolume);
        }

        if (soundVolumeSlider.value != settingDataSO.soundVolume)
        {
            settingDataSO.soundVolume = soundVolumeSlider.value;
            Debug.Log($"{settingDataSO.soundVolume * settingDataSO.masterVolume}");
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
                settingDataSO.musicTrack = 1;
                break;

            case 2:
                BGMusicTrack_2.Play();
                settingDataSO.musicTrack = 2;
                break;

            case 3:
                BGMusicTrack_3.Play();
                settingDataSO.musicTrack = 3;
                break;

            case 4:
                BGMusicTrack_4.Play();
                settingDataSO.musicTrack = 4;
                break;

            case 5:
                BGMusicTrack_5.Play();
                settingDataSO.musicTrack = 5;
                break;

            case 6:
                BGMusicTrack_6.Play();
                settingDataSO.musicTrack = 6;
                break;

            case 7:
                BGMusicTrack_7.Play();
                settingDataSO.musicTrack = 7;
                break;

            case 8:
                BGMusicTrack_8.Play();
                settingDataSO.musicTrack = 8;
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
