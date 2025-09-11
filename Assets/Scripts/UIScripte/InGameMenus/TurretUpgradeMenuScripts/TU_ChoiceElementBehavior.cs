using UnityEngine;
using UnityEngine.UIElements;

public class TU_ChoiceElementBehavior
{
    public VisualElement border;
    private Button button;

    private TurretUpgradeChoiceSO.UpgradeOption option;

    public TU_ChoiceElementBehavior(VisualTreeAsset asset, TurretUpgradeChoiceSO.UpgradeOption _option)
    {
        TemplateContainer choiceElement = asset.Instantiate();

        option = _option;

        border = choiceElement.Q<VisualElement>();
        button = choiceElement.Q<Button>();
        button.RegisterCallback<ClickEvent>(OnClicked);
    }

    void OnClicked(ClickEvent evt)
    {

    }
}
