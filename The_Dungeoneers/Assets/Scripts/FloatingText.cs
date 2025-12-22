using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float destroyTime = 1f;
    public Vector3 offset = new Vector3(0, 2, 0); // Start iets boven het hoofd
    public TextMeshPro textMesh;

    void Awake()
    {
        // Pak automatisch het tekst component
        textMesh = GetComponent<TextMeshPro>();
    }

    void Start()
    {
        // Vernietig jezelf na X seconden
        Destroy(gameObject, destroyTime);
        
        // Startpositie een klein beetje randomizen (zodat ze niet door elkaar clippen)
        transform.position += offset;
        transform.position += new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
    }

    void Update()
    {
        // 1. Beweeg omhoog
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. Kijk altijd naar de camera (Billboarding)
        // Anders zie je de tekst plat van de zijkant als je draait
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }

    // Functie om van buitenaf de tekst en kleur in te stellen
    public void SetText(string text, Color color)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
        }
    }
}