using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject currentWeaponModel;

    [Header("Stats")]
    public int baseDamage = 5;
    public int currentDamage;

    // NIEUW: We onthouden welk item we nu dragen
    public EquipmentItem currentEquippedItem; 

    void Start()
    {
        // Begin met basis damage en lege handen
        currentDamage = baseDamage;
        if (currentWeaponModel != null) currentWeaponModel.SetActive(false);
    }

    public void EquipWeapon(EquipmentItem weapon)
    {
        // Bewaar welk wapen we nu hebben
        currentEquippedItem = weapon;

        // Stats update
        currentDamage = baseDamage + weapon.damageBonus;

        // Model aan
        if (currentWeaponModel != null)
        {
            currentWeaponModel.SetActive(true);
            Debug.Log("Equipped: " + weapon.itemName);
        }
    }

    // NIEUW: Functie om wapen weg te doen
    public void UnequipWeapon()
    {
        // Vergeet het item
        currentEquippedItem = null;

        // Reset damage naar vuist-damage
        currentDamage = baseDamage;

        // Model uit
        if (currentWeaponModel != null)
        {
            currentWeaponModel.SetActive(false);
            Debug.Log("Unequipped weapon. Back to fists.");
        }
    }
}