using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class DoorInteractable : MonoBehaviourPun
{
    [SerializeField] GameObject Door;
    [SerializeField] AudioClip openSound;

    AudioSource audioSource;

    bool open = false;

    public PhotonView PV; 

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        PV = GetComponent<PhotonView>(); 
    }

    public void HandleUI(PlayerController player)
    {
        if (!open) player.SetInteractUI("Door", "Press E to open");
        else player.SetInteractUI("Door", "Press E to close");
    }

    public void ToggleDoor()
    {
        if (PV.IsMine) 
        {
            photonView.RPC("RPC_ToggleDoor", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_ToggleDoor()
    {
        if (!open)
        {
            Door.transform.localRotation = Quaternion.Euler(0f, -125f, 0f);
            audioSource.PlayOneShot(openSound, 1f);
            open = true;
        }
        else
        {
            Door.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            audioSource.PlayOneShot(openSound, 1f);
            open = false;
        }
    }
}
