using UnityEngine;

public class AutoDungeonGenerator : MonoBehaviour
{
    public GameObject muurPrefab;
    public GameObject doorgangPrefab;
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

    public float doorgangKans = 0.5f;

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

        // bereken middelste positie
        int middenZ = startZ + diepte / 2;
        int middenX = startX + diepte / 2;

        // Maak muren RONDOM de vloeren
        // Linker en rechter muur (verticaal)
        for (int z = startZ; z < startZ + diepte; z++)
        {
            bool isMidden = (z == middenZ);
            bool krijgtDoorgang = isMidden && Random.value < doorgangKans;

            // Linker muur
            if (!krijgtDoorgang)
            {
                Vector3 posLinks = new Vector3(startX * tileSize, 0, z * tileSize);
                GameObject muurLinks = Instantiate(muurPrefab, posLinks, Quaternion.Euler(0, 90, 0));
                muurLinks.transform.parent = dungeonParent.transform;
            }else if (doorgangPrefab != null)
            {
                Vector3 posLinks = new Vector3(startX * tileSize, 0, z * tileSize);
                GameObject doorgangLinks = Instantiate(doorgangPrefab, posLinks, Quaternion.Euler(0, 90, 0));
                doorgangLinks.transform.parent = dungeonParent.transform;
            }

            // Rechter muur
            if (!krijgtDoorgang)
            {
                Vector3 posRechts = new Vector3((startX + breedte) * tileSize, 0, z * tileSize);
                GameObject muurRechts = Instantiate(muurPrefab, posRechts, Quaternion.Euler(0, 90, 0));
                muurRechts.transform.parent = dungeonParent.transform;
            }
            else if (doorgangPrefab != null) {
                Vector3 posRechts = new Vector3((startX + breedte) * tileSize, 0, z * tileSize);
                GameObject doorgangRechts = Instantiate(doorgangPrefab, posRechts, Quaternion.Euler(0, 90, 0));
                doorgangRechts.transform.parent = dungeonParent.transform;
            }
        }

        // Voor en achter muur (horizontaal)
        for (int x = startX; x < startX + breedte; x++)
        {
            // Achter muur
            bool isMiddenAchter = (x == middenX);
            bool heeftDoorgangAchter = isMiddenAchter && Random.value < doorgangKans;

            if (!heeftDoorgangAchter)
            {
                Vector3 posAchter = new Vector3(x * tileSize, 0, startZ * tileSize - tileSize);
                GameObject muurAchter = Instantiate(muurPrefab, posAchter, Quaternion.Euler(0, 0, 0));
                muurAchter.transform.parent = dungeonParent.transform;
            }
            else if (doorgangPrefab != null)
            {
                Vector3 posAchter = new Vector3(x * tileSize, 0, startZ * tileSize - tileSize);
                GameObject deur = Instantiate(doorgangPrefab, posAchter, Quaternion.Euler(0, 0, 0));
                deur.transform.parent = dungeonParent.transform;
            }

            // Voor muur
            bool isMiddenVoor = (x == middenX);
            bool heeftDoorgangVoor = isMiddenVoor && Random.value < doorgangKans;

            if (!heeftDoorgangVoor)
            {
                Vector3 posVoor = new Vector3(x * tileSize, 0, (startZ + diepte) * tileSize - tileSize);
                GameObject muurVoor = Instantiate(muurPrefab, posVoor, Quaternion.Euler(0, 0, 0));
                muurVoor.transform.parent = dungeonParent.transform;
            }
            else if (doorgangPrefab != null)
            {
                Vector3 posVoor = new Vector3(x * tileSize, 0, (startZ + diepte) * tileSize - tileSize);
                GameObject deur = Instantiate(doorgangPrefab, posVoor, Quaternion.Euler(0, 0, 0));
                deur.transform.parent = dungeonParent.transform;
            }
        }
    }

}