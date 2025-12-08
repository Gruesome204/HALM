using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class BM_ModifierButtonBehavior
{
    public BuildMasterModifier representedModifier;

    public VisualElement mainContainer;
    private VisualElement icon;
    private Label name;
    private Label description;

    private Button button;

    public BM_ModifierButtonBehavior(VisualTreeAsset asset, BuildMasterModifier _representedModifier)
    {
        representedModifier = _representedModifier;

        TemplateContainer modifierButton = asset.Instantiate();

        mainContainer = modifierButton.Q<VisualElement>("mainContainer");
        icon = modifierButton.Q<VisualElement>("icon");

        name = modifierButton.Q<Label>("name");
        name.SetBinding("text", new LocalizedString($"BMTranslations{_representedModifier.name}", "name"));

        description = modifierButton.Q<Label>("description");
        description.SetBinding("text", new LocalizedString($"BMTranslations{_representedModifier.name}", "description"));

        button = modifierButton.Q<Button>("button");
        button.RegisterCallback<ClickEvent>(OnButtonClicked);
    }

    private void OnButtonClicked(ClickEvent evt)
    {
        BuildmasterModifyManager.Instance.UseABuildmasterModifier(representedModifier);
        InGameMenuManager.Instance.OpenOrCloseOneMenu("BuildmasterModifyDoc", true);
    }
}
