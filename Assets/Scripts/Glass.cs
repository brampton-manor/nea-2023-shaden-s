using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Glass : MonoBehaviourPun
{
    [SerializeField] Transform brokenGlass;
    [SerializeField] AudioClip shatterSound;

    public PhotonView PV;

    AudioSource audioSource;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public void Shatter(Vector3 position)
    {
        if (PV.IsMine)
        {
            PV.RPC("RPC_ShatterGlass", RpcTarget.All, position);
        }
    }

    [PunRPC]
    void RPC_ShatterGlass(Vector3 position)
    {
        Transform shatteredGlass = Instantiate(brokenGlass, position, Quaternion.identity);
        foreach (Transform child in shatteredGlass)
        {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
            {
                rigidbody.AddExplosionForce(100f, position, 5f);
            }
        }
        Destroy(gameObject, 0.2f);
    }
}
