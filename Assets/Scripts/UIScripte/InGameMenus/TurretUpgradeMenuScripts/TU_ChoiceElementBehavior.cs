using UnityEngine;
using UnityEngine.UIElements;

public class TU_ChoiceElementBehavior
{
    public VisualElement border;
    private Button button;

    private TurretUpgradeChoiceSO choice;

    public TU_ChoiceElementBehavior(VisualTreeAsset asset, TurretUpgradeChoiceSO _choice)
    {
        TemplateContainer choiceElement = asset.Instantiate();

        choice = _choice;

        border = choiceElement.Q<VisualElement>();
        button = choiceElement.Q<Button>();
        button.RegisterCallback<ClickEvent>(OnClicked);
    }

    void OnClicked(ClickEvent evt)
    {

    }
}
