using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;



public class GameData : MonoBehaviour
{
    
}
[System.Serializable]
public class HealthData
{
    public int currentHealth;
    public int maxHealth;
}
[System.Serializable]
public class EnemyHealthData
{
    public string enemyID;
    public int currentHealth;
    public int maxHealth;
}
[System.Serializable]
public class EquipmentData
{
    public int headArmorId; 
    public int headArmorCount; 
    public int bodyArmorId; 
    public int bodyArmorCount; 
}
[System.Serializable]
public class InventoryData
{
    public List<ItemInventory> items = new List<ItemInventory>();
}
