using UnityEngine;
public enum BossPhase
{
    Phase1,
    Phase2,
    Phase3
}
public class BossEnemyBehaviour : EnemyBehaviour
{
    [Header("Boss Phases")]
    [SerializeField] private float phase2HealthThreshold = 0.5f;
    [SerializeField] private float phase3HealthThreshold = 0.3f;

    public BossPhase CurrentPhase { get; private set; } = BossPhase.Phase1;

    protected void Awake()
    {
        base.Awake();
    }
    private void OnEnable()
    {
        if (health != null)
            health.OnDamaged += HandleDamaged;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDamaged -= HandleDamaged;
    }

    protected override void HandleDamaged(DamageData damageData, KnockbackData knockbackData)
    {
        base.HandleDamaged(damageData, knockbackData);

        float hpPercent = stats.currentHealth / stats.maxHealth;

        if (hpPercent <= phase3HealthThreshold && CurrentPhase != BossPhase.Phase3)
            EnterPhase(BossPhase.Phase3);
        else if (hpPercent <= phase2HealthThreshold && CurrentPhase == BossPhase.Phase1)
            EnterPhase(BossPhase.Phase2);
    }

    private void EnterPhase(BossPhase newPhase)
    {
        CurrentPhase = newPhase;
        Debug.Log($"{name} entered {newPhase}");

        switch (newPhase)
        {
            case BossPhase.Phase2:
                OnPhase2();
                break;
            case BossPhase.Phase3:
                OnPhase3();
                break;
        }
    }

    private void OnPhase2()
    {
        // Beispiel: höhere Ability-Frequenz
        abilityBehaviour.SetAggressionMultiplier(1.5f);
        var spider = GetComponent<EnemyStats>();
        spider.Heal(200f); // heals 50 HP instantly
        health.UpdatePhaseName($"{stats.baseStats.baseName} – Phase 2");
    }

    private void OnPhase3()
    {
        abilityBehaviour.SetAggressionMultiplier(2f);
        var spider = GetComponent<EnemyStats>();
        spider.Heal(200f);
        health.UpdatePhaseName($"{stats.baseStats.baseName} – Phase 3");
    }

    private void ChangeBossColor(Color newColor)
    {
        var sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            sprite.color = newColor;
    }
}
