using UnityEngine;
using Photon.Pun;

public class DoorInteractable : MonoBehaviour
{
    [SerializeField] GameObject Door;
    [SerializeField] AudioClip openSound;
    [SerializeField] float CustomRotation;
    public string doorID; 

    AudioSource audioSource;
    bool open = false;

    void Awake()
    {
        doorID = GenerateUniqueID(5);
        audioSource = GetComponent<AudioSource>();
    }

    string GenerateUniqueID(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        var uniqueID = new char[length];
        for (int i = 0; i < length; i++)
        {
            uniqueID[i] = chars[random.Next(chars.Length)];
        }
        return new string(uniqueID);
    }

    public void HandleUI(PlayerController player)
    {
        if (!open) player.SetInteractUI("Door", "Press E to open");
        else player.SetInteractUI("Door", "Press E to close");
    }

    public void RequestToggle()
    {
        NetworkManager.Instance.ToggleDoorRPC(doorID, !open);
    }

    public void ToggleDoor(bool newState)
    {
        open = newState;
        Door.transform.localRotation = open ? Quaternion.Euler(0f, CustomRotation, 0f) : Quaternion.Euler(0f, 0f, 0f);
        audioSource.PlayOneShot(openSound, 1f);
    }
}
