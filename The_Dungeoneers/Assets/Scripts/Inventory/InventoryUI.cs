using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform itemsParent;
    public GameObject inventoryUI; // <-- NIEUW: Verwijzing naar het paneel

    Inventory inventory;
    InventorySlot[] slots;

    void Start()
    {
        inventory = Inventory.instance;
        inventory.onItemChangedCallback += UpdateUI;

        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
        
        // Zorg dat de UI start zoals je hem in de inspector hebt ingesteld (aan of uit)
        // Of forceer hem uit bij start:
        // inventoryUI.SetActive(false); 
        
        UpdateUI();
    }

    // <-- NIEUW: Update loop om te luisteren naar toetsen
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
        {
            // Dit is een 'toggle': Als hij aan staat gaat hij uit, en andersom.
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }
    }

    void UpdateUI()
    {
        // ... (de rest van je code blijft hetzelfde)
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count) slots[i].AddItem(inventory.items[i]);
            else slots[i].ClearSlot();
        }
    }
}