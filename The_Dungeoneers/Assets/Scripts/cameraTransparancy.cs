using UnityEngine;

public class CameraTransparancy : MonoBehaviour
{
    public Transform player;

    private Transform currentObstacle;
    private Renderer[] currentRenderers;
    
    // Dit is onze "sticker" die we op de muur plakken
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        // We maken het block één keer aan bij de start
        propBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (player == null) return;
        
        // Debug lijn (Rood = Vrij, Groen = Raak)
        Debug.DrawLine(transform.position, player.position, Color.red);

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance))
        {
            Transform hitObject = hit.transform;
            
            // Als we de speler raken, doen we niks
            if (hitObject == player) 
            {
                ResetOldObstacle();
                return;
            }

            // Hebben we een NIEUWE muur geraakt?
            if (hitObject != currentObstacle)
            {
                // Reset eerst de vorige muur
                ResetOldObstacle();

                currentObstacle = hitObject;
                
                // Pak alle renderers (ook als de muur uit meerdere stukjes bestaat)
                currentRenderers = currentObstacle.GetComponentsInChildren<Renderer>();

                if (currentRenderers.Length > 0)
                {
                    Debug.Log("Muur gevonden: " + hitObject.name);

                    // Loop door alle onderdelen van de muur
                    foreach (Renderer r in currentRenderers)
                    {
                        // 1. Haal de huidige status op
                        r.GetPropertyBlock(propBlock);
                        
                        // 2. Haal de originele kleur uit het materiaal (voor de zekerheid)
                        // URP Lit gebruikt "_BaseColor", Standard gebruikt "_Color"
                        Color baseColor = Color.white;
                        if (r.sharedMaterial.HasProperty("_BaseColor"))
                            baseColor = r.sharedMaterial.GetColor("_BaseColor");
                        else if (r.sharedMaterial.HasProperty("_Color"))
                            baseColor = r.sharedMaterial.color;

                        // 3. Zet de Alpha op 0.3 (30% zichtbaar)
                        baseColor.a = 0.3f;
                        propBlock.SetColor("_BaseColor", baseColor); // Voor URP
                        propBlock.SetColor("_Color", baseColor);     // Voor Standard (voor de zekerheid)

                        // 4. Plak de sticker terug op de muur
                        r.SetPropertyBlock(propBlock);
                    }
                }
            }
        }
        else
        {
            // Er staat niks meer tussen, reset alles
            ResetOldObstacle();
        }
    }

    void ResetOldObstacle()
    {
        if (currentObstacle != null && currentRenderers != null)
        {
            foreach (Renderer r in currentRenderers)
            {
                if (r == null) continue;

                // Haal de sticker op
                r.GetPropertyBlock(propBlock);
                
                // Haal de originele kleur op (die is altijd 1.0 alpha)
                Color baseColor = Color.white;
                 if (r.sharedMaterial.HasProperty("_BaseColor"))
                    baseColor = r.sharedMaterial.GetColor("_BaseColor");
                else if (r.sharedMaterial.HasProperty("_Color"))
                    baseColor = r.sharedMaterial.color;

                // Zet alpha terug naar 1.0 (volledig zichtbaar)
                baseColor.a = 1.0f;
                propBlock.SetColor("_BaseColor", baseColor);
                propBlock.SetColor("_Color", baseColor);

                // Pas toe
                r.SetPropertyBlock(propBlock);
            }

            currentObstacle = null;
            currentRenderers = null;
        }
    }
}