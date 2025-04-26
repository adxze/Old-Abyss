using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float moveSpeed = 3f;
    public float detectionRadius = 10f;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float knockbackForce = 2f;
    public Transform healthBar; 

    [Header("Targeting")]
    public Transform playerTarget;
    public LayerMask playerLayer;
    public bool autoDetectPlayer = true;

    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    [Header("UI")]
    public HealthBar healthBarComponent;
    
    
    [Header("Item Drops")]
    public GameObject[] possibleDrops;   
    public float dropChance = 0.4f;      

    // Private variables
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Direction16 currentDir = Direction16.S;
    private float lastOctant = 8f;            // Default to South
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private bool isDead = false;

    // 0 = E … 15 = ESE (same as player DirectionalMovement2D)
    public enum Direction16 { E, ENE, NE, NNE, N, NNW, NW, WNW, W, WSW, SW, SSW, S, SSE, SE, ESE }
    public Direction16 CurrentDirection => currentDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!animator) animator = GetComponent<Animator>();
        
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Auto-find player if enabled
        if (autoDetectPlayer && playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        if (healthBarComponent != null)
        {
            healthBarComponent.SetMaxHealth(maxHealth);
        }
        

        SetAnimatorFromDirection(currentDir);
    }

    private void Update()
    {
        if (isDead) return;
        
        // Update attack cooldown
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
        

        if (isAttacking) return;
        
        // Check if player is in range
        if (playerTarget != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            

            if (distanceToPlayer <= detectionRadius)
            {

                if (distanceToPlayer <= attackRange && attackTimer <= 0)
                {
                    StartCoroutine(AttackCoroutine());
                }

                else if (distanceToPlayer > attackRange)
                {

                    moveDirection = (playerTarget.position - transform.position).normalized;
                    

                    UpdateDirection(moveDirection);
                }
                else
                {

                    moveDirection = Vector2.zero;
                }
            }
            else
            {

                moveDirection = Vector2.zero;
            }
        }
        else
        {

            moveDirection = Vector2.zero;
        }
        

        if (animator)
        {
            animator.SetFloat("Speed", moveDirection.sqrMagnitude);
        }
    }

    private void LateUpdate()
    {

        if (animator)
        {
            animator.SetFloat("Octant", lastOctant);
        }
    }

    private void FixedUpdate()
    {
        if (!isDead && !isAttacking)
        {
            // Move the enemy
            rb.linearVelocity = moveDirection * moveSpeed;
        }
    }


    private void UpdateDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;
            int sector = Mathf.RoundToInt(angle / 22.5f) & 15;

            Direction16 newDir = (Direction16)sector;
            if (newDir != currentDir)
            {
                currentDir = newDir;
                SetAnimatorFromDirection(currentDir);
            }
        }
    }

    // Convert direction enum to animator parameter
    private void SetAnimatorFromDirection(Direction16 dir)
    {
        int sector = (int)dir;
        float octant;

        if (sector >= 0 && sector <= 4) {
            octant = sector;
        }
        else if (sector >= 5 && sector <= 8) {
            octant = 8 - sector; // Map to E through NE
        }
        else if (sector == 12) {
            octant = 8; 
        }
        else if (sector >= 9 && sector <= 11) {
            octant = 12 - sector + 4; // Map to octants 7-4
        }
        else {
            octant = sector - 8; // Map to octants 5-7
        }

        lastOctant = octant;
        if (animator) animator.SetFloat("Octant", octant);

        bool flip = sector >= 5 && sector <= 11;
        if (spriteRenderer) spriteRenderer.flipX = flip;
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        
        if (animator)
        {
            animator.SetTrigger("Attack");
        }
        
        yield return new WaitForSeconds(0.3f);
        
        if (playerTarget != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer <= attackRange)
            {
                PlayerHealth playerHealth = playerTarget.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
            }
        }
        
        yield return new WaitForSeconds(0.4f);
        
        isAttacking = false;
        attackTimer = attackCooldown;
    }

    // Called when enemy takes damage
    public void TakeDamage(float damage, Vector2 knockbackDirection = default(Vector2))
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        if (healthBarComponent != null)
        {
            healthBarComponent.SetHealth(currentHealth);
        }
        
        if (knockbackDirection != default(Vector2))
        {
            rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }
        
        StartCoroutine(HitFlashEffect());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator HitFlashEffect()
    {
        Color originalColor = spriteRenderer.color;
        
        spriteRenderer.color = Color.red;
        
        yield return new WaitForSeconds(0.15f);
        
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
    
        TryDropItem();
    
        if (animator)
        {
            animator.SetFloat("Octant", lastOctant);
        
            animator.SetTrigger("Death");
        }
    
        GetComponent<Collider2D>().enabled = false;
        rb.isKinematic = true;
    
        Destroy(gameObject, 1f);
    }


    private void TryDropItem()
    {
        if (possibleDrops.Length > 0 && Random.value <= dropChance)
        {
            int itemIndex = Random.Range(0, possibleDrops.Length);
            GameObject selectedItem = possibleDrops[itemIndex];

            Vector3 dropPosition = transform.position;
            dropPosition.y += 0.2f; // Sliaght Y offset to avoid ground clipping

            Instantiate(selectedItem, dropPosition, Quaternion.identity);
        }

    }

    public void ForceOctantParameter()
    {
        if (animator)
        {
            animator.SetFloat("Octant", lastOctant);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}