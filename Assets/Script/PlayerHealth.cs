using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float invincibilityDuration = 1f;
    public float knockbackForce = 7f;
    
    [Header("UI")]
    public Slider healthBarSlider;
    public TextMeshProUGUI healthText;
    
    [Header("Death Screen")]
    public GameObject deathScreen;
    public Button restartButton;
    public float deathDelay = 1f;
    
    [Header("Effects")]
    public float hitFlashDuration = 0.1f;
    public Color hitFlashColor = Color.red;
    public GameObject hitParticlePrefab;
    
    private SpriteRenderer spriteRenderer;
    private DirectionalMovement2D movement;
    private Color originalColor;
    private bool isInvincible = false;
    private bool isDead = false;
    private GameManager gameManager;
    
    void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        movement = GetComponent<DirectionalMovement2D>();
        gameManager = FindObjectOfType<GameManager>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    void Start()
    {
        UpdateHealthUI();
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }
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
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{(int)currentHealth}/{(int)maxHealth}";
        }
    }
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        if (movement != null)
        {
            movement.enabled = false;
        }
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        StartCoroutine(DeathSequence());
    }
    
    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathDelay);
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
        
        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
        }
        
        Time.timeScale = 0f;
    }
    
    void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
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
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitFlashColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }
}