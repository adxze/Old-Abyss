using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseMoveSpeed = 5f;
    public float baseAttackDamage = 20f;
    public float baseAttackCooldown = 0.5f;
    public float baseMaxHealth = 100f;
    public float baseAttackRange = 1.2f;
    
    [Header("Current Multipliers")]
    public float moveSpeedMultiplier = 1f;
    public float attackDamageMultiplier = 1f;
    public float attackSpeedMultiplier = 1f;
    public float maxHealthBonus = 0f;
    public float attackRangeMultiplier = 1f;
    
    private DirectionalMovement2D playerMovement;
    private PlayerHealth playerHealth;
    
    void Start()
    {
        playerMovement = GetComponent<DirectionalMovement2D>();
        playerHealth = GetComponent<PlayerHealth>();
        
        ApplyAllUpgrades();
    }
    
    public void ApplyUpgrade(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.MoveSpeed:
                moveSpeedMultiplier += 0.2f;
                break;
            case UpgradeType.AttackDamage:
                attackDamageMultiplier += 0.25f;
                break;
            case UpgradeType.AttackSpeed:
                attackSpeedMultiplier += 0.15f;
                break;
            case UpgradeType.MaxHealth:
                maxHealthBonus += 30f;
                break;
            case UpgradeType.AttackRange:
                attackRangeMultiplier += 0.15f;
                break;
        }
        
        ApplyAllUpgrades();
    }
    
    void ApplyAllUpgrades()
    {
        if (playerMovement)
        {
            playerMovement.moveSpeed = baseMoveSpeed * moveSpeedMultiplier;
            playerMovement.attackDamage = baseAttackDamage * attackDamageMultiplier;
            playerMovement.attackCooldown = baseAttackCooldown / attackSpeedMultiplier;
            playerMovement.attackRange = baseAttackRange * attackRangeMultiplier;
        }
        
        if (playerHealth)
        {
            float newMaxHealth = baseMaxHealth + maxHealthBonus;
            float healthRatio = playerHealth.currentHealth / playerHealth.maxHealth;
            
            playerHealth.maxHealth = newMaxHealth;
            playerHealth.currentHealth = newMaxHealth * healthRatio;
        }
    }
}