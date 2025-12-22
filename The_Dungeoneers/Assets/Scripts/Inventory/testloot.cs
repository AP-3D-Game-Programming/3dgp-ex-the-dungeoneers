using UnityEngine;

public class TestLoot : MonoBehaviour {
    public ItemData itemToGive; // Sleep hier je zwaard-item in

    void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            Inventory.instance.Add(itemToGive);
        }
    }
}