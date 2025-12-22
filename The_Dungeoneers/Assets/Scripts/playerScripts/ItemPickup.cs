using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData item;   // Welk item is dit? (Wordt ingevuld door de Enemy)
    public float pickupRange = 2f; // Hoe dichtbij moet je staan?

    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        // Als er geen item in zit, vernietig dit object dan (veiligheid)
        if (item == null) 
        {
            Destroy(gameObject);
            return;
        }

        // Check afstand tot speler
        if (player != null && Vector3.Distance(transform.position, player.position) <= pickupRange)
        {
            // Als speler op 'E' drukt
            if (Input.GetKeyDown(KeyCode.E))
            {
                PickUp();
            }
        }
    }

    void PickUp()
    {
        // Probeer toe te voegen aan inventory
        bool wasPickedUp = Inventory.instance.Add(item);

        if (wasPickedUp)
        {
            Debug.Log("Opgeraapt: " + item.itemName);
            
            // Vernietig het 3D object in de wereld
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventory vol!");
        }
    }
    
    // Optioneel: Teken een rondje in de editor om de range te zien
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}