using UnityEngine;
using System.Collections.Generic;

public class AutoDungeonGenerator : MonoBehaviour
{
    [Header("Prefabs - Dungeon")]
    public GameObject vloerPrefab;
    public GameObject muurPrefab;
    public GameObject deurPrefab;

    [Header("Prefabs - Enemies")]
    public List<EnemySpawnInfo> enemyTypes = new List<EnemySpawnInfo>();

    [Header("Dungeon Settings")]
    public int aantalKamers = 5;
    public int minKamerGrootte = 5;
    public int maxKamerGrootte = 10;
    public int minGangLengte = 5;
    public int maxGangLengte = 10;

    [Header("Enemy Settings")]
    public bool spawnsEnemiesInEersteKamer = false;
    [Range(0f, 1f)]
    public float kamerEnemySpawnKans = 0.8f;
    public int minEnemiesPerKamer = 1;
    public int maxEnemiesPerKamer = 5;
    public float enemySpawnMargin = 1;  // Afstand van muren

    [Range(0f, 1f)]
    public float gangKans = 0.5f;

    private float tileSize = 4f;
    private GameObject dungeonParent;
    private GameObject enemiesParent;
    private List<Kamer> kamers = new List<Kamer>();
    private List<Gang> gangen = new List<Gang>();
    private List<GameObject> gespawndeEnemies = new List<GameObject>();

    // ══════════════════════════════════════════════════════════════
    // CLASSES
    // ══════════════════════════════════════════════════════════════

    [System.Serializable]
    public class EnemySpawnInfo
    {
        public string naam = "Enemy";
        public GameObject prefab;
        [Range(1, 100)]
        public int spawnGewicht = 10;  // Hogere waarde = vaker spawnen
        public int minimumKamerGrootte = 0;  // 0 = geen minimum
    }

    [System.Serializable]
    public class Kamer
    {
        public int x, z;
        public int breedte, diepte;
        public KamerType type = KamerType.Normaal;
        public List<GameObject> enemies = new List<GameObject>();

        // Handige properties
        public int Oppervlakte => breedte * diepte;
        public int CenterX => x + breedte / 2;
        public int CenterZ => z + diepte / 2;
        public Vector3 CenterWorld(float tileSize) => new Vector3(CenterX * tileSize, 0, CenterZ * tileSize);

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

        // Geeft een random positie binnen de kamer (met margin van de muren)
        public Vector3 GetRandomPositie(float tileSize, float margin = 1f)
        {
            float minX = (x + margin) * tileSize;
            float maxX = (x + breedte - margin) * tileSize;
            float minZ = (z + margin) * tileSize;
            float maxZ = (z + diepte - margin) * tileSize;

            return new Vector3(
                Random.Range(minX, maxX),
                0f,
                Random.Range(minZ, maxZ)
            );
        }
    }

    public enum KamerType
    {
        Normaal,
        Start,
        Boss,
        Treasure,
        Safe  // Geen enemies
    }

    [System.Serializable]
    public class Gang
    {
        public int startX, startZ;
        public int eindX, eindZ;
        public bool isHorizontaal;
        public List<Vector2Int> tiles = new List<Vector2Int>();

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

    // ══════════════════════════════════════════════════════════════
    // MAIN GENERATION
    // ══════════════════════════════════════════════════════════════

    [ContextMenu("Genereer Dungeon")]
    public void GenereerDungeon()
    {
        // Cleanup
        VerwijderDungeon();

        // Maak parent objecten
        dungeonParent = new GameObject("Dungeon");
        enemiesParent = new GameObject("Enemies");
        enemiesParent.transform.parent = dungeonParent.transform;

        // 1. Genereer kamers
        GenereerAlleKamers();

        // 2. Wijs kamer types toe
        WijsKamerTypesToe();

        // 3. Plaats muren en deuren
        PlaatsAlleKamerMuren();
        PlaatsAlleGangMuren();

        // 4. Spawn enemies
        SpawnAlleEnemies();

        Debug.Log($"Dungeon klaar: {kamers.Count} kamers, {gangen.Count} gangen, {gespawndeEnemies.Count} enemies");
    }

    void GenereerAlleKamers()
    {
        // Maak eerste kamer
        Kamer eersteKamer = new Kamer(0, 0,
            Random.Range(minKamerGrootte, maxKamerGrootte),
            Random.Range(minKamerGrootte, maxKamerGrootte));
        eersteKamer.type = KamerType.Start;

        kamers.Add(eersteKamer);
        PlaatsKamerVloeren(eersteKamer);

        // Voeg meer kamers toe
        int toegevoegdeKamers = 1;
        int maxPogingen = aantalKamers * 10;
        int pogingen = 0;

        while (toegevoegdeKamers < aantalKamers && pogingen < maxPogingen)
        {
            pogingen++;

            Kamer oudeKamer = kamers[Random.Range(0, kamers.Count)];
            Kamer nieuweKamer = ProbeeerKamerToevoegen(oudeKamer);

            if (nieuweKamer != null)
            {
                kamers.Add(nieuweKamer);
                PlaatsKamerVloeren(nieuweKamer);
                MaakGangVloeren(oudeKamer, nieuweKamer);
                toegevoegdeKamers++;
            }
        }
    }

