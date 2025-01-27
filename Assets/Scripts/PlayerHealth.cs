using System.IO; 
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    public int currentHealth;

    [Header("UI References")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Text healthText;

    private EquipmentSlots equipment;
    private DataItem dataItem;

    private string saveFilePath;

    private void Start()
    {
        
        saveFilePath = Application.persistentDataPath + "/healthData.json";

        equipment = FindObjectOfType<EquipmentSlots>();
        dataItem = FindObjectOfType<DataItem>();

       
        LoadHealth();

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        float totalArmor = CalculateTotalArmor();
        float damageReduction = totalArmor * 2.5f;
        int finalDamage = Mathf.RoundToInt(damage * (1 - damageReduction / 100f));

        currentHealth = Mathf.Max(currentHealth - finalDamage, 0);
        UpdateHealthUI();

        Debug.Log($"Received {damage} damage | " +
                 $"Armor: {totalArmor} ({damageReduction}%) | " +
                 $"Final damage: {finalDamage}");

        if (currentHealth <= 0) Die();

       
        SaveHealth();
    }

    private float CalculateTotalArmor()
    {
        float armor = 0f;

        // Head a
        if (equipment.EquippedHeadArmor != null)
        {
            ItemData item = dataItem.GetItemById(equipment.EquippedHeadArmor.id);
            if (item != null) armor += item.armorValue;
        }

        // Body 
        if (equipment.EquippedBodyArmor != null)
        {
            ItemData item = dataItem.GetItemById(equipment.EquippedBodyArmor.id);
            if (item != null) armor += item.armorValue;
        }

        return armor;
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);
        UpdateHealthUI();

        
        SaveHealth();
    }

    public void UpdateHealthUI()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = $"{currentHealth}/{maxHealth}";
    }

    private void Die()
    {
        Debug.Log("Player died!");
    }

    public void Initialize(int health, int max)
    {
        currentHealth = health;
        maxHealth = max;
        UpdateHealthUI();
    }

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    
    public void SaveHealth()
    {
        HealthData data = new HealthData
        {
            currentHealth = currentHealth,
            maxHealth = maxHealth
        };

        string json = JsonUtility.ToJson(data, true); 
        File.WriteAllText(saveFilePath, json); 
        Debug.Log("Player health saved!");
    }

   
    public void LoadHealth()
    {
        if (File.Exists(saveFilePath)) 
        {
            string json = File.ReadAllText(saveFilePath);
            HealthData data = JsonUtility.FromJson<HealthData>(json); 

           
            currentHealth = data.currentHealth;
            maxHealth = data.maxHealth;

            Debug.Log("Player health loaded!");
        }
        else
        {
        
            currentHealth = maxHealth;
            Debug.Log("No save file found. Initialized to max health.");
        }
    }
}