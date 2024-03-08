using Photon.Pun;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    PhotonView PV;

    [SerializeField] Transform brokenGlass;

    void Awake()
    {
        Instance = this;
        PV = GetComponent<PhotonView>();
    }

    public void ShatterGlass(Vector3 position)
    {
        PV.RPC(nameof(RPC_ShatterGlass), RpcTarget.All, position);
    }

    [PunRPC]
    void RPC_ShatterGlass(Vector3 position)
    {
        Instantiate(brokenGlass, position, Quaternion.identity);
    }

    public void ToggleDoorRPC(string doorId, bool open)
    {
        PV.RPC(nameof(RPC_ToggleDoor), RpcTarget.All, doorId, open);
    }

    [PunRPC]
    void RPC_ToggleDoor(string doorId, bool open)
    {
        // Find door by ID and toggle it
        var doors = FindObjectsOfType<DoorInteractable>();
        var door = doors.FirstOrDefault(d => d.doorID == doorId);
        if (door != null) door.ToggleDoor(open);
    }

    public void ExplodeBarrelRPC(string barrelID)
    {
        PV.RPC(nameof(RPC_ExplodeBarrel), RpcTarget.All, barrelID);
    }

    [PunRPC]
    void RPC_ExplodeBarrel(string barrelID)
    {
        var barrels = FindObjectsOfType<ExplosiveBarrel>();
        var barrel = barrels.FirstOrDefault(b => b.barrelID == barrelID);
        if (barrel != null) barrel.Die();
    }
}