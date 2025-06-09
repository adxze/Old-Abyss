using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public float maxHealth = 100f;
    [SerializeField] private float health;
    private float lerpSpeed = 0.01f;
    
    [Header("Visibility")]
    public bool hideUntilDamaged = true;
    public float visibilityDuration = 4f;
    private float visibilityTimer = 0f;
    private bool isVisible = false;
    private CanvasGroup canvasGroup;
    
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    void Start()
    {
        health = maxHealth;
        if (healthSlider != null) healthSlider.maxValue = maxHealth;
        if (easeHealthSlider != null) easeHealthSlider.maxValue = maxHealth;
        
        if (hideUntilDamaged)
        {
            SetVisibility(false);
        }
        else
        {
            SetVisibility(true);
        }
    }

    void Update()
    {
        if (healthSlider != null && healthSlider.value != health)
        {
            healthSlider.value = health;
        }

        if (healthSlider != null && easeHealthSlider != null && 
            healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, health, lerpSpeed);
        }
        
        // Handle visibility timer
        if (isVisible && hideUntilDamaged)
        {
            visibilityTimer -= Time.deltaTime;
            if (visibilityTimer <= 0)
            {
                SetVisibility(false);
            }
        }
    }

    public void SetHealth(float newHealth)
    {
        if (newHealth < health && hideUntilDamaged)
        {
            ShowBar();
        }
        
        health = Mathf.Clamp(newHealth, 0, maxHealth);
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        
        if (healthSlider != null) healthSlider.maxValue = maxHealth;
        if (easeHealthSlider != null) easeHealthSlider.maxValue = maxHealth;
        
        health = maxHealth;  // Reset health to max
    }
    
    private void SetVisibility(bool visible)
    {
        isVisible = visible;
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
    }
    
    public void ShowBar()
    {
        SetVisibility(true);
        visibilityTimer = visibilityDuration;
    }
}