using UnityEngine;

[CreateAssetMenu(fileName = "Nieuw Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon = null;
    public bool isStackable = false;
    
    // NIEUW: De limiet per slot (standaard op 10, pas aan in inspector)
    public int maxStackSize = 10; 

    [TextArea(4, 4)]
    public string description;

    public virtual void Use()
    {
        Debug.Log("Item gebruikt: " + itemName);
    }
}