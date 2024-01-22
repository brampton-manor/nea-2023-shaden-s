using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] Camera camera;
    [SerializeField] PlayerController player;
    [SerializeField] private LayerMask pickupLayerMask;

    GameObject lookingat;

    public float range = 2f;

    public void DetectInteractable()
    {
        RaycastHit interactableHit;

        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out interactableHit, range, pickupLayerMask))
        {
            if (interactableHit.transform.TryGetComponent(out WeaponInteractable weaponInteractable))
            {
                weaponInteractable.AddToInventory(player);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            DetectInteractable();
        }
    }
}
