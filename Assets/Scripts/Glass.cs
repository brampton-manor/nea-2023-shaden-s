using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Glass : MonoBehaviourPun
{
    public void Shatter(Vector3 position)
    {
        NetworkManager.Instance.ShatterGlass(position);
        Destroy(gameObject);
    }
}
