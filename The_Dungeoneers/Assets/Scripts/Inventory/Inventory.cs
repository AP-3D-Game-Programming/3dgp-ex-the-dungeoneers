using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // Singleton pattern (zodat we er overal bij kunnen)
    public static Inventory instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Meer dan 1 inventory gevonden!");
            return;
        }
        instance = this;
    }

    // Dit is een 'Event': De UI luistert hiernaar. 
    // Als er iets verandert, roept dit script: "Hé UI, update jezelf!"
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public int space = 5; // Hoeveel vakjes heb je?
    public List<ItemData> items = new List<ItemData>();

    // Functie om items toe te voegen
    public bool Add(ItemData item)
    {
        if (items.Count >= space)
        {
            Debug.Log("Inventory zit vol!");
            return false;
        }

        items.Add(item);

        // Roep de UI aan als die luistert
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();

        return true;
    }

    // Functie om items te verwijderen
    public void Remove(ItemData item)
    {
        items.Remove(item);

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
}