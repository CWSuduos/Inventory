using UnityEngine;
using UnityEngine.UI;

public class ItemRemover : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button consumableButton1;
    [SerializeField] private Button consumableButton2;
    [SerializeField] private Button removeButton;
    [SerializeField] private Image outline1;
    [SerializeField] private Image outline2;
    [SerializeField] private Color selectedColor = Color.green;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Inventory inventory;

    private int selectedItemId = -1;

    private void Start()
    {
        inventory = FindObjectOfType<Inventory>();

        consumableButton1.onClick.AddListener(() => SelectConsumable(1));
        consumableButton2.onClick.AddListener(() => SelectConsumable(2));
        removeButton.onClick.AddListener(OnRemoveButtonClicked);

        outline1.enabled = false;
        outline2.enabled = false;
    }

    private void SelectConsumable(int buttonNumber)
    {
        outline1.enabled = false;
        outline2.enabled = false;

        selectedItemId = buttonNumber switch
        {
            1 => 1,
            2 => 2,
            _ => -1
        };

        if (selectedItemId == 1) outline1.enabled = true;
        else if (selectedItemId == 2) outline2.enabled = true;
    }

    private void OnRemoveButtonClicked()
    {
         if (playerHealth != null)
         {
             int randomDamage = Random.Range(5, 16);
             playerHealth.TakeDamage(randomDamage);
             Debug.Log($"<color=red>Игрок получил {randomDamage} урона</color>");
         }
         else
         {
             Debug.LogError("PlayerHealth reference is missing!");
         }


         if (enemyHealth != null)
         {
             if (selectedItemId != -1)
             {
                 int enemyDamage = 0;
                 switch (selectedItemId)
                 {
                     case 1:
                         enemyDamage = 5;
                         break;
                     case 2:
                         enemyDamage = 9;
                         break;
                     default:
                         Debug.LogWarning($"Unknown item ID: {selectedItemId}");
                         break;
                 }

                 if (enemyDamage > 0)
                 {
                     int previousHealth = enemyHealth.currentHealth;
                     enemyHealth.TakeDamage(enemyDamage);

                 }
             }
             else
             {
                 Debug.LogWarning("Предмет не выбран!");
             }
         }
         else
         {
             Debug.LogError("EnemyHealth reference is missing!");
         }
        if (selectedItemId != -1)
        {
            ItemData itemData = inventory.data.GetItemById(selectedItemId);
            if (itemData != null)
            {
                inventory.RemoveItem(selectedItemId);
                Debug.Log($"Предмет {itemData.name} удален");
            }
        }

        // Сброс выбора
        selectedItemId = -1;
        outline1.enabled = false;
        outline2.enabled = false;
    }
}