using UnityEngine;

[CreateAssetMenu(fileName = "Nieuw Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon = null; // Het plaatje voor in de UI
    public bool isStackable = false; // Kun je ze stapelen?
    
    // NIEUW: [TextArea] zorgt voor een groter invulvakje in de Inspector
    [TextArea(4, 4)] 
    public string description;
    
    // 'virtual' betekent: Andere scripts mogen deze functie overschrijven (aanpassen)
    public virtual void Use()
    {
        Debug.Log("Item gebruikt: " + itemName);
    }

    

}