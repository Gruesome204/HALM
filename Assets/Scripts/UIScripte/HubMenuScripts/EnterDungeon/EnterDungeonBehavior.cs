using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class EnterDungeonBehavior : MonoBehaviour, IMenu
{
    private Label headline;

    private Button enterDungeonButton;
    private Button stayInHubButton;

    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the SettingsMenu and adds it to openMenu List
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("settingsMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);

            //Play a Click sound to give audio feedback to the Player
            SoundManager.Instance.PlayWoodMenuOpen();

            //This ensures that the picture in the back can grow as large as possible while keeping it's correct scale :D
            //It can only be run when the Menu is actually on screen, otherwise the numbers go hairwire O.o
            // I wanted to run this once in the Enable Method, but due to the above that didn't work :C
            //I'm leaving it like this for now, it works fine so just close your eyes and look elswhere Lukas XD
            var container = root.Q<VisualElement>("container");
            container.style.width = (int)((int)container.resolvedStyle.height / (float)1.485);
        }
        else
        {
            //Closes SettingsMenu and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("settingsMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
        }
    }

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("EnterDungeonTranslationTable", "headline"));

        enterDungeonButton = root.Q<Button>("enterDungeonButton");
        enterDungeonButton.SetBinding("text", new LocalizedString("EnterDungeonTranslationTable", "enterDungeonButton"));
        enterDungeonButton.RegisterCallback<ClickEvent>(OnEnterDungeonBtnClicked);

        stayInHubButton = root.Q<Button>("stayInHubButton");
        stayInHubButton.SetBinding("text", new LocalizedString("EnterDungeonTranslationTable", "stayInHubButton"));
        stayInHubButton.RegisterCallback<ClickEvent>(OnStayInHubBtnClicked);
    }

    void OnEnterDungeonBtnClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayWoodClick();

        SceneManager.LoadScene("GameScene");
        GameManager.Instance.SaveGame();
    }

    void OnStayInHubBtnClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        SoundManager.Instance.PlayWoodClick();

        InGameMenuManager.Instance.ReturnToGame();
        GameManager.Instance.SaveGame();
    }
}
