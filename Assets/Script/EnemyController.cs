using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;
    public float stopDistance = 1.2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float knockbackForce = 2f;
    public Transform healthBar; 

    [Header("Targeting")]
    public Transform playerTarget;
    public bool autoDetectPlayer = true;

    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    
    [Header("UI")]
    public HealthBar healthBarComponent;
    
    [Header("Item Drops")]
    public GameObject[] possibleDrops;   
    public float dropChance = 0.4f;      

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector3 lastPlayerPosition;
    private Direction16 currentDir = Direction16.S;
    private float lastOctant = 8f;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private bool isDead = false;
    private bool playerMoved = false;
    private float playerMovementThreshold = 0.1f;

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

        if (playerTarget != null)
        {
            lastPlayerPosition = playerTarget.position;
        }

        SetAnimatorFromDirection(currentDir);
    }

    private void Update()
    {
        if (isDead) return;
        
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        if (isAttacking) return;
        
        if (playerTarget != null)
        {
            CheckPlayerMovement();
            
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            
            if (distanceToPlayer <= attackRange && attackTimer <= 0)
            {
                StartCoroutine(AttackCoroutine());
            }
            else if (distanceToPlayer > stopDistance || playerMoved)
            {
                moveDirection = (playerTarget.position - transform.position).normalized;
                UpdateDirection(moveDirection);
                playerMoved = false;
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

    private void CheckPlayerMovement()
    {
        if (playerTarget != null)
        {
            float distanceMoved = Vector3.Distance(playerTarget.position, lastPlayerPosition);
            
            if (distanceMoved > playerMovementThreshold)
            {
                playerMoved = true;
                lastPlayerPosition = playerTarget.position;
            }
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
            rb.linearVelocity = moveDirection * moveSpeed;
        }
        else if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
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

    private void SetAnimatorFromDirection(Direction16 dir)
    {
        int sector = (int)dir;
        float octant;

        if (sector >= 0 && sector <= 4) {
            octant = sector;
        }
        else if (sector >= 5 && sector <= 8) {
            octant = 8 - sector; 
        }
        else if (sector == 12) {
            octant = 8; 
        }
        else if (sector >= 9 && sector <= 11) {
            octant = 12 - sector + 4; 
        }
        else {
            octant = sector - 8; 
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

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.OnEnemyDied();
        }

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
            
            if (selectedItem != null)  
            {
                Vector3 dropPosition = transform.position;
                dropPosition.y += 0.2f;
                Instantiate(selectedItem, dropPosition, Quaternion.identity);
            }
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}