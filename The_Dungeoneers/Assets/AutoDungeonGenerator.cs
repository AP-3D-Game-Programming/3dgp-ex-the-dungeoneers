using UnityEngine;
using System.Collections.Generic;

public class AutoDungeonGenerator : MonoBehaviour
{
    public GameObject vloerPrefab;
    public GameObject muurPrefab;
    private float tileSize = 4f;

    [Header("Dungeon Settings")]
    public int aantalKamers = 5;
    public int minKamerGrootte = 5;
    public int maxKamerGrootte = 10;

    [Range(0f, 1f)]
    public float gangKans = 0.5f;

    private GameObject dungeonParent;
    private List<Kamer> kamers = new List<Kamer>();
    private List<Gang> gangen = new List<Gang>(); // ← Fixed (was "new")

    // Kamer class
    [System.Serializable]
    public class Kamer
    {
        public int x, z;
        public int breedte, diepte;

        public Kamer(int x, int z, int breedte, int diepte)
        {
            this.x = x;
            this.z = z;
            this.breedte = breedte;
            this.diepte = diepte;
        }

        public bool Overlapt(Kamer andere)
        {
            return !(x + breedte < andere.x ||
                     x > andere.x + andere.breedte ||
                     z + diepte < andere.z ||
                     z > andere.z + andere.diepte);
        }
    }

    // Gang class
    [System.Serializable]
    public class Gang
    {
        public int startX, startZ;      // Start positie
        public int eindX, eindZ;        // Eind positie
        public bool isHorizontaal;      // Richting
        public List<Vector2Int> tiles = new List<Vector2Int>(); // Alle vloer posities
        public int Lengte
        {
            get
            {
                if (isHorizontaal)
                    return Mathf.Abs(eindX - startX) + 1;
                else
                    return Mathf.Abs(eindZ - startZ) + 1;
            }
        }
        public Gang(int startX, int startZ, int eindX, int eindZ, bool isHorizontaal)
        {
            this.startX = startX;
            this.startZ = startZ;
            this.eindX = eindX;
            this.eindZ = eindZ;
            this.isHorizontaal = isHorizontaal;
        }

        public void VoegTileToe(int x, int z)
        {
            tiles.Add(new Vector2Int(x, z));
        }
    }

