using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class SM_TowerListElementBehavior
{
    public Button listButton;
    public TurretBlueprint turret;

    private StatsMenuBehavior statsMenu;
    public SM_TowerListElementBehavior(TurretBlueprint _turret, VisualTreeAsset asset, StatsMenuBehavior _statsMenu)
    {
        turret = _turret;
        statsMenu = _statsMenu;

        TemplateContainer listElement = asset.Instantiate();

        listButton = listElement.Q<Button>("listElement");
        listButton.RegisterCallback<ClickEvent>(onListButtonClicked);
        listButton.text = $"{turret.turretName}";

        listElement.Q<VisualElement>("listElementIcon").AddToClassList($"{turret.turretName}Icon");
    }

    void onListButtonClicked(ClickEvent evt)
    {
        statsMenu.FillTurretStatsDetails(turret);
    }
}
