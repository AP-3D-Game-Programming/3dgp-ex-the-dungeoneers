using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    // OUDE VERSIE: public List<ItemData> items = new List<ItemData>();
    // NIEUWE VERSIE:
    public List<InventoryItem> items = new List<InventoryItem>();
    
    public int space = 5;

    // Delegate voor update
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Inventory found!");
            return;
        }
        instance = this;
    }

    public bool Add(ItemData item)
    {
        // 1. Check of het item stackable is
        if (item.isStackable)
        {
            // Zoek of we al een stapel hebben DIE NOG NIET VOL IS
            foreach (InventoryItem i in items)
            {
                // We checken nu: Is het hetzelfde item? EN is er nog plek in de stapel?
                if (i.data == item && i.stackSize < item.maxStackSize)
                {
                    i.AddToStack();
                    if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
                    return true;
                }
            }
        }

        // 2. Als we hier komen, is er geen stapel gevonden met ruimte.
        // We moeten een NIEUW slot vullen. Is er nog plek in de tas?
        if (items.Count >= space)
        {
            Debug.Log("Inventory is helemaal vol (ook geen ruimte voor nieuwe stapels).");
            return false;
        }

        // 3. Maak een nieuwe stapel aan in een leeg slot
        items.Add(new InventoryItem(item));

        if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
        
        return true;
    }

    public void Remove(ItemData item)
    {
        // We moeten het juiste inventory-item zoeken
        InventoryItem itemToRemove = null;

        foreach (InventoryItem i in items)
        {
            if (i.data == item)
            {
                itemToRemove = i;
                break;
            }
        }

        if (itemToRemove != null)
        {
            // Als het stackable is, verlaag aantal
            if (itemToRemove.data.isStackable)
            {
                itemToRemove.RemoveFromStack();
                
                // Als aantal 0 is, gooi het hele slot weg
                if (itemToRemove.stackSize <= 0)
                {
                    items.Remove(itemToRemove);
                }
            }
            else
            {
                // Niet stackable? Gewoon weg ermee
                items.Remove(itemToRemove);
            }

            if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
        }
    }
}

[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int stackSize;

    public InventoryItem(ItemData item)
    {
        data = item;
        stackSize = 1;
    }

    public void AddToStack()
    {
        stackSize++;
    }

    public void RemoveFromStack()
    {
        stackSize--;
    }
}