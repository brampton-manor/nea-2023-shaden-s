using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AttachmentOrb : MonoBehaviourPunCallbacks
{
    [SerializeField] SingleShotGun gun;
    [SerializeField] string attachmentType; // Unique identifier for the attachment type, e.g., 'S' for sights
    [SerializeField] private GameObject attachmentPrefabs;

    public int orbIndex;

    private int currentAttachmentIndex = 0;

    private void Awake()
    {
        if (attachmentPrefabs.transform.childCount <= 1)
        {
            gameObject.SetActive(false);
        }
        EquipFirstAttachment();
    }

    public void OnOrbClicked()
    {
        currentAttachmentIndex = (currentAttachmentIndex + 1) % attachmentPrefabs.transform.childCount;
        gun.UpdateAttachments(attachmentType, currentAttachmentIndex);
    }

    private void EquipFirstAttachment()
    {
        int childCount = attachmentPrefabs.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            attachmentPrefabs.transform.GetChild(i).gameObject.SetActive(false);
        }

        if (childCount > 0)
        {
            attachmentPrefabs.transform.GetChild(0).gameObject.SetActive(true);
            currentAttachmentIndex = 0; 
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

