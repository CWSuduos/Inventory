using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using System.IO;
public class Inventory : MonoBehaviour
{
    [Header("Main Settings")]
    public List<ItemInventory> items = new List<ItemInventory>();
    [SerializeField] private GameObject GameObjShow;
    [SerializeField] private GameObject InventoryMainObject;
    [SerializeField] private int maxCount;

    public RectTransform movingObject;
    [SerializeField] private Vector3 offset;
    [SerializeField] private List<DefaultItem> defaultItems;

    [Header("Item Info Panel")]
    [SerializeField] private GameObject itemInfoPanel;
    [SerializeField] private Image itemInfoImage;
    [SerializeField] private Text itemNameText;
    [SerializeField] private Text itemDescriptionText;
    public DataItem data;
    public int currentId = -1;
    public ItemInventory currentItem;
    private Camera mainCamera;

    private bool isDragging = false;
    private float mouseHoldTimer = 0f;
    private float clickTime = 0f;
    private const float HOLD_TIME = 0.2f;
    private const float CLICK_THRESHOLD = 0.2f;
    private GameObject currentHoveredItem;


    [Header("Item Action Buttons")]
    [SerializeField] private Button actionButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Text actionButtonText;

    private EquipmentSlots equipmentSlots;
    void Start()
    {
        mainCamera = Camera.main;
        saveFilePath = Path.Combine(Application.persistentDataPath, "inventoryData.json");
        equipmentSlots = FindObjectOfType<EquipmentSlots>();
        InitializeInventory();
        LoadOrInitializeInventory();

    }
    private void UpdateSlotUI(int slotIndex, ItemData item, int count)
    {
        if (items[slotIndex].itemGameObj != null)
        {
            Image img = items[slotIndex].itemGameObj.GetComponent<Image>();
            Text txt = items[slotIndex].itemGameObj.GetComponentInChildren<Text>();

            if (item != null)
            {
                img.sprite = item.image;
                img.color = Color.white;
                txt.text = count > 1 ? count.ToString() : "";
            }
            else
            {
                img.sprite = null;
                img.color = Color.clear;
                txt.text = "";
            }
        }
    }

