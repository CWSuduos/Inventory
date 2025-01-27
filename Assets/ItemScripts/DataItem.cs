using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using NUnit.Framework;

public class DataItem : MonoBehaviour
{
    public List items = new List();


    public ItemData GetItemById(int id)
    {
        foreach (ItemData item in items)
        {
            if (item.id == id)
            {
                return item;
            }
        }
        return null;
    }
}

public enum ItemType
{
    Consumable,
    HeadArmor,
    BodyArmor,
    HealingItem
}

[System.Serializable]
public class ItemData
{
    public int id;
    public string name;
    public Sprite image;
    public int maxCountForItem;
    public bool isStackable;
    public int removeAmount = 1;
    public float weight;
    public ItemType itemType;
    public int countInWorld;
    public int armorValue;
    public int healAmount;
}

[System.Serializable]
public class DefaultItem
{
    public int slot;
    public int itemId;
    public int count;
}