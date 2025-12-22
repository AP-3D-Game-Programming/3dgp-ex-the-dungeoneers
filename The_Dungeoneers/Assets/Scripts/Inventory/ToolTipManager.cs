using UnityEngine;
using TMPro; // Of 'using UnityEngine.UI;' als je oude tekst gebruikt
using UnityEngine.InputSystem; // Als je het nieuwe input system gebruikt

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager instance;

    [Header("UI Koppelingen")]
    public TextMeshProUGUI headerText;  // Gebruik 'Text' als je geen TMP gebruikt
    public TextMeshProUGUI contentText; 

    void Awake()
    {
        instance = this;
        // Start onzichtbaar
        gameObject.SetActive(false); 
    }

    void Update()
    {
        // Laat de tooltip de muis volgen
        // We tellen een klein beetje op bij de positie zodat hij niet ONDER je muis zit
        transform.position = Input.mousePosition + new Vector3(15, -15, 0); 
    }

    public void Show(string header, string content)
    {
        headerText.text = header;
        contentText.text = content;
        
        // Pas de grootte aan op basis van de tekst (optioneel, maar netjes)
        // Voor nu gewoon aanzetten:
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}