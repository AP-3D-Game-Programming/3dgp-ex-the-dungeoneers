using UnityEngine;

[CreateAssetMenu(fileName = "Nieuw Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon = null; // Het plaatje voor in de UI
    public bool isStackable = false; // Kun je ze stapelen?
}