using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Beweging")]
    public Transform target;            // De speler
    public float smoothSpeed = 0.125f;  // Snelheid
    public Vector3 offset;              // Afstand

    [Header("Muren Doorzichtig Maken")]
    public LayerMask obstacleLayer;     // Optioneel: Welke lagen zijn muren? (Default = alles)
    private Transform currentObstacle;
    private Renderer[] currentRenderers;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        // 1. Zoek de speler als die er nog niet in zit
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }

        // 2. Initialiseer de 'sticker' voor de kleuren
        propBlock = new MaterialPropertyBlock();
        
        // (Optioneel) Zet offset automatisch als je dat wilt:
        // offset = transform.position - target.position; 
    }

    void LateUpdate()
    {
        if (target == null) return;

        // --- STAP A: Beweeg de Camera ---
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // --- STAP B: Check voor Muren ---
        HandleWallTransparency();
    }

    void HandleWallTransparency()
    {
        // Richting van camera NAAR speler
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;

        // Teken een rode lijn in de Scene view om te testen
        Debug.DrawRay(transform.position, direction, Color.red);

        // Schiet de laser
        // We gebruiken 'distance - 0.5f' zodat we net NIET de speler zelf raken
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance - 0.5f, obstacleLayer))
        {
            Transform hitObject = hit.transform;

            // Is dit een NIEUWE muur die we raken?
            if (hitObject != currentObstacle)
            {
                // Zet de vorige muur weer normaal
                ResetOldObstacle();

                // Stel de nieuwe muur in
                currentObstacle = hitObject;
                currentRenderers = currentObstacle.GetComponentsInChildren<Renderer>();

                // Maak deze muur doorzichtig
                if (currentRenderers != null)
                {
                    foreach (Renderer r in currentRenderers)
                    {
                        ChangeAlpha(r, 0.3f); // 0.3 = 30% zichtbaar
                    }
                }
            }
        }
        else
        {
            // We raken niks (of alleen de speler), dus reset alles
            ResetOldObstacle();
        }
    }

    void ResetOldObstacle()
    {
        if (currentObstacle != null && currentRenderers != null)
        {
            foreach (Renderer r in currentRenderers)
            {
                if (r != null) ChangeAlpha(r, 1.0f); // 1.0 = 100% zichtbaar
            }
        }
        currentObstacle = null;
        currentRenderers = null;
    }

    // Hulpfunctie om de kleur/alpha aan te passen
    void ChangeAlpha(Renderer r, float alphaValue)
    {
        r.GetPropertyBlock(propBlock);

        // Probeer de basiskleur te vinden (werkt voor URP en Standard)
        Color baseColor = Color.white;
        if (r.sharedMaterial.HasProperty("_BaseColor"))
            baseColor = r.sharedMaterial.GetColor("_BaseColor");
        else if (r.sharedMaterial.HasProperty("_Color"))
            baseColor = r.sharedMaterial.GetColor("_Color"); // Let op: soms sharedMaterial.color

        // Pas alpha aan
        baseColor.a = alphaValue;

        // Zet waardes terug in het block
        propBlock.SetColor("_BaseColor", baseColor);
        propBlock.SetColor("_Color", baseColor);
        
        // Plak het op de renderer
        r.SetPropertyBlock(propBlock);
    }
}