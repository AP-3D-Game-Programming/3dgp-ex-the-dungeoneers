using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DungeonMinimap : MonoBehaviour
{
    public AutoDungeonGenerator dungeonGenerator;  // Referentie naar je dungeon generator

    [Header("Minimap Settings")]
    public RawImage minimapImage;    // Verwijzing naar de UI RawImage
    public int textureSize = 256;    // Resolutie van de minimap
    public Color kamerKleur = Color.white;
    public Color gangKleur = Color.gray;
    public Color spelerKleur = Color.green;
    public Color enemyKleur = Color.red;
    public Color chestKleur = Color.yellow;
    public Color bossKamerKleur = Color.magenta;
    public Color safeKamerKleur = Color.blue;
    public Color achtergrondKleur = Color.black;

    private Texture2D minimapTexture;
    private bool isGeïnitialiseerd = false;
    void Start()
    {
        if (!dungeonGenerator)
        {
            Debug.LogError("DungeonMinimap: Geen AutoDungeonGenerator gekoppeld!");
            return;
        }

        // Initialiseer de minimap texture
        minimapTexture = new Texture2D(textureSize, textureSize);
        minimapTexture.filterMode = FilterMode.Point;
        minimapTexture.wrapMode = TextureWrapMode.Clamp;

        // Koppel deze Texture2D aan de minimap RawImage
        if (minimapImage)
        {
            minimapImage.texture = minimapTexture;
        }

        // Teken init minimap (leeg, wachtend op dungeon)
        ClearMinimap();
        isGeïnitialiseerd = true;

        // Zorg ervoor dat de minimap wordt geüpdatet na dungeon generatie
        dungeonGenerator.GenereerDungeon();  // Genereer de dungeon (of trigger vanaf UI)
        UpdateMinimap();  // Update de minimap direct na de eerste generatie
    }

    // Functie om de minimap te wissen
    void ClearMinimap()
    {
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                minimapTexture.SetPixel(x, y, achtergrondKleur);
            }
        }
        minimapTexture.Apply();
    }

    // Functie om de minimap te updaten met de huidige dungeon layout
    public void UpdateMinimap()
    {
        if (!isGeïnitialiseerd || !dungeonGenerator) return;

        // Wis de minimap eerst
        ClearMinimap();

        // Haal kamers en gangen op uit de generator
        List<AutoDungeonGenerator.Kamer> kamers = dungeonGenerator.GetKamers();
        List<AutoDungeonGenerator.Gang> gangen = dungeonGenerator.GetGangen();

        // Bereken schaling: hoeveel pixels per dungeon tile?
        float pixelsPerTileX = (float)textureSize / (dungeonGenerator.maxKamerGrootte * dungeonGenerator.aantalKamers * 2);
        float pixelsPerTileZ = (float)textureSize / (dungeonGenerator.maxKamerGrootte * dungeonGenerator.aantalKamers * 2);

        // Teken alle kamers
        foreach (var kamer in kamers)
        {
            Color fillColor = kamerKleur;  // Normale kamer kleur

            // Pas kleur aan afhankelijk van kamertype
            switch (kamer.type)
            {
                case AutoDungeonGenerator.KamerType.Start:
                    fillColor = spelerKleur;  // De startkamer krijgt de kleur van de speler
                    break;
                case AutoDungeonGenerator.KamerType.Boss:
                    fillColor = bossKamerKleur;
                    break;
                case AutoDungeonGenerator.KamerType.Treasure:
                    fillColor = chestKleur;
                    break;
                case AutoDungeonGenerator.KamerType.Safe:
                    fillColor = safeKamerKleur;
                    break;
            }

            // Bepaal pixel positie en grootte
            int pixelX = (int)(kamer.x * pixelsPerTileX);
            int pixelZ = (int)(kamer.z * pixelsPerTileZ);
            int pixelBreedte = (int)(kamer.breedte * pixelsPerTileX);
            int pixelDiepte = (int)(kamer.diepte * pixelsPerTileZ);

            // Teken de kamer als rechthoek op de minimap
            for (int px = 0; px < pixelBreedte; px++)
            {
                for (int pz = 0; pz < pixelDiepte; pz++)
                {
                    minimapTexture.SetPixel(pixelX + px, pixelZ + pz, fillColor);
                }
            }
        }

        // Teken alle gangen
        foreach (var gang in gangen)
        {
            foreach (var tile in gang.tiles)
            {
                int pixelX = (int)(tile.x * pixelsPerTileX);
                int pixelZ = (int)(tile.y * pixelsPerTileZ);

                // Teken een gang-tile als 1 pixel of een klein vierkant
                minimapTexture.SetPixel(pixelX, pixelZ, gangKleur);
                // Je kunt de gang ook dikker maken als je wilt
                // minimapTexture.SetPixel(pixelX + 1, pixelZ, gangKleur);
                // minimapTexture.SetPixel(pixelX, pixelZ + 1, gangKleur);
            }
        }

        // Teken de enemies als kleine rode pixels
        foreach (var enemy in dungeonGenerator.GetEnemies())
        {
            Vector3 pos = enemy.transform.position;
            int pixelX = (int)(pos.x / dungeonGenerator.tileSize * pixelsPerTileX);
            int pixelZ = (int)(pos.z / dungeonGenerator.tileSize * pixelsPerTileZ);

            // Teken een vijand
            minimapTexture.SetPixel(pixelX, pixelZ, enemyKleur);
        }

        // Teken de chests als gele pixels
        foreach (var chest in dungeonGenerator.GetChests())
        {
            Vector3 pos = chest.transform.position;
            int pixelX = (int)(pos.x / dungeonGenerator.tileSize * pixelsPerTileX);
            int pixelZ = (int)(pos.z / dungeonGenerator.tileSize * pixelsPerTileZ);

            // Teken een chest
            minimapTexture.SetPixel(pixelX, pixelZ, chestKleur);
        }

        // Teken de speler als een groen pixel
        if (dungeonGenerator.spelerInstance != null)
        {
            Vector3 spelerPos = dungeonGenerator.spelerInstance.transform.position;
            int pixelX = (int)(spelerPos.x / dungeonGenerator.tileSize * pixelsPerTileX);
            int pixelZ = (int)(spelerPos.z / dungeonGenerator.tileSize * pixelsPerTileZ);

            minimapTexture.SetPixel(pixelX, pixelZ, spelerKleur);
        }

        // Uiteindelijk de texture toepassen om de wijzigingen zichtbaar te maken
        minimapTexture.Apply();
    }

    // Optioneel: Je kunt bijvoorbeeld de minimap laten updaten om de paar seconden
    void Update()
    {
        // Hier kun je bijvoorbeeld periodiek updaten
        // UpdateMinimap(); // Uncomment als je een continue update wilt (of roep het aan bij veranderingen)
    }
}