using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public ItemInfo itemInfo;
    public GameObject itemGameObject;
    public bool allowButtonHold;
    public bool isAbleToBeUsed;

    public abstract void Use();

    public abstract void Reload();

    public abstract int GetAmmo();

    public abstract int GetMaxAmmo();

    public abstract bool GetReloadState();

    public abstract bool GetAimState();

    public abstract bool GetButtonHold();
}
