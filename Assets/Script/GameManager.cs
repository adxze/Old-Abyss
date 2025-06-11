using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public float gameTimer = 0f;
    public bool gameActive = true;
    
    [Header("Level System")]
    public int playerLevel = 1;
    public float currentExp = 0f;
    public float expToNextLevel = 100f;
    public float expMultiplier = 1.5f;
    
    [Header("Spawning")]
    public EnemySpawner[] enemySpawners;
    public float baseSpawnRate = 5f;
    public float spawnRateDecrease = 0.1f;
    public float minSpawnRate = 0.5f;
    
    [Header("Enemy Scaling")]
    public float enemyHealthMultiplier = 1.1f;
    public float enemySpeedMultiplier = 1.05f;
    public float enemyDamageMultiplier = 1.08f;
    
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI levelText;
    public Slider expSlider;
    public GameObject upgradePanel;
    public Button[] upgradeButtons;
    
    [Header("Upgrade System")]
    public PlayerUpgrades playerUpgrades;
    
    private float nextLevelThreshold = 60f;
    private int currentDifficultyLevel = 1;
    private bool upgradePanelOpen = false;
    
    void Start()
    {
        Time.timeScale = 1f;
        UpdateUI();
        SetupUpgradeButtons();
    }
    
    void Update()
    {
        if (!gameActive) return;
        
        gameTimer += Time.deltaTime;
        
        UpdateDifficulty();
        UpdateUI();
    }
    
    void UpdateDifficulty()
    {
        int newDifficultyLevel = Mathf.FloorToInt(gameTimer / nextLevelThreshold) + 1;
        
        if (newDifficultyLevel > currentDifficultyLevel)
        {
            currentDifficultyLevel = newDifficultyLevel;
            IncreaseDifficulty();
        }
    }
    
    void IncreaseDifficulty()
    {
        foreach (EnemySpawner spawner in enemySpawners)
        {
            spawner.maxEnemiesAlive += 2;
            
            float newSpawnRate = Mathf.Max(minSpawnRate, baseSpawnRate - (currentDifficultyLevel * spawnRateDecrease));
            spawner.minSpawnTime = newSpawnRate;
            spawner.maxSpawnTime = newSpawnRate + 2f;
            
            spawner.healthVariation += 0.1f;
            spawner.speedVariation += 0.05f;
            spawner.damageVariation += 0.05f;
        }
    }
    
    public void AddExperience(float exp)
    {
        currentExp += exp;
        
        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }
    
    void LevelUp()
    {
        currentExp -= expToNextLevel;
        playerLevel++;
        expToNextLevel *= expMultiplier;
        
        ShowUpgradePanel();
    }
    
    void ShowUpgradePanel()
    {
        upgradePanelOpen = true;
        upgradePanel.SetActive(true);
        Time.timeScale = 0f;
        
        GenerateRandomUpgrades();
    }
    
    void GenerateRandomUpgrades()
    {
        List<UpgradeType> availableUpgrades = new List<UpgradeType>
        {
            UpgradeType.MoveSpeed,
            UpgradeType.AttackDamage,
            UpgradeType.AttackSpeed,
            UpgradeType.MaxHealth,
            UpgradeType.AttackRange
        };
        
        for (int i = 0; i < upgradeButtons.Length && i < availableUpgrades.Count; i++)
        {
            UpgradeType upgradeType = availableUpgrades[Random.Range(0, availableUpgrades.Count)];
            availableUpgrades.Remove(upgradeType);
            
            SetupUpgradeButton(upgradeButtons[i], upgradeType);
        }
    }
    
    void SetupUpgradeButton(Button button, UpgradeType upgradeType)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SelectUpgrade(upgradeType));
        
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText == null)
        {
            Text legacyText = button.GetComponentInChildren<Text>();
            if (legacyText != null)
            {
                legacyText.text = GetUpgradeDescription(upgradeType);
            }
        }
        else
        {
            buttonText.text = GetUpgradeDescription(upgradeType);
        }
    }
    
    string GetUpgradeDescription(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.MoveSpeed:
                return "Move Speed +20%";
            case UpgradeType.AttackDamage:
                return "Attack Damage +25%";
            case UpgradeType.AttackSpeed:
                return "Attack Speed +15%";
            case UpgradeType.MaxHealth:
                return "Max Health +30";
            case UpgradeType.AttackRange:
                return "Attack Range +15%";
            default:
                return "Unknown Upgrade";
        }
    }
    
    public void SelectUpgrade(UpgradeType upgradeType)
    {
        playerUpgrades.ApplyUpgrade(upgradeType);
        
        upgradePanel.SetActive(false);
        upgradePanelOpen = false;
        Time.timeScale = 1f;
    }
    
    void SetupUpgradeButtons()
    {
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            upgradeButtons[i].gameObject.SetActive(true);
        }
    }
    
    void UpdateUI()
    {
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(gameTimer / 60f);
            int seconds = Mathf.FloorToInt(gameTimer % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        
        if (levelText)
        {
            levelText.text = "Level: " + playerLevel;
        }
        
        if (expSlider)
        {
            expSlider.value = currentExp / expToNextLevel;
        }
    }
    
    public void GameOver()
    {
        gameActive = false;
        Time.timeScale = 0f;
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}

public enum UpgradeType
{
    MoveSpeed,
    AttackDamage,
    AttackSpeed,
    MaxHealth,
    AttackRange
}