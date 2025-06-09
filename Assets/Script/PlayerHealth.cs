using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float invincibilityDuration = 1f;
    public float knockbackForce = 7f;
    
    [Header("UI")]
    public Image healthBarFill;
    public Text healthText;
    
    [Header("Effects")]
    public float hitFlashDuration = 0.1f;
    public Color hitFlashColor = Color.red;
    public GameObject hitParticlePrefab;
    
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private DirectionalMovement2D movement;
    private Color originalColor;
    private bool isInvincible = false;
    private bool isDead = false;
    
    void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        movement = GetComponent<DirectionalMovement2D>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    void Start()
    {
        UpdateHealthUI();
    }
    
    public void TakeDamage(float damage)
    {
        if (isInvincible || isDead) return;
        
        currentHealth -= damage;
        
        currentHealth = Mathf.Max(0, currentHealth);
        
        UpdateHealthUI();
        
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashSprite());
        }
        
        if (hitParticlePrefab != null)
        {
            Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
        }
        
        if (movement != null)
        {
            EnemyController[] enemies = FindObjectsOfType<EnemyController>();
            Vector2 knockbackDir = Vector2.zero;
            float closestDistance = float.MaxValue;
            
            foreach (EnemyController enemy in enemies)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    knockbackDir = (transform.position - enemy.transform.position).normalized;
                }
            }
            
            // If we found an enemy, apply knockback
            if (knockbackDir != Vector2.zero)
            {
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }
    
    void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
    
    void Die()
    {
        isDead = true;
        
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        if (movement != null)
        {
            movement.enabled = false;
        }
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
    }
    
    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        
        if (spriteRenderer != null)
        {
            float endTime = Time.time + invincibilityDuration;
            while (Time.time < endTime)
            {
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
            
            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityDuration);
        }
        
        isInvincible = false;
    }
    
    IEnumerator FlashSprite()
    {
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }
}