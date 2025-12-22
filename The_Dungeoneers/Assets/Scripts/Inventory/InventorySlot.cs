using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

    // Als de muis erop komt:
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Alleen tonen als er echt een item in zit
        if (item != null)
        {
            TooltipManager.instance.Show(item.itemName, item.description);
        }
    }

    // Als de muis eraf gaat:
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.instance.Hide();
    }
}