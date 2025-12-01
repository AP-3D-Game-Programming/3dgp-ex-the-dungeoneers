using UnityEngine;

public class AutoDungeonGenerator : MonoBehaviour
{
    public GameObject muurPrefab; 
    public GameObject vloerPrefab; //4x4
    public float tileSize = 4f; // dit is de grote van de tile

    // Om bij te houden wat we gemaakt hebben
    private GameObject dungeonParent;

    void Start()
    {
        genereerDungeon();
    }
    [ContextMenu("genereer dungeon")]
    public void genereerDungeon()
    {
        // Maak een parent object om alles onder te zetten
        dungeonParent = new GameObject("Dungeon");

        // Maakt dungeon
        MaakKamer(0, 0, 10, 10);
    }
    [ContextMenu("verwijder dungeon")]
    public void deleteDungeon()
    {
        // Verwijder de hele dungeon
        if (dungeonParent != null)
        {
            DestroyImmediate(dungeonParent); // Voor Editor
            // Of: Destroy(dungeonParent); // Voor Play mode
        }
    }

    void MaakKamer(int startX, int startZ, int breedte, int diepte)
    {
        float tileSize = 4f;

        // Maak alle vloeren
        for (int x = startX; x < startX + breedte; x++)
        {
            for (int z = startZ; z < startZ + diepte; z++)
            {
                Vector3 positie = new Vector3(x * tileSize, 0, z * tileSize);
                GameObject vloer = Instantiate(vloerPrefab, positie, Quaternion.identity);
                vloer.transform.parent = dungeonParent.transform;
            }
        }

        // Maak muren RONDOM de vloeren
        // Linker en rechter muur (verticaal)
        for (int z = startZ; z < startZ + diepte; z++)
        {
            // Linker muur
            Vector3 posLinks = new Vector3(startX * tileSize, 0, z * tileSize);
            GameObject muurLinks = Instantiate(muurPrefab, posLinks, Quaternion.Euler(0, 90, 0));
            muurLinks.transform.parent = dungeonParent.transform;

            // Rechter muur
            Vector3 posRechts = new Vector3((startX + breedte) * tileSize, 0, z * tileSize);
            GameObject muurRechts = Instantiate(muurPrefab, posRechts, Quaternion.Euler(0, 90, 0));
            muurRechts.transform.parent = dungeonParent.transform;
        }

        // Voor en achter muur (horizontaal)
        for (int x = startX; x < startX + breedte; x++)
        {
            // Achter muur
            Vector3 posAchter = new Vector3(x * tileSize , 0, startZ * tileSize -tileSize);
            GameObject muurAchter = Instantiate(muurPrefab, posAchter, Quaternion.Euler(0, 0, 0));
            muurAchter.transform.parent = dungeonParent.transform;

            // Voor muur
            Vector3 posVoor = new Vector3(x * tileSize , 0, (startZ + diepte) * tileSize -tileSize);
            GameObject muurVoor = Instantiate(muurPrefab, posVoor, Quaternion.Euler(0, 0, 0));
            muurVoor.transform.parent = dungeonParent.transform;
        }
    }
}