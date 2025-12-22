using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;       // Sleep hier het Image child in (in de prefab)
    public Button removeButton; // De knop zelf

    ItemData item;

    public void AddItem(ItemData newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
        icon.enabled = true;
    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
    }
    
    // Deze functie koppel je later aan de Button OnClick in de inspector
    public void OnRemoveButton()
    {
        if (item != null)
        {
            Inventory.instance.Remove(item);
        }
    }
    // Koppel deze functie straks aan de knop op het Icoon
    public void OnUseButton()
    {
        if (item != null)
        {
            item.Use();
        }
    }
}