using UnityEngine;

// Let op: We maken hier een nieuw menu-item voor aan!
[CreateAssetMenu(fileName = "Nieuwe Potion", menuName = "Inventory/Consumable")]
public class ConsumableItem : ItemData // <-- Let op: erft van ItemData, niet MonoBehaviour!
{
    public int healthGain = 20; // Hoeveel levens krijg je erbij?

    // Hier 'overschrijven' we de standaard Use() functie
    public override void Use()
    {
        base.Use(); // Roept de Debug.Log aan uit ItemData

        // 1. Zoek de speler (Zorg dat je speler de tag "Player" heeft!)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 2. Zoek het Health script op de speler
            // Pas 'Health' aan naar hoe jouw levens-script heet!
            Health playerHealth = player.GetComponent<Health>();
            
            if (playerHealth != null)
            {
                playerHealth.Heal(healthGain); // We gaan deze functie zo maken
                
                // 3. Verwijder het item uit de inventory na gebruik
                Inventory.instance.Remove(this);
            }
        }
    }
}