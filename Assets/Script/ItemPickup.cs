using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType { HealthPotion, Coin, Weapon, Key, Experience, Generic }
    
    [Header("Item Settings")]
    public ItemType itemType = ItemType.Generic;
    public string itemName = "Item";
    public int value = 1;
    
    [Header("Experience Settings")]
    public float magnetRange = 1f;
    public float magnetSpeed = 8f;
    public bool autoMagnet = false;
    
    [Header("Visual Settings")]
    public float bobHeight = 0.5f;
    public float bobSpeed = 2f;
    public bool rotate = true;
    public float rotateSpeed = 100f;
    
    private Vector3 startPosition;
    private Transform player;
    private GameManager gameManager;
    private bool isBeingMagnetized = false;
    
    void Start()
    {
        startPosition = transform.position;
        
        if (itemType == ItemType.Experience)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj) player = playerObj.transform;
            
            gameManager = FindObjectOfType<GameManager>();
            
            if (autoMagnet)
            {
                Invoke("StartMagnetizing", 0.5f);
            }
        }
    }
    
    void Update()
    {
        if (itemType == ItemType.Experience && !isBeingMagnetized)
        {
            Bob();
            CheckMagnetRange();
        }
        else if (itemType == ItemType.Experience && isBeingMagnetized)
        {
            MoveToPlayer();
        }
        else
        {
            RegularMovement();
        }
    }
    
    void RegularMovement()
    {
        if (bobHeight > 0)
        {
            float newY = startPosition.y + (Mathf.Sin(Time.time * bobSpeed) * bobHeight * 0.1f);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        
        if (rotate)
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }
    
    void Bob()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight * 0.1f;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void CheckMagnetRange()
    {
        if (player && Vector2.Distance(transform.position, player.position) <= magnetRange)
        {
            StartMagnetizing();
        }
    }
    
    void StartMagnetizing()
    {
        isBeingMagnetized = true;
    }
    
    void MoveToPlayer()
    {
        if (player)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, magnetSpeed * Time.deltaTime);
            
            if (Vector2.Distance(transform.position, player.position) < 0.3f)
            {
                CollectItem();
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectItem();
        }
    }
    
    void CollectItem()
    {
        switch (itemType)
        {
            case ItemType.HealthPotion:
                var playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(value);
                }
                break;
                
            case ItemType.Coin:
                Debug.Log($"Collected {value} coins");
                break;
                
            case ItemType.Experience:
                if (gameManager)
                {
                    gameManager.AddExperience(value);
                }
                break;
                
            default:
                Debug.Log($"Collected {itemName}");
                break;
        }
        
        Destroy(gameObject);
    }
}