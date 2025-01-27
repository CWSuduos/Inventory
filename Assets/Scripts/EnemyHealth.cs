using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    [SerializeField] private bool showHealthBar = true;

    [Header("UI References")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Text healthText;
    [SerializeField] private Canvas healthCanvas;

    [Header("Loot Settings")]
    [SerializeField] private DataItem lootData;
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private int maxLootItems = 3;

    [Header("Save Settings")]
    [SerializeField] private string enemyID; 
    private string saveFilePath;

    private void Start()
    {
        currentHealth = maxHealth;

      
        saveFilePath = Application.persistentDataPath + $"/enemy_{enemyID}.json";

       
        LoadHealth();

        UpdateHealthUI();

        if (healthCanvas != null)
        {
            healthCanvas.worldCamera = Camera.main;
        }
        if (playerInventory == null) playerInventory = FindObjectOfType<Inventory>();
        if (lootData == null) lootData = FindObjectOfType<DataItem>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }

        
        SaveHealth();
    }

    private void UpdateHealthUI()
    {
        if (!showHealthBar) return;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    private void Die()
    {
        Debug.Log($"Enemy {enemyID} has died!");

       
        currentHealth = maxHealth;

       
        AddRandomLootToInventory();

      
        DeleteHealthSave();

        UpdateHealthUI();
    }

    private void AddRandomLootToInventory()
    {
        if (lootData == null || playerInventory == null) return;

        int itemsToDrop = Random.Range(1, maxLootItems + 1);

        for (int i = 0; i < itemsToDrop; i++)
        {
            int randomIndex = Random.Range(0, lootData.items.Count);
            ItemData randomItem = lootData.items[randomIndex];

            int freeSlot = playerInventory.FindFreeSlot();

            if (freeSlot != -1)
            {
                playerInventory.AddItem(
                    freeSlot,
                    randomItem,
                    randomItem.isStackable ? randomItem.maxCountForItem : 1
                );
                Debug.Log($"Looted: {randomItem.name} x{randomItem.maxCountForItem}");
            }
        }
    }

   
    private void SaveHealth()
    {
        EnemyHealthData data = new EnemyHealthData
        {
            enemyID = enemyID,
            currentHealth = currentHealth,
            maxHealth = maxHealth
        };

        string json = JsonUtility.ToJson(data, true); 
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Enemy {enemyID} health saved!");
    }

 
    private void LoadHealth()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath); 
            EnemyHealthData data = JsonUtility.FromJson<EnemyHealthData>(json); 

            
            currentHealth = data.currentHealth;
            maxHealth = data.maxHealth;

            Debug.Log($"Enemy {enemyID} health loaded!");
        }
        else
        {
            
            currentHealth = maxHealth;
            Debug.Log($"No save found for enemy {enemyID}, initialized to max health.");
        }
    }

    private void DeleteHealthSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log($"Enemy {enemyID} save deleted.");
        }
    }
}