    [ContextMenu("Genereer Dungeon (Alleen Vloeren)")]
    public void GenereerDungeon()
    {
        if (dungeonParent != null)
            DestroyImmediate(dungeonParent);

        dungeonParent = new GameObject("Dungeon");
        kamers.Clear();
        gangen.Clear();

        // Maak eerste kamer
        Kamer eersteKamer = new Kamer(0, 0,
            Random.Range(minKamerGrootte, maxKamerGrootte),
            Random.Range(minKamerGrootte, maxKamerGrootte));

        kamers.Add(eersteKamer);
        PlaatsKamerVloeren(eersteKamer);

        // Voeg meer kamers toe met meerdere pogingen
        int toegevoegdeKamers = 1;
        int maxPogingen = aantalKamers * 10; // Veel meer pogingen!
        int pogingen = 0;

        while (toegevoegdeKamers < aantalKamers && pogingen < maxPogingen)
        {
            pogingen++;

            // Kies random bestaande kamer
            Kamer oudeKamer = kamers[Random.Range(0, kamers.Count)];

            // Probeer nieuwe kamer toe te voegen
            Kamer nieuweKamer = ProbeeerKamerToevoegen(oudeKamer);

            if (nieuweKamer != null)
            {
                kamers.Add(nieuweKamer);
                PlaatsKamerVloeren(nieuweKamer);
                MaakGangVloeren(oudeKamer, nieuweKamer);

                toegevoegdeKamers++;
                Debug.Log($"Kamer {toegevoegdeKamers}/{aantalKamers} toegevoegd na {pogingen} pogingen");
            }
        }

        if (toegevoegdeKamers < aantalKamers)
        {
            Debug.LogWarning($"Kon maar {toegevoegdeKamers}/{aantalKamers} kamers plaatsen na {pogingen} pogingen!");
        }
        PlaatsAlleKamerMuren();
        Debug.Log($"Dungeon klaar: {kamers.Count} kamers, {gangen.Count} gangen");
    }
    void PlaatsAlleKamerMuren()
    {
        if (muurPrefab == null)
        {
            Debug.LogWarning("Geen muur prefab ingesteld!");
            return;
        }

        foreach (Kamer kamer in kamers)
        {
            PlaatsKamerMuren(kamer);
        }
    }
    void PlaatsKamerMuren(Kamer kamer)
    {
        // Loop rondom de kamer (de rand)
        for (int x = kamer.x; x < kamer.x + kamer.breedte; x++)
        {
            for (int z = kamer.z; z < kamer.z + kamer.diepte; z++)
            {
                // Is dit een rand positie?
                bool isRand = (x == kamer.x ||
                              x == kamer.x + kamer.breedte - 1 ||
                              z == kamer.z ||
                              z == kamer.z + kamer.diepte - 1);

                if (!isRand) continue; // Niet op rand? Skip!

                // Check alle 4 richtingen voor muur plaatsing
                CheckEnPlaatsKamerMuur(kamer, x, z, -1, 0, 90f);  // Links
                CheckEnPlaatsKamerMuur(kamer, x, z, 1, 0, 90f);  // Rechts
                CheckEnPlaatsKamerMuur(kamer, x, z, 0, -1, 0f);   // Achter
                CheckEnPlaatsKamerMuur(kamer, x, z, 0, 1, 0f);   // Voor
            }
        }
    }
    void CheckEnPlaatsKamerMuur(Kamer kamer, int x, int z, int dx, int dz, float rotatie)
    {
        // Bereken waar de muur zou komen (tussen twee tiles)
        int muurX = x + dx;
        int muurZ = z + dz;

        // Is deze positie buiten de kamer? (dan hebben we een muur nodig)
        bool isBuitenKamer = (muurX < kamer.x ||
                              muurX >= kamer.x + kamer.breedte ||
                              muurZ < kamer.z ||
                              muurZ >= kamer.z + kamer.diepte);

        if (!isBuitenKamer) return; // Binnen kamer? Geen muur nodig!

        // Check of deze positie een gang tile is (dan GEEN muur!)
        if (IsGangTile(x, z))
        {
            Debug.Log($"Skip muur op ({x},{z}) - dit is een gang ingang!");
            return; // Gang ingang! Geen muur plaatsen
        }
        bool isLinks = dx == -1 && dz == 0;
        bool isBoven = dx == 0 && dz == 1;
        Vector3 muurPos = new Vector3();

        // hier doe ik boven en links appart omdat de muurprefab 1 kantig is. zo behoud je het gridformaat.
        if (isBoven)
        {
            // Plaats de muur
            muurPos = new Vector3((x + dx) * tileSize, 0, ((z + dz) * tileSize) - tileSize);
            GameObject muur = Instantiate(muurPrefab, muurPos, Quaternion.Euler(0, rotatie, 0));
            muur.transform.parent = dungeonParent.transform;
            muur.name = $"Muur_Kamer_{x}_{z}";
        }else if (isLinks){
            // Plaats de muur
            muurPos = new Vector3(((x + dx) * tileSize + tileSize), 0, (z + dz) * tileSize);
            GameObject muur = Instantiate(muurPrefab, muurPos, Quaternion.Euler(0, rotatie, 0));
            muur.transform.parent = dungeonParent.transform;
            muur.name = $"Muur_Kamer_{x}_{z}";
        }else
        {
            muurPos = new Vector3((x + dx) * tileSize, 0, (z + dz) * tileSize);
            GameObject muur = Instantiate(muurPrefab, muurPos, Quaternion.Euler(0, rotatie, 0));
            muur.transform.parent = dungeonParent.transform;
            muur.name = $"Muur_Kamer_{x}_{z}";
        }

    }
    bool IsGangTile(int x, int z)
    {
        // Check of deze positie in een van de gangen zit
        foreach (Gang gang in gangen)
        {
            foreach (Vector2Int tile in gang.tiles)
            {
                if (tile.x == x && tile.y == z)
                {
                    return true; // Dit is een gang tile!
                }
            }
        }
        return false; // Geen gang hier
    }
    Kamer ProbeeerKamerToevoegen(Kamer oudeKamer)
    {
        // Probeer ALLE 4 richtingen (geen gangKans check hier!)
        int[] richtingen = { 0, 1, 2, 3 };

        // Shuffle
        for (int i = 0; i < richtingen.Length; i++)
        {
            int temp = richtingen[i];
            int randomIndex = Random.Range(i, richtingen.Length);
            richtingen[i] = richtingen[randomIndex];
            richtingen[randomIndex] = temp;
        }

        foreach (int richting in richtingen)
        {
            int nieuwBreedte = Random.Range(minKamerGrootte, maxKamerGrootte);
            int nieuwDiepte = Random.Range(minKamerGrootte, maxKamerGrootte);
            int spacing = Random.Range(2, 5); // Variabele gang lengte

            Kamer nieuweKamer = null;

            switch (richting)
            {
                case 0: // Links
                    nieuweKamer = new Kamer(
                        oudeKamer.x - nieuwBreedte - spacing,
                        oudeKamer.z + Random.Range(-nieuwDiepte / 2, oudeKamer.diepte / 2), // Variatie in uitlijning
                        nieuwBreedte, nieuwDiepte);
                    break;

                case 1: // Rechts
                    nieuweKamer = new Kamer(
                        oudeKamer.x + oudeKamer.breedte + spacing,
                        oudeKamer.z + Random.Range(-nieuwDiepte / 2, oudeKamer.diepte / 2),
                        nieuwBreedte, nieuwDiepte);
                    break;

                case 2: // Boven
                    nieuweKamer = new Kamer(
                        oudeKamer.x + Random.Range(-nieuwBreedte / 2, oudeKamer.breedte / 2),
                        oudeKamer.z + oudeKamer.diepte + spacing,
                        nieuwBreedte, nieuwDiepte);
                    break;

                case 3: // Onder
                    nieuweKamer = new Kamer(
                        oudeKamer.x + Random.Range(-nieuwBreedte / 2, oudeKamer.breedte / 2),
                        oudeKamer.z - nieuwDiepte - spacing,
                        nieuwBreedte, nieuwDiepte);
                    break;
            }

            // Check overlap met ALLE bestaande kamers
            bool overlapt = false;
            foreach (Kamer bestaandeKamer in kamers)
            {
                if (nieuweKamer.Overlapt(bestaandeKamer))
                {
                    overlapt = true;
                    break;
                }
            }

            // Ook checken of niet te ver weg
            int afstand = Mathf.Abs(nieuweKamer.x - oudeKamer.x) + Mathf.Abs(nieuweKamer.z - oudeKamer.z);
            if (afstand > 50) // Max afstand
            {
                continue;
            }

            if (!overlapt)
            {
                return nieuweKamer; // Succes!
            }
        }

        return null; // Geen enkele richting werkte
    }

