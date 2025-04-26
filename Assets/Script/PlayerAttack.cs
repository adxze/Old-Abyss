using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackDamage = 20f;
    public float attackRange = 1.2f;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayers;
    
    [Header("Effects")]
    public GameObject attackEffectPrefab; // Optional visual effect
    
    // References
    private DirectionalMovement2D movement;
    private Animator animator;
    private float cooldownTimer = 0f;
    private bool isAttacking = false;
    
    void Start()
    {
        movement = GetComponent<DirectionalMovement2D>();
        animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        // Update cooldown timer
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        // Don't accept attack input during cooldown or while attacking
        if (isAttacking || cooldownTimer > 0) return;
        
        // Check for left mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }
    
    void Attack()
    {
        // Start cooldown
        cooldownTimer = attackCooldown;
        
        // Trigger attack animation if we have an animator
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Start attack coroutine to sync with animation timing
        StartCoroutine(AttackCoroutine());
    }
    
    IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        
        // Wait a bit for the animation to reach the "hit" frame
        yield return new WaitForSeconds(0.2f);
        
        // Get attack direction based on the player's current facing direction
        Vector2 attackDir = GetAttackDirection();
        
        // Calculate attack position (slightly in front of player in attack direction)
        Vector2 attackPos = (Vector2)transform.position + (attackDir * 0.5f);
        
        // Check for enemies in attack range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayers);
        
        // Spawn attack effect if available
        if (attackEffectPrefab != null)
        {
            Instantiate(attackEffectPrefab, attackPos, Quaternion.identity);
        }
        
        // Deal damage to enemies hit
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                // Direction from player to enemy (for knockback)
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                
                // Deal damage with knockback
                enemyController.TakeDamage(attackDamage, knockbackDir);
            }
        }
        
        // Wait for attack animation to finish
        yield return new WaitForSeconds(0.3f);
        
        isAttacking = false;
    }
    
    Vector2 GetAttackDirection()
    {
        // If you have the current direction from your DirectionalMovement2D component
        if (movement != null)
        {
            DirectionalMovement2D.Direction16 dir = movement.CurrentDirection;
            
            switch (dir)
            {
                case DirectionalMovement2D.Direction16.E:
                    return Vector2.right;
                case DirectionalMovement2D.Direction16.NE:
                case DirectionalMovement2D.Direction16.ENE:
                    return new Vector2(0.7f, 0.7f).normalized;
                case DirectionalMovement2D.Direction16.N:
                case DirectionalMovement2D.Direction16.NNE:
                case DirectionalMovement2D.Direction16.NNW:
                    return Vector2.up;
                case DirectionalMovement2D.Direction16.NW:
                case DirectionalMovement2D.Direction16.WNW:
                    return new Vector2(-0.7f, 0.7f).normalized;
                case DirectionalMovement2D.Direction16.W:
                    return Vector2.left;
                case DirectionalMovement2D.Direction16.SW:
                case DirectionalMovement2D.Direction16.WSW:
                    return new Vector2(-0.7f, -0.7f).normalized;
                case DirectionalMovement2D.Direction16.S:
                case DirectionalMovement2D.Direction16.SSW:
                case DirectionalMovement2D.Direction16.SSE:
                    return Vector2.down;
                case DirectionalMovement2D.Direction16.SE:
                case DirectionalMovement2D.Direction16.ESE:
                    return new Vector2(0.7f, -0.7f).normalized;
                default:
                    return Vector2.down;
            }
        }
        else
        {
            // Fallback: use mouse position to determine attack direction
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return (mousePos - (Vector2)transform.position).normalized;
        }
    }
    
    // Helper to visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}