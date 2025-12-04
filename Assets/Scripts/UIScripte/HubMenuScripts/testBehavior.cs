using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class testBehavior : MonoBehaviour, IMenu
{
    private Button testBtn;

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

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        testBtn = root.Q<Button>("testBtn");
        //testBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "backBtnText"));
        testBtn.RegisterCallback<ClickEvent>(OnTestButtonClicked);
    }

    void OnTestButtonClicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.OpenOrCloseOneMenu("TestDoc", false);
    }
}
