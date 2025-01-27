using System.IO; // Для работы с файлами
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlots : MonoBehaviour
{
    [Header("Equipment UI")]
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private GameObject headSlot;
    [SerializeField] private GameObject bodySlot;
    [SerializeField] private Image headIcon;
    [SerializeField] private Image bodyIcon;
    [SerializeField] private Text headText;
    [SerializeField] private Text bodyText;
    [SerializeField] private Sprite emptySlotSprite;

    private ItemInventory equippedHeadArmor;
    private ItemInventory equippedBodyArmor;
    private Inventory inventory;

    private string saveFilePath;

    private void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        InitializeSlots();

    
        saveFilePath = Application.persistentDataPath + "/equipmentData.json";

  
        LoadEquipment();
    }

    public void EquipArmor(ItemInventory item, ItemData itemData)
    {
        if (itemData == null) return;

        int freeSlot = FindFirstEmptySlot();

        if (itemData.itemType == ItemType.HeadArmor)
        {
            if (equippedHeadArmor != null)
            {
                if (freeSlot != -1)
                {
                    inventory.AddItem(freeSlot, inventory.data.items[equippedHeadArmor.id], equippedHeadArmor.count);
                }
                else
                {
                    Debug.LogWarning("Нет места в инвентаре для снятия текущего предмета!");
                    return;
                }
            }

            equippedHeadArmor = new ItemInventory
            {
                id = item.id,
                count = item.count
            };
            headIcon.sprite = itemData.image;
            headText.text = $"+ {itemData.armorValue}";

            if (inventory.currentId != -1)
            {
                inventory.ClearSlot(inventory.currentId);
                inventory.currentId = -1;
                inventory.movingObject.gameObject.SetActive(false);
            }
        }
        else if (itemData.itemType == ItemType.BodyArmor)
        {
            if (equippedBodyArmor != null)
            {
                if (freeSlot != -1)
                {
                    inventory.AddItem(freeSlot, inventory.data.items[equippedBodyArmor.id], equippedBodyArmor.count);
                }
                else
                {
                    Debug.LogWarning("Нет места в инвентаре для снятия текущего предмета!");
                    return;
                }
            }

            equippedBodyArmor = new ItemInventory
            {
                id = item.id,
                count = item.count
            };
            bodyIcon.sprite = itemData.image;
            bodyText.text = $"+ {itemData.armorValue}";

            if (inventory.currentId != -1)
            {
                inventory.ClearSlot(inventory.currentId);
                inventory.currentId = -1;
                inventory.movingObject.gameObject.SetActive(false);
            }
        }

        
        UpdateEquipmentUI();
        SaveEquipment();
    }

    public void UnequipHeadArmor()
    {
        if (equippedHeadArmor != null)
        {
            int freeSlot = FindFirstEmptySlot();
            if (freeSlot != -1)
            {
                inventory.AddItem(freeSlot, inventory.data.items[equippedHeadArmor.id], equippedHeadArmor.count);
                equippedHeadArmor = null;
                headIcon.sprite = emptySlotSprite;
                headText.text = "";

                
                SaveEquipment();
            }
            else
            {
                Debug.LogWarning("Нет свободного места в инвентаре!");
            }
        }
    }

    public void UnequipBodyArmor()
    {
        if (equippedBodyArmor != null)
        {
            int freeSlot = FindFirstEmptySlot();
            if (freeSlot != -1)
            {
                inventory.AddItem(freeSlot, inventory.data.items[equippedBodyArmor.id], equippedBodyArmor.count);
                equippedBodyArmor = null;
                bodyIcon.sprite = emptySlotSprite;
                bodyText.text = "";

                
                SaveEquipment();
            }
            else
            {
                Debug.LogWarning("Нет свободного места в инвентаре!");
            }
        }
    }

    private void SaveEquipment()
    {
        EquipmentData data = new EquipmentData
        {
            headArmorId = equippedHeadArmor != null ? equippedHeadArmor.id : 0,
            headArmorCount = equippedHeadArmor != null ? equippedHeadArmor.count : 0,
            bodyArmorId = equippedBodyArmor != null ? equippedBodyArmor.id : 0,
            bodyArmorCount = equippedBodyArmor != null ? equippedBodyArmor.count : 0
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Equipment data saved!");
    }

    private void LoadEquipment()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            EquipmentData data = JsonUtility.FromJson<EquipmentData>(json);

            if (data.headArmorId != 0)
            {
                ItemData headArmorData = inventory.data.GetItemById(data.headArmorId);
                equippedHeadArmor = new ItemInventory
                {
                    id = data.headArmorId,
                    count = data.headArmorCount
                };
                headIcon.sprite = headArmorData.image;
                headText.text = $"+ {headArmorData.armorValue}";
            }

            if (data.bodyArmorId != 0)
            {
                ItemData bodyArmorData = inventory.data.GetItemById(data.bodyArmorId);
                equippedBodyArmor = new ItemInventory
                {
                    id = data.bodyArmorId,
                    count = data.bodyArmorCount
                };
                bodyIcon.sprite = bodyArmorData.image;
                bodyText.text = $"+ {bodyArmorData.armorValue}";
            }

            Debug.Log("Equipment data loaded!");
        }
        else
        {
            Debug.Log("No equipment save file found. Slots initialized to empty.");
        }
    }
    private void SetupSlotTriggers(GameObject slot, ItemType type)
    {
        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = slot.AddComponent<EventTrigger>();

        // Enter
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnPointerEnter(slot); });
        trigger.triggers.Add(enterEntry);

        // Exit
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnPointerExit(); });
        trigger.triggers.Add(exitEntry);

        // Drop
        EventTrigger.Entry dropEntry = new EventTrigger.Entry();
        dropEntry.eventID = EventTriggerType.Drop;
        dropEntry.callback.AddListener((data) => { OnDrop(slot, type); });
        trigger.triggers.Add(dropEntry);

        // Click
        Button button = slot.GetComponent<Button>();
        if (button == null)
            button = slot.AddComponent<Button>();

        button.onClick.AddListener(() => OnSlotClick(type));
    }
    public void OnPointerEnter(GameObject slot)
    {
        if (inventory.currentId != -1)
        {
            ItemData draggedItemData = inventory.data.GetItemById(inventory.currentItem.id);
            if (draggedItemData != null)
            {
                bool isCorrectSlot =
                    (slot == headSlot && draggedItemData.itemType == ItemType.HeadArmor) ||
                    (slot == bodySlot && draggedItemData.itemType == ItemType.BodyArmor);

                if (isCorrectSlot)
                {
                    slot.GetComponent<Image>().color = new Color(0.8f, 0.8f, 1f); 
                }
            }
        }
    }
    public void OnPointerExit()
    {
        headSlot.GetComponent<Image>().color = Color.white;
        bodySlot.GetComponent<Image>().color = Color.white;
    }
    public void OnDrop(GameObject slot, ItemType type)
    {
        if (inventory.currentId != -1)
        {
            ItemData draggedItemData = inventory.data.GetItemById(inventory.currentItem.id);
            if (draggedItemData != null && draggedItemData.itemType == type)
            {
                int freeSlot = FindFirstEmptySlot();

                if ((type == ItemType.HeadArmor && equippedHeadArmor != null) ||
                    (type == ItemType.BodyArmor && equippedBodyArmor != null))
                {
                    if (freeSlot == -1)
                    {
                        Debug.LogWarning("Нет места в инвентаре для снятия текущего предмета!");
                        slot.GetComponent<Image>().color = Color.white;
                        return;
                    }

                    if (type == ItemType.HeadArmor && equippedHeadArmor != null)
                    {
                        inventory.AddItem(freeSlot, inventory.data.items[equippedHeadArmor.id], equippedHeadArmor.count);
                    }
                    else if (type == ItemType.BodyArmor && equippedBodyArmor != null)
                    {
                        inventory.AddItem(freeSlot, inventory.data.items[equippedBodyArmor.id], equippedBodyArmor.count);
                    }
                }

                EquipArmor(inventory.currentItem, draggedItemData);
                slot.GetComponent<Image>().color = Color.white;
            }
        }
    }
    private void OnSlotClick(ItemType type)
    {
        switch (type)
        {
            case ItemType.HeadArmor:
                if (equippedHeadArmor != null)
                    UnequipHeadArmor();
                break;
            case ItemType.BodyArmor:
                if (equippedBodyArmor != null)
                    UnequipBodyArmor();
                break;
        }
    }
    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < inventory.items.Count; i++)
        {
            if (inventory.items[i].id == 0)
            {
                return i;
            }
        }
        return -1;
    }

    private void InitializeSlots()
    {
        headIcon.sprite = emptySlotSprite;
        bodyIcon.sprite = emptySlotSprite;
        headIcon.enabled = true;
        bodyIcon.enabled = true;
        headText.text = "";
        bodyText.text = "";

        SetupSlotTriggers(headSlot, ItemType.HeadArmor);
        SetupSlotTriggers(bodySlot, ItemType.BodyArmor);
    }

    private void UpdateEquipmentUI()
    {
        if (EquippedHeadArmor != null)
        {
            headIcon.sprite = inventory.data.GetItemById(EquippedHeadArmor.id).image;
            headText.text = $"+{inventory.data.GetItemById(EquippedHeadArmor.id).armorValue}";
        }

        if (EquippedBodyArmor != null)
        {
            bodyIcon.sprite = inventory.data.GetItemById(EquippedBodyArmor.id).image;
            bodyText.text = $"+{inventory.data.GetItemById(EquippedBodyArmor.id).armorValue}";
        }
    }

    public ItemInventory EquippedHeadArmor => equippedHeadArmor;
    public ItemInventory EquippedBodyArmor => equippedBodyArmor;
}