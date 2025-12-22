using UnityEngine;

[CreateAssetMenu(fileName = "Nieuw Wapen", menuName = "Inventory/Equipment")]
public class EquipmentItem : ItemData
{
    public int damageBonus = 10;

    public override void Use()
    {
        base.Use();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerEquipment manager = player.GetComponent<PlayerEquipment>();
            if (manager != null)
            {
                // DE LOGICA:
                // Als we DIT item al vast hebben...
                if (manager.currentEquippedItem == this)
                {
                    // ... dan doen we hem af (Unequip)
                    manager.UnequipWeapon();
                }
                else
                {
                    // ... anders doen we hem aan (Equip)
                    manager.EquipWeapon(this);
                }
            }
        }
    }
}