using UnityEngine;

public class LootChest : MonoBehaviour
{
    [Header("Wat zit er in?")]
    public ItemData itemInChest; 

    [Header("Status")]
    public bool isOpened = false;
    public float interactRange = 3f;

    [Header("Effects")]
    public GameObject floatingTextPrefab; // <--- NIEUW: Sleep hier je prefab in

    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void OnMouseDown()
    {
        if (isOpened) return;

        if (player != null && Vector3.Distance(transform.position, player.position) > interactRange)
        {
            Debug.Log("Te ver weg!");
            return;
        }

        bool succes = Inventory.instance.Add(itemInChest);

        if (succes)
        {
            // NIEUW: Zwevende tekst tonen
            if (floatingTextPrefab != null)
            {
                // We spawnen de tekst iets boven de kist (Vector3.up)
                GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity);
                
                // Groene tekst met "+ Naam van item"
                ft.GetComponent<FloatingText>().SetText("+ " + itemInChest.itemName, Color.green);
            }

            OpenChest();
        }
        else
        {
            Debug.Log("Inventory zit vol!");
            // TIP: Je zou hier RODE tekst kunnen tonen: "Inventory Vol!"
             if (floatingTextPrefab != null)
            {
                GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity);
                ft.GetComponent<FloatingText>().SetText("Inventory Full!", Color.red);
            }
        }
    }

    void OpenChest()
    {
        isOpened = true;
        Debug.Log("Je hebt gevonden: " + itemInChest.name);
        GetComponent<Renderer>().material.color = Color.gray; 
    }
}