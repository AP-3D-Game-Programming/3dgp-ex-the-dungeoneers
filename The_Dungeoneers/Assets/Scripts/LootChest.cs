using UnityEngine;

public class LootChest : MonoBehaviour
{
    [Header("Wat zit er in?")]
    public ItemData itemInChest; // Sleep je Item hierin (via Inspector)

    [Header("Status")]
    public bool isOpened = false;
    public float interactRange = 3f; // Hoe dichtbij moet je zijn?

    private Transform player;

    void Start()
    {
        // We zoeken de speler om de afstand te checken
        // ZORG DAT JE SPELER DE TAG "Player" HEEFT!
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void OnMouseDown()
    {
        // 1. Als hij al open is, doe niets
        if (isOpened) return;

        // 2. Check afstand (zodat je niet van 100 meter afstand kan klikken)
        if (player != null && Vector3.Distance(transform.position, player.position) > interactRange)
        {
            Debug.Log("Te ver weg!");
            return;
        }

        // 3. Probeer het item aan de inventory te geven
        // .Add() geeft 'true' terug als het lukt, en 'false' als de tas vol is.
        bool succes = Inventory.instance.Add(itemInChest);

        if (succes)
        {
            OpenChest();
        }
        else
        {
            Debug.Log("Inventory zit vol! Maak eerst ruimte.");
            // Hier zou je later een tekstje op het scherm kunnen tonen
        }
    }

    void OpenChest()
    {
        isOpened = true;
        Debug.Log("Je hebt gevonden: " + itemInChest.name);

        // VISUELE FEEDBACK:
        // Verander de kleur naar grijs of speel een animatie
        GetComponent<Renderer>().material.color = Color.white; 
        
        // Of als je een animatie hebt:
        // GetComponent<Animator>().SetTrigger("Open");
    }
}