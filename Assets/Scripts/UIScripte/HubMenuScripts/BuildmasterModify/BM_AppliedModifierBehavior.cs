using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class BM_AppliedModifierBehavior
{
    public BuildMasterModifier representedModifier;
    private BuildmasterModifyBehavior buildmaster;

    public VisualElement mainContainer;
    private VisualElement icon;
    private Label name;
    private Label description;

    private Button button;

    public BM_AppliedModifierBehavior(VisualTreeAsset asset, BuildMasterModifier _representedModifier, BuildmasterModifyBehavior _buildmaster)
    {
        representedModifier = _representedModifier;
        buildmaster = _buildmaster;

        TemplateContainer modifierButton = asset.Instantiate();

        mainContainer = modifierButton.Q<VisualElement>("mainContainer");
        icon = modifierButton.Q<VisualElement>("icon");

        name = modifierButton.Q<Label>("name");
        name.SetBinding("text", new LocalizedString($"BMTranslations{_representedModifier.options.name}", "name"));

        description = modifierButton.Q<Label>("description");
        description.SetBinding("text", new LocalizedString($"BMTranslations{_representedModifier.options.name}", "description"));

        button = modifierButton.Q<Button>("button");
        button.RegisterCallback<ClickEvent>(OnButtonClicked);
    }

    private void OnButtonClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("wood");

        buildmaster.ClearDetails();
        buildmaster.FillModifierDetails(representedModifier, true,true);
    }
}
