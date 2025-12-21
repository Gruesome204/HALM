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

        var turretIcon = towerDetails.Q<VisualElement>("icon");
        turretIcon.AddToClassList($"{turret.turretName}Icon");
        FillDetailValue("fireRate", turret.baseFireRate);
        FillDetailValue("fireCountdown", turret.BaseFireCountdown);
        FillDetailValue("projectileSpeed", turret.baseProjectileSpeed);
        FillDetailValue("attackRange", turret.baseAttackRange);
        FillDetailValue("damage", turret.baseAttackDamage);
        FillDetailValue("knockbackStrength", turret.baseKnockbackStrength);
        FillDetailValue("knockbackDuration", turret.baseKnockbackDuration);

        // Fill buy cost
        FillBuyCostDetails();
    }

    void FillDetailValue(string value, float turretValue)
    {
        towerDetails.Q<Label>($"{value}Name").SetBinding("text", new LocalizedString("TurretTranslationCommon", $"{value}"));
        towerDetails.Q<Label>($"{value}").text = $"{turretValue}";
    }

    void FillBuyCostDetails()
    {
        // Make sure your UXML has a container named "buyCostContainer"
        var costContainer = towerDetails.Q<VisualElement>("buyCostContainer");
        costContainer.Clear();

        if (turret.buyCost == null || turret.buyCost.Length == 0)
        {
            costContainer.Add(new Label("No cost"));
            return;
        }

        foreach (var cost in turret.buyCost)
        {
            // You can use an icon or just text
            var costLabel = new Label($"{cost.amount} {cost.resourceType}");
            costContainer.Add(costLabel);
        }
    }

}
