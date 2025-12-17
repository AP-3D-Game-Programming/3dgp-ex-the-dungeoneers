using UnityEngine;
using System.Collections.Generic;

public class PlayerPickup : MonoBehaviour
{
    public bool HasNearbyObject => nearbyObjects.Count > 0;

    [Header("Setup")]
    public Transform holdPoint;                 // Punt waar object vastgehouden wordt
    public KeyCode pickupKey = KeyCode.E;       // Oppakken / neerleggen
    public KeyCode throwKey = KeyCode.Mouse0;   // Gooien
    public bool isCarrying = false;              // Voor sprint / animaties

    [Header("Throw Settings")]
    public float throwForce = 10f;

    private GameObject heldObject;
    private Rigidbody heldRb;
    private HashSet<GameObject> nearbyObjects = new HashSet<GameObject>();

    // Animator
    private Animator playerAnimator;

    void Start()
    {
        // Animator ophalen van de Player
        playerAnimator = GetComponent<Animator>();

        if (playerAnimator == null)
        {
            Debug.LogError(
                "Animator component niet gevonden op de Player! " +
                "Zorg dat de Animator op hetzelfde GameObject zit als PlayerPickup."
            );
        }
    }

    void Update()
    {
        // Oppakken / neerleggen
        if (Input.GetKeyDown(pickupKey))
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Pickup");
            }

            if (heldObject == null)
            {
                PickupNearest();
            }
            else
            {
                DropHeld();
            }
        }

        // Gooien
        if (Input.GetKeyDown(throwKey) && heldObject != null)
        {
            ThrowHeld();
        }

        // Houd object vast op holdPoint
        if (heldObject != null)
        {
            heldObject.transform.position = holdPoint.position;
            heldObject.transform.rotation = holdPoint.rotation;
        }
    }

    // Zoek het dichtstbijzijnde object
    void PickupNearest()
    {
        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (GameObject obj in nearbyObjects)
        {
            if (obj == null) continue;

            float dist = Vector3.Distance(obj.transform.position, holdPoint.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = obj;
            }
        }

        if (nearest != null)
        {
            Pickup(nearest);
            isCarrying = true;
        }
    }

    void Pickup(GameObject obj)
    {
        heldObject = obj;
        heldRb = obj.GetComponent<Rigidbody>();

        if (heldRb != null)
        {
            heldRb.isKinematic = true;
            heldRb.detectCollisions = false;
        }

        obj.transform.SetParent(holdPoint);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    public void DropHeld()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);

        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.detectCollisions = true;
        }

        heldObject = null;
        heldRb = null;
        isCarrying = false;
    }

    void ThrowHeld()
    {
        if (heldObject == null || heldRb == null) return;

        heldObject.transform.SetParent(null);

        heldRb.isKinematic = false;
        heldRb.detectCollisions = true;

        Vector3 throwDir = (transform.forward + Vector3.up * 0.2f).normalized;
        heldRb.AddForce(throwDir * throwForce, ForceMode.Impulse);

        heldObject = null;
        heldRb = null;
        isCarrying = false;
    }

    // Detecteer pickupable objecten
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickupable") || other.CompareTag("SpawnedPickup"))
        {
            nearbyObjects.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pickupable") || other.CompareTag("SpawnedPickup"))
        {
            nearbyObjects.Remove(other.gameObject);
        }
    }
}