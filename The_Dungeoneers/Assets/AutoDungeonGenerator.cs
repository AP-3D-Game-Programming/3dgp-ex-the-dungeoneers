using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

public class AutoDungeonGenerator : MonoBehaviour
{
    public GameObject vloerPrefab;
    public GameObject muurPrefab;
    public GameObject deurPrefab;
    private float tileSize = 4f;

    [Header("Dungeon Settings")]
    public int aantalKamers = 5;
    public int minKamerGrootte = 5;
    public int maxKamerGrootte = 10;
    public int minGangLengte = 5;
    public int maxGangLengte = 10;

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
        public bool Bevat(int checkX, int checkZ)
        {
            return checkX >= x && checkX < x + breedte &&
                   checkZ >= z && checkZ < z + diepte;
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
        int maxPogingen = aantalKamers * 10;
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

        // 2. Muren en Deuren plaatsen
        PlaatsAlleKamerMuren(); // Plaatst muren rond kamers + deuren
        PlaatsAlleGangMuren();  // NIEUW: Plaatst muren langs gangen

        Debug.Log($"Dungeon klaar: {kamers.Count} kamers, {gangen.Count} gangen");
    }

    void PlaatsAlleKamerMuren()
    {
        if (muurPrefab == null) return;

        foreach (Kamer kamer in kamers)
        {
            for (int x = kamer.x; x < kamer.x + kamer.breedte; x++)
            {
                for (int z = kamer.z; z < kamer.z + kamer.diepte; z++)
                {
                    bool isRand = (x == kamer.x || x == kamer.x + kamer.breedte - 1 ||
                                    z == kamer.z || z == kamer.z + kamer.diepte - 1);

                    if (!isRand) continue;

                    CheckEnPlaatsKamerMuur(kamer, x, z, -1, 0, 90f); // Links
                    CheckEnPlaatsKamerMuur(kamer, x, z, 1, 0, 90f);  // Rechts
                    CheckEnPlaatsKamerMuur(kamer, x, z, 0, -1, 0f);  // Achter
                    CheckEnPlaatsKamerMuur(kamer, x, z, 0, 1, 0f);   // Voor
                }
            }
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
        // De positie net buiten de kamer
        int muurX = x + dx;
        int muurZ = z + dz;

        // Is de 'buren' positie buiten de kamer?
        bool isBuitenKamer = (muurX < kamer.x || muurX >= kamer.x + kamer.breedte ||
                              muurZ < kamer.z || muurZ >= kamer.z + kamer.diepte);

        if (!isBuitenKamer) return;

        // Check of de HUIDIGE rand positie een gang is (Ingang)
        if (IsGangTile(x, z))
        {
            // NIEUW: Plaats hier een deur i.p.v. niets!
            if (deurPrefab != null)
            {
                PlaatsObjectOpRand(deurPrefab, x, z, dx, dz, rotatie, "Deur");
            }
            return;
        }

        // Anders: Plaats een muur
        PlaatsObjectOpRand(muurPrefab, x, z, dx, dz, rotatie, "Muur_Kamer");

    }
    void PlaatsAlleGangMuren()
    {
        if (muurPrefab == null) return;

        foreach (Gang gang in gangen)
        {
            foreach (Vector2Int tile in gang.tiles)
            {
                // Check 4 richtingen rondom elke gang-tegel
                CheckEnPlaatsGangMuur(tile.x, tile.y, -1, 0, 90f); // Links
                CheckEnPlaatsGangMuur(tile.x, tile.y, 1, 0, 90f);  // Rechts
                CheckEnPlaatsGangMuur(tile.x, tile.y, 0, -1, 0f);  // Onder
                CheckEnPlaatsGangMuur(tile.x, tile.y, 0, 1, 0f);   // Boven
            }
        }
    }
    void CheckEnPlaatsGangMuur(int x, int z, int dx, int dz, float rotatie)
    {
        int buurX = x + dx;
        int buurZ = z + dz;

        // We plaatsen alleen een gang-muur als de buur-tegel LEEG is.
        // Dus: Geen kamer én Geen gang.
        bool isLeeg = !IsKamerTile(buurX, buurZ) && !IsGangTile(buurX, buurZ);

        if (isLeeg)
        {
            PlaatsObjectOpRand(muurPrefab, x, z, dx, dz, rotatie, "Muur_Gang");
        }
    }
    void PlaatsObjectOpRand(GameObject prefab, int x, int z, int dx, int dz, float rotatie, string naamPrefix)
    {
        bool isLinks = dx == -1 && dz == 0;
        bool isBoven = dx == 0 && dz == 1;
        Vector3 pos;

        // Jouw originele positie logica:
        if (isBoven)
        {
            pos = new Vector3((x + dx) * tileSize, 0, ((z + dz) * tileSize) - tileSize);
        }
        else if (isLinks)
        {
            pos = new Vector3(((x + dx) * tileSize + tileSize), 0, (z + dz) * tileSize);
        }
        else
        {
            pos = new Vector3((x + dx) * tileSize, 0, (z + dz) * tileSize);
        }

        GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(0, rotatie, 0));
        obj.transform.parent = dungeonParent.transform;
        obj.name = $"{naamPrefix}_{x}_{z}";
    }

