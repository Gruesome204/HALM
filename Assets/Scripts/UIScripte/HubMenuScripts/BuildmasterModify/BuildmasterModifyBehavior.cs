using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class BuildmasterModifyBehavior : MonoBehaviour, IMenu
{
    public Label headline;
    public VisualElement btnContainer;
    public Button backBtn;

    public VisualTreeAsset modifierButtonAsset;


    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the SettingsMenu and adds it to openMenu List
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);
            FillMenu();
        }
        else
        {
            //Closes SettingsMenu and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
            ClearMenu();
        }
    }

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("ActionRowTranslationTable", "pauseMenuButton"));

        btnContainer = root.Q<VisualElement>("btnContainer");

        backBtn = root.Q<Button>("backBtn");
        backBtn.SetBinding("text", new LocalizedString("ActionRowTranslationTable", "pauseMenuButton"));
        backBtn.RegisterCallback<ClickEvent>(OnBackBtnClicked);
    }


    private void FillMenu()
    {
        Debug.Log("FillingMenu");
        //foreach (var _modifier in BuildmasterModifyManager.Instance.GetBuildmasterModifiers())
        //{
        //    BM_ModifierButtonBehavior _modifierButton = new BM_ModifierButtonBehavior(modifierButtonAsset, _modifier);
        //    btnContainer.Add(_modifierButton.mainContainer);
        //}
    }
    private void ClearMenu()
    {
        btnContainer.Clear();
    }

    private void OnBackBtnClicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.OpenOrCloseOneMenu("BuildmasterModifyDoc", false);
    }
}
