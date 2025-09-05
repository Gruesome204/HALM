using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class SM_TowerListElementBehavior
{
    public Button listButton;
    public TurretBlueprint turret;

    public TemplateContainer towerDetails;
    public SM_TowerListElementBehavior(TurretBlueprint turret, VisualTreeAsset asset, ref TemplateContainer towerDetails)
    {
        TemplateContainer listElement = asset.Instantiate();

        listButton = listElement.Q<Button>("listElement");
        listButton.RegisterCallback<ClickEvent>(onListButtonClicked);
        listButton.text = $"{turret.turretName}";

        this.turret = turret;
        this.towerDetails = towerDetails;
    }

    void onListButtonClicked(ClickEvent evt)
    {
        Debug.Log($"This turret is called {turret.name}");
        towerDetails.Q<Label>("name").SetBinding("text", new LocalizedString($"TurretTranslation{turret.turretName}",$"name"));

        FillDetailValue("cost", turret.buildingCost);
        FillDetailValue("fireRate", turret.fireRate);
        FillDetailValue("fireCountdown", turret.fireCountdown);
        FillDetailValue("projectileSpeed", turret.projectileSpeed);
        FillDetailValue("attackRange", turret.attackRange);
        FillDetailValue("damage", turret.attackDamage);
        FillDetailValue("knockbackStrenght", turret.knockbackStrength);
        FillDetailValue("knockbackDuration", turret.knockbackDuration);

    }

    void FillDetailValue(string value, float turretValue)
    {
        towerDetails.Q<Label>($"{value}Name").SetBinding("text", new LocalizedString("TurretTranslationCommon", $"{value}"));
        towerDetails.Q<Label>($"{value}").text = $"{turretValue}";
    }
}
