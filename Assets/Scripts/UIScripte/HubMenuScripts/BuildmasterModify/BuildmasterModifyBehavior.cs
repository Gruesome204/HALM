using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using static BuildMasterModifier;

public class BuildmasterModifyBehavior : MonoBehaviour, IMenu
{
    public Label headline;
    public Label subHeadline;
    public VisualElement btnContainer;
    public VisualElement appliedModifierContainer;
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
        headline.SetBinding("text", new LocalizedString("BuildmasterModifyTranslationTable", "headline"));

        subHeadline = root.Q<Label>("subHeadline");
        subHeadline.SetBinding("text", new LocalizedString("BuildmasterModifyTranslationTable", "subHeadline"));

        btnContainer = root.Q<VisualElement>("btnContainer");

        appliedModifierContainer = root.Q<VisualElement>("appliedModifierContainer");

        backBtn = root.Q<Button>("backBtn");
        backBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "backBtnText"));
        backBtn.RegisterCallback<ClickEvent>(OnBackBtnClicked);
    }


    private void FillMenu()
    {
        ClearMenu();
        foreach (var modifier in BuildmasterModifyManager.Instance.GetBuildmasterModifiers())
        {
            if (BuildmasterModifyManager.Instance.GetAppliedBuildmasterModifiers().Contains<BuildMasterModifier>(modifier))
            {

            }
            else
            {
                var btn = new BM_ModifierButtonBehavior(modifierButtonAsset, modifier);
                var uiDocument = this.gameObject.GetComponent<UIDocument>();
                uiDocument.rootVisualElement.Q("unity-content-container").Add(btn.mainContainer);
            }         
        }

        foreach (var _modifier in BuildmasterModifyManager.Instance.GetAppliedBuildmasterModifiers())
        {
            var btn = new BM_AppliedModifierBehavior(modifierButtonAsset, _modifier);
            appliedModifierContainer.Add(btn.mainContainer);
        }
    }
    private void ClearMenu()
    {
        var uiDocument = this.gameObject.GetComponent<UIDocument>();
        uiDocument.rootVisualElement.Q("unity-content-container").Clear();

        appliedModifierContainer.Clear();
    }

    private void OnBackBtnClicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.OpenOrCloseOneMenu("BuildmasterModifyDoc", false);
    }
}
