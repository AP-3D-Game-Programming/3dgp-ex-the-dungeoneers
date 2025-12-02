using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Hier sleep je straks je speler in
    
    public float smoothSpeed = 0.125f; // Hoe hoger, hoe sneller de camera volgt (0-1)
    public Vector3 offset; // De afstand tussen camera en speler

    void LateUpdate()
    {
        if (target == null) return; // Veiligheidscheck als er geen speler is

        // Waar willen we dat de camera naartoe gaat?
        Vector3 desiredPosition = target.position + offset;
        
        // Zorg voor een vloeiende beweging (Smooth Damp)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Pas de positie toe
        transform.position = smoothedPosition;

        // Optioneel: Zorg dat de camera altijd naar de speler kijkt
        // transform.LookAt(target); 
    }
}