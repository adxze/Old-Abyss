using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class DirectionalMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    
    [Header("Attack Settings")]
    public float attackDamage = 20f;
    public float attackRange = 1.2f;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayers;
    
    [Header("Effects")]
    public GameObject attackEffectPrefab;
    
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Direction16 currentDir = Direction16.S;
    private float lastOctant = 8f;
    
    private Animator animator;
    private float cooldownTimer = 0f;
    private bool isAttacking = false;
    
    public enum Direction16 { E, ENE, NE, NNE, N, NNW, NW, WNW, W, WSW, SW, SSW, S, SSE, SE, ESE }
    public Direction16 CurrentDirection => currentDir;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        if (!isAttacking && cooldownTimer <= 0 && Input.GetMouseButtonDown(0))
        {
            Attack();
            return;
        }
        
        if (isAttacking) return;
        
        HandleMovementInput();
        UpdateAnimator();
    }
    
    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        moveDirection = new Vector2(horizontal, vertical).normalized;
        
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            UpdateDirection(moveDirection);
        }
    }
    
    private void UpdateAnimator()
    {
        if (animator)
        {
            animator.SetFloat("Speed", moveDirection.sqrMagnitude);
            animator.SetFloat("Octant", lastOctant);
        }
    }
    
    private void FixedUpdate()
    {
        if (!isAttacking)
        {
            rb.linearVelocity = moveDirection * moveSpeed;
        }
        else
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
        
        if (sector >= 0 && sector <= 4)
        {
            octant = sector;
        }
        else if (sector >= 5 && sector <= 8)
        {
            octant = 8 - sector;
        }
        else if (sector == 12)
        {
            octant = 8;
        }
        else if (sector >= 9 && sector <= 11)
        {
            octant = 12 - sector + 4;
        }
        else
        {
            octant = sector - 8;
        }
        
        lastOctant = octant;
        
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            bool flip = sector >= 5 && sector <= 11;
            spriteRenderer.flipX = flip;
        }
    }
    
    private void Attack()
    {
        cooldownTimer = attackCooldown;
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        StartCoroutine(AttackCoroutine());
    }
    
    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        
        yield return new WaitForSeconds(0.2f);
        
        Vector2 attackDir = GetAttackDirection();
        Vector2 attackPos = (Vector2)transform.position + (attackDir * 0.5f);
        
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayers);
        
        if (attackEffectPrefab != null)
        {
            Instantiate(attackEffectPrefab, attackPos, Quaternion.identity);
        }
        
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyController.TakeDamage(attackDamage, knockbackDir);
            }
        }
        
        yield return new WaitForSeconds(0.3f);
        
        isAttacking = false;
    }
    
    private Vector2 GetAttackDirection()
    {
        switch (currentDir)
        {
            case Direction16.E:
                return Vector2.right;
            case Direction16.NE:
            case Direction16.ENE:
                return new Vector2(0.7f, 0.7f).normalized;
            case Direction16.N:
            case Direction16.NNE:
            case Direction16.NNW:
                return Vector2.up;
            case Direction16.NW:
            case Direction16.WNW:
                return new Vector2(-0.7f, 0.7f).normalized;
            case Direction16.W:
                return Vector2.left;
            case Direction16.SW:
            case Direction16.WSW:
                return new Vector2(-0.7f, -0.7f).normalized;
            case Direction16.S:
            case Direction16.SSW:
            case Direction16.SSE:
                return Vector2.down;
            case Direction16.SE:
            case Direction16.ESE:
                return new Vector2(0.7f, -0.7f).normalized;
            default:
                return Vector2.down;
        }
    }
    
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}