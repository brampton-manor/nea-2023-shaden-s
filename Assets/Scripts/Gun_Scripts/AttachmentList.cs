using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttachmentList
{
    public char identifier; // "S" for sights, "B" for barrels, etc.
    public List<GameObject> attachments;
}