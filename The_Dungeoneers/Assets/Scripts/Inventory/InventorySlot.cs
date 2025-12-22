using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Vergeet deze niet!

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public Button removeButton;
    public TextMeshProUGUI amountText; // <--- NIEUW: Sleep hier je tekst in

    ItemData item;

    // Pas de AddItem functie aan zodat hij ook het aantal (amount) ontvangt
    public void AddItem(ItemData newItem, int amount)
    {
        item = newItem;

        icon.sprite = item.icon;
        icon.enabled = true;
        removeButton.interactable = true;

        // LOGICA VOOR HET GETALLETJE:
        // Als stackable is EN aantal > 1 -> Toon tekst
        if (item.isStackable && amount > 1)
        {
            amountText.text = amount.ToString();
            amountText.enabled = true;
        }
        else
        {
            amountText.enabled = false;
        }
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false;
        removeButton.interactable = false;
        
        // Verberg de tekst
        if (amountText != null) amountText.enabled = false;
    }
    
    // ... je andere functies (OnRemoveButton, OnUseButton, Tooltip) blijven hetzelfde ...
    // ... Let op: bij OnRemoveButton roep je Inventory.instance.Remove(item) aan, dat blijft werken.

    
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