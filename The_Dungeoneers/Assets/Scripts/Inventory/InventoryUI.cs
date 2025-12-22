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
        // Let op: slots[] array blijft hetzelfde
        // Maar we vragen nu de nieuwe lijst op
        
        // We itereren tot de limiet van OF de slots OF de items
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                // We hebben een item voor dit slot!
                // Pak het item EN het aantal uit de InventoryItem class
                slots[i].AddItem(inventory.items[i].data, inventory.items[i].stackSize);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
}