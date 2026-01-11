using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class CreditsMenuBehavior : MonoBehaviour, IMenu
{
    private Button backBtn;
    private Label creditsHeadline;

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
        var root = GetComponent<UIDocument>().rootVisualElement;

        backBtn = root.Q<Button>("backBtn");
        backBtn.RegisterCallback<ClickEvent>(OnBackBtnClicked);

        creditsHeadline = root.Q<Label>("headline");
        creditsHeadline.SetBinding("text", new LocalizedString("CreditsMenuTranslationaTable", "headline"));
    }

    void OnBackBtnClicked(ClickEvent clicked)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("stone");

        InGameMenuManager.Instance.OpenOrCloseOneMenu("CreditsMenuDoc", false);
    }
}
