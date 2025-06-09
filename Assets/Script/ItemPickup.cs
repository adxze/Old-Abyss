using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType { HealthPotion, Coin, Weapon, Key, Generic }
    
    [Header("Item Settings")]
    public ItemType itemType = ItemType.Generic;
    public string itemName = "Item";
    public int value = 1;
    
    [Header("Visual Settings")]
    public float bobHeight = 0.5f;
    public float bobSpeed = 2f;
    public bool rotate = true;
    public float rotateSpeed = 100f;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
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
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            switch (itemType)
            {
                case ItemType.HealthPotion:
                    var playerHealth = other.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.Heal(value);
                    }
                    break;
                    
                case ItemType.Coin:
                    Debug.Log($"Collected {value} coins");
                    break;
                    
                default:
                    Debug.Log($"Collected {itemName}");
                    break;
            }
            
            Destroy(gameObject);
        }
    }
}