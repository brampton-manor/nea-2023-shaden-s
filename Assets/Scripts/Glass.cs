using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

public class Glass : MonoBehaviourPun
{
    [SerializeField] Transform brokenGlass;

    public string glassID;
    void Awake()
    {
        glassID = "Glass_" + transform.position.x + "_" + transform.position.z;
    }

    public void SyncShatter(Vector3 position)
    {
        Instantiate(brokenGlass, position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void Shatter(Vector3 position)
    {
        NetworkManager.Instance.ShatterGlass(position, glassID);
    }
}