    void PlaatsKamerVloeren(Kamer kamer)
    {
        for (int x = 0; x < kamer.breedte; x++)
        {
            for (int z = 0; z < kamer.diepte; z++)
            {
                Vector3 pos = new Vector3(
                    (kamer.x + x) * tileSize,
                    0,
                    (kamer.z + z) * tileSize
                );

                GameObject vloer = Instantiate(vloerPrefab, pos, Quaternion.identity);
                vloer.transform.parent = dungeonParent.transform;
                vloer.name = $"Vloer_Kamer_{kamer.x + x}_{kamer.z + z}";
            }
        }
    }

    void MaakGangVloeren(Kamer kamer1, Kamer kamer2)
    {
        int midden1X = kamer1.x + kamer1.breedte / 2;
        int midden1Z = kamer1.z + kamer1.diepte / 2;
        int midden2X = kamer2.x + kamer2.breedte / 2;
        int midden2Z = kamer2.z + kamer2.diepte / 2;

        // Maak L-vormige gang (2 rechte stukken)

        // 1. HORIZONTAAL STUK
        int startX = Mathf.Min(midden1X, midden2X);
        int eindX = Mathf.Max(midden1X, midden2X);

        Gang horizontaleGang = new Gang(startX, midden1Z, eindX, midden1Z, true);

        for (int x = startX; x <= eindX; x++)
        {
            Vector3 pos = new Vector3(x * tileSize, 0, midden1Z * tileSize);
            GameObject vloer = Instantiate(vloerPrefab, pos, Quaternion.identity);
            vloer.transform.parent = dungeonParent.transform;
            vloer.name = $"Vloer_Gang_{x}_{midden1Z}";

            horizontaleGang.VoegTileToe(x, midden1Z);
        }

        gangen.Add(horizontaleGang);

        // 2. VERTICAAL STUK
        int startZ = Mathf.Min(midden1Z, midden2Z);
        int eindZ = Mathf.Max(midden1Z, midden2Z);

        Gang verticaleGang = new Gang(midden2X, startZ, midden2X, eindZ, false);

        for (int z = startZ; z <= eindZ; z++)
        {
            Vector3 pos = new Vector3(midden2X * tileSize, 0, z * tileSize);
            GameObject vloer = Instantiate(vloerPrefab, pos, Quaternion.identity);
            vloer.transform.parent = dungeonParent.transform;
            vloer.name = $"Vloer_Gang_{midden2X}_{z}";

            verticaleGang.VoegTileToe(midden2X, z);
        }

        gangen.Add(verticaleGang);
    }

    [ContextMenu("Verwijder Dungeon")]
    public void deleteDungeon()
    {
        if (dungeonParent != null)
            DestroyImmediate(dungeonParent);
        kamers.Clear();
        gangen.Clear();
    }
}