    void WijsKamerTypesToe()
    {
        // Eerste kamer is al Start
        // Laatste kamer kan Boss zijn
        if (kamers.Count > 2)
        {
            kamers[kamers.Count - 1].type = KamerType.Boss;
        }

        // Eventueel: random treasure kamers
        for (int i = 1; i < kamers.Count - 1; i++)
        {
            if (Random.value < 0.1f)  // 10% kans
            {
                kamers[i].type = KamerType.Treasure;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    // ENEMY SPAWNING
    // ══════════════════════════════════════════════════════════════

    void SpawnAlleEnemies()
    {
        if (enemyTypes.Count == 0)
        {
            Debug.LogWarning("Geen enemy types geconfigureerd!");
            return;
        }

        for (int i = 0; i < kamers.Count; i++)
        {
            Kamer kamer = kamers[i];

            // Skip eerste kamer als ingesteld
            if (i == 0 && !spawnsEnemiesInEersteKamer)
                continue;

            // Skip safe kamers
            if (kamer.type == KamerType.Safe || kamer.type == KamerType.Start)
                continue;

            // Check spawn kans
            if (Random.value > kamerEnemySpawnKans)
                continue;

            // Spawn enemies in deze kamer
            SpawnEnemiesInKamer(kamer);
        }
    }

    void SpawnEnemiesInKamer(Kamer kamer)
    {
        // Bepaal aantal enemies (schaal met kamer grootte)
        int baseAantal = Random.Range(minEnemiesPerKamer, maxEnemiesPerKamer + 1);

        // Bonus enemies voor grote kamers
        int bonusEnemies = kamer.Oppervlakte > 50 ? Random.Range(0, 3) : 0;

        // Boss kamers krijgen meer enemies
        if (kamer.type == KamerType.Boss)
        {
            baseAantal += 2;
        }

        int totaalEnemies = baseAantal + bonusEnemies;

        // Spawn elke enemy
        for (int i = 0; i < totaalEnemies; i++)
        {
            EnemySpawnInfo enemyInfo = KiesRandomEnemy(kamer);

            if (enemyInfo == null || enemyInfo.prefab == null)
                continue;

            Vector3 spawnPos = kamer.GetRandomPositie(tileSize, enemySpawnMargin);

            GameObject enemy = Instantiate(
                enemyInfo.prefab,
                spawnPos,
                Quaternion.Euler(0, Random.Range(0f, 360f), 0)
            );

            enemy.transform.parent = enemiesParent.transform;
            enemy.name = $"{enemyInfo.naam}_{kamer.CenterX}_{kamer.CenterZ}_{i}";

            kamer.enemies.Add(enemy);
            gespawndeEnemies.Add(enemy);
        }
    }

    EnemySpawnInfo KiesRandomEnemy(Kamer kamer)
    {
        // Filter enemies die in deze kamer mogen spawnen
        List<EnemySpawnInfo> beschikbaar = new List<EnemySpawnInfo>();
        int totaalGewicht = 0;

        foreach (EnemySpawnInfo info in enemyTypes)
        {
            if (info.prefab == null) continue;
            if (info.minimumKamerGrootte > kamer.Oppervlakte) continue;

            beschikbaar.Add(info);
            totaalGewicht += info.spawnGewicht;
        }

        if (beschikbaar.Count == 0) return null;

        // Gewogen random selectie
        int randomWaarde = Random.Range(0, totaalGewicht);
        int huidigeWaarde = 0;

        foreach (EnemySpawnInfo info in beschikbaar)
        {
            huidigeWaarde += info.spawnGewicht;
            if (randomWaarde < huidigeWaarde)
                return info;
        }

        return beschikbaar[0];
    }

    // ══════════════════════════════════════════════════════════════
    // MUREN EN VLOEREN (Origineel behouden)
    // ══════════════════════════════════════════════════════════════

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

                    CheckEnPlaatsKamerMuur(kamer, x, z, -1, 0, 90f);
                    CheckEnPlaatsKamerMuur(kamer, x, z, 1, 0, 90f);
                    CheckEnPlaatsKamerMuur(kamer, x, z, 0, -1, 0f);
                    CheckEnPlaatsKamerMuur(kamer, x, z, 0, 1, 0f);
                }
            }
        }
    }

    void CheckEnPlaatsKamerMuur(Kamer kamer, int x, int z, int dx, int dz, float rotatie)
    {
        int muurX = x + dx;
        int muurZ = z + dz;

        bool isBuitenKamer = (muurX < kamer.x || muurX >= kamer.x + kamer.breedte ||
                              muurZ < kamer.z || muurZ >= kamer.z + kamer.diepte);

        if (!isBuitenKamer) return;

        if (IsGangTile(x, z))
        {
            if (deurPrefab != null)
            {
                PlaatsObjectOpRand(deurPrefab, x, z, dx, dz, rotatie, "Deur");
            }
            return;
        }

        PlaatsObjectOpRand(muurPrefab, x, z, dx, dz, rotatie, "Muur_Kamer");
    }

