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

        listElement.Q<VisualElement>("listElementIcon").AddToClassList($"{turret.turretName}Icon");

        this.turret = turret;
        this.towerDetails = towerDetails;
    }

    void onListButtonClicked(ClickEvent evt)
    {
        towerDetails.Q<Label>("name").SetBinding("text", new LocalizedString($"TurretTranslation{turret.turretName}",$"name"));

        var turretIcon = towerDetails.Q<VisualElement>("icon");
        turretIcon.ClearClassList();
        turretIcon.AddToClassList($"{turret.turretName}Icon");

        towerDetails.Q<Label>($"costName").text = "";
        towerDetails.Q<Label>($"cost").text = $"";

        FillDetailValue("fireRate", turret.baseFireRate);
        FillDetailValue("fireCountdown", turret.BaseFireCountdown);
        FillDetailValue("projectileSpeed", turret.baseProjectileSpeed);
        FillDetailValue("attackRange", turret.baseAttackRange);
        FillDetailValue("damage", turret.baseAttackDamage);
        FillDetailValue("knockbackStrength", turret.baseKnockbackStrength);
        FillDetailValue("knockbackDuration", turret.baseKnockbackDuration);

    }

    void FillDetailValue(string value, float turretValue)
    {
        towerDetails.Q<Label>($"{value}Name").SetBinding("text", new LocalizedString("TurretTranslationCommon", $"{value}"));
        towerDetails.Q<Label>($"{value}").text = $"{turretValue}";
    }
}
