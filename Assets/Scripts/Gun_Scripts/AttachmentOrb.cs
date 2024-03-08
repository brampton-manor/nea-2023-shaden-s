using System.Collections.Generic;
using UnityEngine;

public class AttachmentOrb : MonoBehaviour
{
    [SerializeField] private string orbName;
    [SerializeField] private GameObject attachmentPrefabs;

    private int currentAttachmentIndex = -1;

    private void Awake()
    {
        if (attachmentPrefabs.transform.childCount <= 1)
        {
            gameObject.SetActive(false);
        }

        EquipFirstAttachment();
    }

    public void ToggleAttachment()
    {
        if (currentAttachmentIndex != -1)
        {
            attachmentPrefabs.transform.GetChild(currentAttachmentIndex).gameObject.SetActive(false);
        }

        currentAttachmentIndex = (currentAttachmentIndex + 1) % attachmentPrefabs.transform.childCount;

        attachmentPrefabs.transform.GetChild(currentAttachmentIndex).gameObject.SetActive(true);
    }

    public void OnOrbClicked()
    {
        ToggleAttachment();
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