    void PlaatsAlleGangMuren()
    {
        if (muurPrefab == null) return;

        foreach (Gang gang in gangen)
        {
            foreach (Vector2Int tile in gang.tiles)
            {
                CheckEnPlaatsGangMuur(tile.x, tile.y, -1, 0, 90f);
                CheckEnPlaatsGangMuur(tile.x, tile.y, 1, 0, 90f);
                CheckEnPlaatsGangMuur(tile.x, tile.y, 0, -1, 0f);
                CheckEnPlaatsGangMuur(tile.x, tile.y, 0, 1, 0f);
            }
        }
    }

    void CheckEnPlaatsGangMuur(int x, int z, int dx, int dz, float rotatie)
    {
        int buurX = x + dx;
        int buurZ = z + dz;

        bool isLeeg = !IsKamerTile(buurX, buurZ) && !IsGangTile(buurX, buurZ);

        if (isLeeg)
        {
            PlaatsObjectOpRand(muurPrefab, x, z, dx, dz, rotatie, "Muur_Gang");
        }
    }

    // ORIGINELE OFFSET LOGICA BEHOUDEN
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

    // ══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ══════════════════════════════════════════════════════════════

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

        // Fisher-Yates shuffle
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
            int gangLengte = Random.Range(minGangLengte, maxGangLengte);

            Kamer nieuweKamer = null;

            switch (richting)
            {
                case 0: // Links
                    nieuweKamer = new Kamer(
                        oudeKamer.x - nieuwBreedte - gangLengte,
                        oudeKamer.z + Random.Range(-nieuwDiepte / 2, oudeKamer.diepte / 2),
                        nieuwBreedte, nieuwDiepte);
                    break;
                case 1: // Rechts
                    nieuweKamer = new Kamer(
                        oudeKamer.x + oudeKamer.breedte + gangLengte,
                        oudeKamer.z + Random.Range(-nieuwDiepte / 2, oudeKamer.diepte / 2),
                        nieuwBreedte, nieuwDiepte);
                    break;
                case 2: // Boven
                    nieuweKamer = new Kamer(
                        oudeKamer.x + Random.Range(-nieuwBreedte / 2, oudeKamer.breedte / 2),
                        oudeKamer.z + oudeKamer.diepte + gangLengte,
                        nieuwBreedte, nieuwDiepte);
                    break;
                case 3: // Onder
                    nieuweKamer = new Kamer(
                        oudeKamer.x + Random.Range(-nieuwBreedte / 2, oudeKamer.breedte / 2),
                        oudeKamer.z - nieuwDiepte - gangLengte,
                        nieuwBreedte, nieuwDiepte);
                    break;
            }

            bool overlapt = false;
            foreach (Kamer bestaandeKamer in kamers)
            {
                if (nieuweKamer.Overlapt(bestaandeKamer))
                {
                    overlapt = true;
                    break;
                }
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

    // ══════════════════════════════════════════════════════════════
    // CLEANUP & UTILITY
    // ══════════════════════════════════════════════════════════════

    [ContextMenu("Verwijder Dungeon")]
    public void VerwijderDungeon()
    {
        if (dungeonParent != null)
            DestroyImmediate(dungeonParent);

        kamers.Clear();
        gangen.Clear();
        gespawndeEnemies.Clear();
    }

    [ContextMenu("Verwijder Alleen Enemies")]
    public void VerwijderAlleenEnemies()
    {
        foreach (GameObject enemy in gespawndeEnemies)
        {
            if (enemy != null)
                DestroyImmediate(enemy);
        }
        gespawndeEnemies.Clear();

        foreach (Kamer kamer in kamers)
        {
            kamer.enemies.Clear();
        }

        Debug.Log("Alle enemies verwijderd!");
    }

    [ContextMenu("Respawn Enemies")]
    public void RespawnEnemies()
    {
        VerwijderAlleenEnemies();
        SpawnAlleEnemies();
    }

    // ══════════════════════════════════════════════════════════════
    // PUBLIC GETTERS (handig voor andere scripts)
    // ══════════════════════════════════════════════════════════════

    public List<Kamer> GetKamers() => kamers;
    public List<Gang> GetGangen() => gangen;
    public List<GameObject> GetEnemies() => gespawndeEnemies;
    public Kamer GetStartKamer() => kamers.Count > 0 ? kamers[0] : null;
    public Kamer GetBossKamer() => kamers.Find(k => k.type == KamerType.Boss);

    public Vector3 GetSpawnPoint()
    {
        Kamer startKamer = GetStartKamer();
        if (startKamer != null)
        {
            return startKamer.CenterWorld(tileSize);
        }
        return Vector3.zero;
    }
}