    bool IsGangTile(int x, int z)
    {
        foreach (Gang gang in gangen)
        {
            foreach (Vector2Int tile in gang.tiles)
            {
                if (tile.x == x && tile.y == z) return true;
            }
        }
        return false;
    }
    bool IsKamerTile(int x, int z)
    {
        foreach (Kamer kamer in kamers)
        {
            if (kamer.Bevat(x, z)) return true;
        }
        return false;
    }
    Kamer ProbeeerKamerToevoegen(Kamer oudeKamer)
    {
        int[] richtingen = { 0, 1, 2, 3 };
        // Shuffle (Fisher-Yates)
        for (int i = 0; i < richtingen.Length; i++)
        {
            int t = richtingen[i];
            int r = Random.Range(i, richtingen.Length);
            richtingen[i] = richtingen[r];
            richtingen[r] = t;
        }

        foreach (int richting in richtingen)
        {
            int nieuwBreedte = Random.Range(minKamerGrootte, maxKamerGrootte);
            int nieuwDiepte = Random.Range(minKamerGrootte, maxKamerGrootte);
            int gangLente = Random.Range(minGangLengte, maxGangLengte);

            Kamer nieuweKamer = null;

            switch (richting)
            {
                case 0: // Links
                    nieuweKamer = new Kamer(oudeKamer.x - nieuwBreedte - gangLente, oudeKamer.z + Random.Range(-nieuwDiepte / 2, oudeKamer.diepte / 2), nieuwBreedte, nieuwDiepte); break;
                case 1: // Rechts
                    nieuweKamer = new Kamer(oudeKamer.x + oudeKamer.breedte + gangLente, oudeKamer.z + Random.Range(-nieuwDiepte / 2, oudeKamer.diepte / 2), nieuwBreedte, nieuwDiepte); break;
                case 2: // Boven
                    nieuweKamer = new Kamer(oudeKamer.x + Random.Range(-nieuwBreedte / 2, oudeKamer.breedte / 2), oudeKamer.z + oudeKamer.diepte + gangLente, nieuwBreedte, nieuwDiepte); break;
                case 3: // Onder
                    nieuweKamer = new Kamer(oudeKamer.x + Random.Range(-nieuwBreedte / 2, oudeKamer.breedte / 2), oudeKamer.z - nieuwDiepte - gangLente, nieuwBreedte, nieuwDiepte); break;
            }

            bool overlapt = false;
            foreach (Kamer bestaandeKamer in kamers)
            {
                if (nieuweKamer.Overlapt(bestaandeKamer)) { overlapt = true; break; }
            }

            int afstand = Mathf.Abs(nieuweKamer.x - oudeKamer.x) + Mathf.Abs(nieuweKamer.z - oudeKamer.z);
            if (afstand > 50) continue;

            if (!overlapt) return nieuweKamer;
        }
        return null;
    }

    void PlaatsKamerVloeren(Kamer kamer)
    {
        for (int x = 0; x < kamer.breedte; x++)
        {
            for (int z = 0; z < kamer.diepte; z++)
            {
                Vector3 pos = new Vector3((kamer.x + x) * tileSize, 0, (kamer.z + z) * tileSize);
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