    private void UpdateSlotUI(int slotIndex)
    {
        var item = data.GetItemById(items[slotIndex].id);
        UpdateSlotUI(slotIndex, item, items[slotIndex].count);
    }
    public void AddItem(int slot, ItemData item, int count)
    {
        if (slot < 0 || slot >= items.Count) return;

        items[slot].id = item?.id ?? 0;
        items[slot].count = count;
        UpdateSlotUI(slot, item, count);
    }
    public void ClearSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < items.Count)
        {
            items[slotIndex].id = 0;
            items[slotIndex].count = 0;
            UpdateSlotUI(slotIndex, null, 0);
        }
    }
    public void AddInventoryItem(int id, ItemInventory invItem)
    {
        items[id].id = invItem.id;
        items[id].count = invItem.count;

        ItemData itemData = data.GetItemById(invItem.id);
        UpdateSlotUI(id, itemData, invItem.count);
    }



    private void ShowItemInfo(ItemInventory inventoryItem)
    {
        ItemData itemData = data.GetItemById(inventoryItem.id);
        if (itemData != null)
        {
            itemInfoImage.sprite = itemData.image;
            itemNameText.text = itemData.name;

            StringBuilder description = new StringBuilder();


            if (itemData.isStackable)
            {
                description.AppendLine($"Количество: {inventoryItem.count}/{itemData.maxCountForItem}");
            }


            switch (itemData.itemType)
            {
                case ItemType.Consumable:
                    description.AppendLine("Тип: Расходуемый предмет");
                    description.AppendLine($"Вес одного: {itemData.weight:F1}");
                    if (inventoryItem.count > 1)
                    {
                        description.AppendLine($"Общий вес: {itemData.weight * inventoryItem.count:F1}");
                    }
                    SetupActionButton("Купить", () => {
                        FillStack(inventoryItem);
                        HideItemInfo();
                    });
                    break;

                case ItemType.HeadArmor:
                case ItemType.BodyArmor:
                    string armorType = itemData.itemType == ItemType.HeadArmor ? "Защита головы" : "Защита тела";
                    description.AppendLine($"Тип: {armorType}");
                    description.AppendLine($"Показатель брони: {itemData.armorValue}");
                    description.AppendLine($"Вес: {itemData.weight:F1}");
                    SetupActionButton("Экипировать", () => {
                        EquipItem(inventoryItem);
                        HideItemInfo();
                    });
                    break;

                case ItemType.HealingItem:
                    description.AppendLine("Тип: Лечебное зелье");
                    description.AppendLine($"Лечение: +{itemData.healAmount} HP");
                    description.AppendLine($"Вес одного: {itemData.weight:F1}");
                    if (inventoryItem.count > 1)
                    {
                        description.AppendLine($"Общий вес: {itemData.weight * inventoryItem.count:F1}");
                    }
                    SetupActionButton("Лечить", () => {
                        UseHealingItem(inventoryItem);
                        HideItemInfo();
                    });
                    break;
            }

            itemDescriptionText.text = description.ToString();
            itemInfoPanel.SetActive(true);


            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(() => {
                    DeleteItem(inventoryItem);
                    HideItemInfo();
                });
                deleteButton.gameObject.SetActive(true);
            }
        }
    }

    private void ResetSelection()
    {
        currentId = -1;
        if (movingObject != null)
        {
            movingObject.gameObject.SetActive(false);
        }
    }

    private void HideItemInfo()
    {
        itemInfoPanel.SetActive(false);

        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
        }
    }



    private void SetupActionButton(string buttonText, UnityAction action)
    {
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(true);
            actionButtonText.text = buttonText;
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => {
                action?.Invoke();
                ResetSelection();
            });
        }
    }


    private void FillStack(ItemInventory inventoryItem)
    {
        ItemData itemData = data.GetItemById(inventoryItem.id);
        if (itemData != null)
        {
            inventoryItem.count = itemData.maxCountForItem;
            UpdateItemUI(inventoryItem);
            Debug.Log($"Стак предмета {itemData.name} заполнен до максимума ({itemData.maxCountForItem})");
        }
    }
    [SerializeField] private PlayerHealth playerHealth;

    private void UseHealingItem(ItemInventory inventoryItem)
    {
        ItemData itemData = data.GetItemById(inventoryItem.id);

        if (itemData != null && inventoryItem.count > 0)
        {
            Debug.Log($"Использована аптечка. Восстановлено {itemData.healAmount} HP");
            inventoryItem.count--;
            playerHealth.Heal(50);
            playerHealth.UpdateHealthUI();
            if (inventoryItem.count <= 0)
            {
                DeleteItem(inventoryItem);

            }
            else
            {
                UpdateItemUI(inventoryItem);
            }
        }
    }



    private void DeleteItem(ItemInventory inventoryItem)
    {
        int slotIndex = items.IndexOf(inventoryItem);
        if (slotIndex != -1)
        {
            ClearSlot(slotIndex);
            CloseItemInfo();
            Debug.Log("Предмет удален");
        }
    }

    private void UpdateItemUI(ItemInventory inventoryItem)
    {
        if (inventoryItem.count > 1)
        {
            inventoryItem.itemGameObj.GetComponentInChildren<Text>().text = inventoryItem.count.ToString();
        }
        else
        {
            inventoryItem.itemGameObj.GetComponentInChildren<Text>().text = "";
        }
    }

    private void CloseItemInfo()
    {
        itemInfoPanel.SetActive(false);
        if (actionButton != null) actionButton.gameObject.SetActive(false);
        if (deleteButton != null) deleteButton.gameObject.SetActive(false);
    }


    public void RemoveItemsById(int itemId, int amountToRemove)
    {
        if (amountToRemove <= 0) return;

        int remainingToRemove = amountToRemove;

        for (int i = 0; i < items.Count && remainingToRemove > 0; i++)
        {
            if (items[i].id == itemId)
            {
                if (items[i].count <= remainingToRemove)
                {
                    remainingToRemove -= items[i].count;
                    ClearSlot(i);
                }
                else
                {
                    items[i].count -= remainingToRemove;
                    UpdateSlotUI(i);
                    remainingToRemove = 0;
                }
            }
        }

        Debug.Log($"Удалено {amountToRemove - remainingToRemove} предметов с ID {itemId}");
        if (remainingToRemove > 0)
        {
            Debug.LogWarning($"Не удалось удалить {remainingToRemove} предметов, их недостаточно ");
        }
    }



    public int GetTotalItemCount(int itemId)
    {
        int total = 0;
        foreach (var item in items)
        {
            if (item.id == itemId)
            {
                total += item.count;
            }
        }
        return total;
    }


    public bool CanRemoveItems(int itemId, int amount)
    {
        return GetTotalItemCount(itemId) >= amount;
    }


    public void OnRemoveButtonClick(int itemId, int amount)
    {
        if (CanRemoveItems(itemId, amount))
        {
            RemoveItemsById(itemId, amount);
        }
        else
        {
            Debug.LogWarning("Недостаточно предметов для удаления!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickTime = Time.time;


            if (itemInfoPanel.activeSelf)
            {
                Vector2 mousePosition = Input.mousePosition;
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                bool clickedOnPanel = false;
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.transform.IsChildOf(itemInfoPanel.transform))
                    {
                        clickedOnPanel = true;
                        break;
                    }
                }

                if (!clickedOnPanel)
                {
                    CloseItemInfo();
                }
            }
        }

        if (Input.GetMouseButton(0) && currentHoveredItem != null)
        {
            if (!isDragging)
            {
                mouseHoldTimer += Time.deltaTime;
                if (mouseHoldTimer >= HOLD_TIME)
                {
                    StartDragging();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            float holdDuration = Time.time - clickTime;

            if (holdDuration < CLICK_THRESHOLD && currentHoveredItem != null)
            {
                HandleItemClick();
            }

            if (isDragging && currentId != -1)
            {
                EndDragging();
            }
            ResetDragState();
        }

        if (currentId != -1)
        {
            MoveObject();
        }
    }

    private void HandleItemClick()
    {
        if (currentHoveredItem != null)
        {
            int slotId = int.Parse(currentHoveredItem.name);
            ItemInventory clickedItem = items[slotId];

            if (clickedItem.id != 0)
            {
                ShowItemInfo(clickedItem);
            }
        }
    }




    private void StartDragging()
    {
        if (currentHoveredItem != null && !isDragging)
        {
            int slotId = int.Parse(currentHoveredItem.name);
            if (items[slotId].id == 0) return;

            isDragging = true;
            currentId = slotId;
            currentItem = CopyInventoryItem(items[currentId]);
            movingObject.gameObject.SetActive(true);
            movingObject.GetComponent<Image>().sprite = data.items[currentItem.id].image;
            AddItem(currentId, data.items[0], 0);
        }
    }

    private void EndDragging()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        bool itemPlaced = false;

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.transform.parent == InventoryMainObject.transform)
            {
                int targetId = int.Parse(result.gameObject.name);

                if (targetId == currentId)
                {
                    AddInventoryItem(currentId, currentItem);
                    itemPlaced = true;
                    break;
                }

                ItemInventory targetItem = items[targetId];


                ItemData itemInfo = data.GetItemById(currentItem.id);
                if (itemInfo == null) continue;

                if (targetItem.id == 0)
                {
                    AddInventoryItem(targetId, currentItem);
                    itemPlaced = true;
                    break;
                }
                else if (currentItem.id != targetItem.id)
                {
                    AddInventoryItem(currentId, targetItem);
                    AddInventoryItem(targetId, currentItem);
                    itemPlaced = true;
                    break;
                }
                else if (itemInfo.isStackable)
                {
                    int maxCount = itemInfo.maxCountForItem;
                    if (targetItem.count + currentItem.count <= maxCount)
                    {
                        targetItem.count += currentItem.count;
                        targetItem.itemGameObj.GetComponentInChildren<Text>().text = targetItem.count.ToString();
                        itemPlaced = true;
                        break;
                    }
                    else
                    {
                        AddItem(currentId, data.items[targetItem.id], targetItem.count + currentItem.count - maxCount);
                        targetItem.count = maxCount;
                        targetItem.itemGameObj.GetComponentInChildren<Text>().text = targetItem.count.ToString();
                        itemPlaced = true;
                        break;
                    }
                }
                else
                {
                    AddInventoryItem(currentId, targetItem);
                    AddInventoryItem(targetId, currentItem);
                    itemPlaced = true;
                    break;
                }
            }
        }

        if (!itemPlaced)
        {
            AddInventoryItem(currentId, currentItem);
        }

        currentId = -1;
        movingObject.gameObject.SetActive(false);
        isDragging = false;
    }






    private void ResetDragState()
    {
        isDragging = false;
        mouseHoldTimer = 0f;
        currentHoveredItem = null;
    }

    private void MoveObject()
    {
        Vector3 pos = Input.mousePosition + offset;
        pos.z = InventoryMainObject.GetComponent<RectTransform>().position.z;
        movingObject.position = mainCamera.ScreenToWorldPoint(pos);
    }

    public void OnPointerEnter(GameObject item)
    {
        if (!isDragging)
        {
            currentHoveredItem = item;
        }
    }

    public void OnPointerExit()
    {
        if (!isDragging)
        {
            currentHoveredItem = null;
            ResetDragState();
        }
    }

    private void AddGraphics()
    {
        for (int i = 0; i < maxCount; i++)
        {
            GameObject newItem = Instantiate(GameObjShow, InventoryMainObject.transform);
            newItem.name = i.ToString();

            EventTrigger trigger = newItem.AddComponent<EventTrigger>();

            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener((data) => OnPointerEnter(newItem));
            trigger.triggers.Add(enterEntry);

            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener((data) => OnPointerExit());
            trigger.triggers.Add(exitEntry);

            ItemInventory ii = new ItemInventory { itemGameObj = newItem };
            RectTransform rect = newItem.GetComponent<RectTransform>();
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;
            newItem.GetComponentInChildren<RectTransform>().localScale = Vector3.one;
            items.Add(ii);
        }
    }
    public void CountAllItemsDetailed()
    {
        Dictionary<int, int> itemCounts = new Dictionary<int, int>();


        foreach (ItemInventory item in items)
        {
            if (item.id != 0)
            {
                if (itemCounts.ContainsKey(item.id))
                {
                    itemCounts[item.id] += item.count;
                }
                else
                {
                    itemCounts[item.id] = item.count;
                }
            }
        }

        foreach (var itemCount in itemCounts)
        {
            ItemData itemInfo = data.GetItemById(itemCount.Key);
            if (itemInfo != null)
            {
                string stackInfo = itemInfo.isStackable ?
                    $" (Макс. в стаке: {itemInfo.maxCountForItem})" :
                    " (Нестакаемый)";

                Debug.Log($"Предмет: {itemInfo.name}\n" +
                         $"ID: {itemCount.Key}\n" +
                         $"Тип: {itemInfo.itemType}\n" +
                         $"Общее количество: {itemCount.Value}{stackInfo}\n" +
                         $"Вес одного: {itemInfo.weight}\n" +
                         $"Общий вес: {itemCount.Value * itemInfo.weight}\n" +
                         "------------------------");
            }
        }
    }


    private void EquipItem(ItemInventory inventoryItem)
    {
        ItemData itemData = data.GetItemById(inventoryItem.id);
        if (itemData != null)
        {
            equipmentSlots.EquipArmor(inventoryItem, itemData);

            int slotIndex = items.IndexOf(inventoryItem);
            if (slotIndex != -1)
            {
                ClearSlot(slotIndex);
            }
            CloseItemInfo();
        }
    }

    public void AddItemToInventory(ItemInventory item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == 0)
            {
                items[i].id = item.id;
                items[i].count = item.count;
                items[i].itemGameObj.GetComponent<Image>().sprite = data.GetItemById(item.id).image;
                items[i].itemGameObj.GetComponentInChildren<Text>().text =
                    item.count > 1 ? item.count.ToString() : "";
                break;
            }
        }
    }
    public int FindFreeSlot()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == 0)
            {
                return i;
            }
        }
        return -1;
    }
    private void InitializeInventory()
    {

        for (int i = 0; i < maxCount; i++)
        {
            if (i >= items.Count)
            {
                items.Add(new ItemInventory());
            }
        }
    }
    private ItemInventory CopyInventoryItem(ItemInventory old)
    {
        return new ItemInventory
        {
            id = old.id,
            itemGameObj = old.itemGameObj,
            count = old.count
        };
    }
    public void RemoveItem(int itemId)
    {
        ItemData itemData = data.GetItemById(itemId);
        if (itemData != null && itemData.itemType == ItemType.Consumable)
        {
            bool itemFound = false;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].id == itemId)
                {
                    if (items[i].count >= itemData.removeAmount)
                    {
                        items[i].count -= itemData.removeAmount;
                        itemFound = true;


                        if (items[i].count > 0)
                        {
                            items[i].itemGameObj.GetComponentInChildren<Text>().text = items[i].count.ToString();
                        }
                        else
                        {

                            ClearSlot(i);
                        }
                        Debug.Log($"Удалено {itemData.removeAmount} предметов с ID {itemId}");
                        break;
                    }
                }
            }

            if (!itemFound)
            {
                Debug.LogWarning($"Недостаточно предметов с ID {itemId} для удаления");
            }
        }
    }
    private string saveFilePath;

    void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/inventory.json";
        LoadInventory();
    }

    void OnApplicationQuit()
    {

        SaveInventory();
    }

    public void SaveInventory()
    {
        InventoryData data = new InventoryData
        {
            items = items
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Инвентарь сохранён!");
    }

    public void LoadInventory()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            InventoryData data = JsonUtility.FromJson<InventoryData>(json);

            items = data.items;


            foreach (var item in items)
            {
                if (item.id != 0)
                {
                    ItemData itemData = this.data.GetItemById(item.id);
                    if (itemData != null)
                    {
                        AddItem(items.IndexOf(item), itemData, item.count);
                    }
                }
            }

            Debug.Log("Инвентарь загружен!");
        }
        else
        {
            Debug.Log("Файл сохранения инвентаря не найден. Создаём новый инвентарь.");
        }
    }


    private void InitializeDefaultItems()
    {
        for (int i = 0; i < maxCount; i++)
        {
            if (i >= items.Count || items[i].id == 0)
            {
                DefaultItem defaultItem = defaultItems.Find(item => item.slot == i);
                if (defaultItem != null)
                {
                    ItemData itemData = data.GetItemById(defaultItem.itemId);
                    if (itemData != null)
                    {
                        if (i >= items.Count)
                        {
                            items.Add(new ItemInventory());
                        }

                        GameObject slotGameObject = Instantiate(GameObjShow, InventoryMainObject.transform);
                        Image slotImage = slotGameObject.GetComponent<Image>();

                        if (slotImage != null)
                        {
                            slotImage.sprite = itemData.image;
                            slotImage.color = Color.white;
                        }

                        items[i] = new ItemInventory
                        {
                            id = defaultItem.itemId,
                            count = defaultItem.count,
                            itemGameObj = slotGameObject
                        };

                        UpdateSlotUI(i, itemData, defaultItem.count);
                    }
                    else
                    {
                        Debug.LogWarning($"ItemData not found for itemId {defaultItem.itemId} in defaultItems");
                    }
                }
                else
                {
                    Debug.LogWarning($"No default item defined for slot {i}");
                }
            }
        }
    }
    private void LoadOrInitializeInventory()
    {

        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            List<ItemInventory> loadedItems = JsonUtility.FromJson<InventoryData>(json).items;

            for (int i = 0; i < loadedItems.Count && i < maxCount; i++)
            {
                items[i] = loadedItems[i];
                UpdateSlotUI(i, data.GetItemById(items[i].id), items[i].count);
            }
        }


        InitializeDefaultItems();
    }

    public void SaveInventoryToJSON()
    {
        List<ItemInventorySaveData> saveData = new List<ItemInventorySaveData>();
        foreach (var item in items)
        {
            saveData.Add(new ItemInventorySaveData(item));
        }


        string json = JsonUtility.ToJson(new InventorySaveData { items = saveData }, true);


        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Инвентарь сохранён в {saveFilePath}");
    }
    private void LoadInventoryFromJSON()
    {
        Debug.Log("Загрузка инвентаря из JSON...");
        string json = File.ReadAllText(saveFilePath);


        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);


        foreach (var savedItem in saveData.items)
        {
            ItemData itemData = data.GetItemById(savedItem.id);
            AddItem(savedItem.slot, itemData, savedItem.count);
        }
    }
}


[System.Serializable]
public class ItemInventory
{
    public int id;
    public GameObject itemGameObj;
    public int count;
    public int slot;
}

[System.Serializable]
public class InventorySaveData
{
    public List<ItemInventorySaveData> items;
}

[System.Serializable]
public class ItemInventorySaveData
{
    public int slot;
    public int id;
    public int count;

    public ItemInventorySaveData(ItemInventory item)
    {
        slot = item.slot;
        id = item.id;
        count = item.count;
    }
}
