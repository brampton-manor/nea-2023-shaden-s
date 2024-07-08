using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;

public class InteractManager : MonoBehaviour
{
    [SerializeField] Camera camera;
    [SerializeField] PlayerController player;
    [SerializeField] GameObject InteractableWindow;
    [SerializeField] Image interactBar;
    [SerializeField] Image interactObject;
    [SerializeField] TMP_Text interactText;
    [SerializeField] private LayerMask pickupLayerMask;
    [SerializeField] PhotonView PV;

    GameObject lookingat;

    public float range = 2.5f;
    float interactDelay;
    float reviveKeyTime = 0f;

    bool reviving;

    public void DetectInteractable()
    {
        RaycastHit interactableHit;

        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out interactableHit, range, pickupLayerMask))
        {
            if (interactableHit.transform.TryGetComponent(out WeaponInteractable weaponInteractable))
            {
                weaponInteractable.HandleUI(player);
                if (Input.GetKeyDown(KeyCode.E)) weaponInteractable.AddToInventory(player);
            }
            else if (interactableHit.transform.TryGetComponent(out DoorInteractable doorInteractable))
            {
                doorInteractable.HandleUI(player);
                if (Input.GetKeyDown(KeyCode.E)) doorInteractable.RequestToggle();
            }
            else if (interactableHit.transform.TryGetComponent(out Ladder ladder))
            {
                ladder.HandleUI(player);
                if (Input.GetKeyDown(KeyCode.E)) player.Climb(ladder.transform);
            }
            else if (interactableHit.transform.TryGetComponent(out HealthItem healthItem))
            {
                healthItem.HandleUI(player);
                if (Input.GetKeyDown(KeyCode.E)) healthItem.HealPlayer(player);
            }
            else if (interactableHit.transform.TryGetComponent(out ArmourInteractable armourInteractable))
            {
                armourInteractable.HandleUI(player);
                if (Input.GetKeyDown(KeyCode.E)) armourInteractable.EquipArmour(player);
            }
            else if (interactableHit.transform.TryGetComponent(out PlayerController playerController))
            {
                if (playerController.isDowned)
                {
                    player.ReviveUI();
                    if (Input.GetKey(KeyCode.E))
                    {
                        interactObject.gameObject.SetActive(true);
                        interactText.text = "REVIVING";
                        reviving = true;
                        if (Time.time - reviveKeyTime > 5)
                        {
                            playerController.Revive();
                            interactBar.fillAmount = 1;
                        }
                    }
                }
            }
        }
        else InteractableWindow.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (PV.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                reviveKeyTime = Time.time;
                interactDelay = 5;
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                reviveKeyTime = 0;
                reviving = false;
            }
            if (reviving)
            {
                interactDelay -= Time.deltaTime;
                interactBar.fillAmount = interactDelay / 5;
            }
            else
            {
                interactObject.gameObject.SetActive(false);
            }
            DetectInteractable();
        }
    }
}
