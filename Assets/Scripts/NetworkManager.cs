using Photon.Pun;
using System.Linq;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    PhotonView PV;

    void Awake()
    {
        Instance = this;
        PV = GetComponent<PhotonView>();
    }

    public void ShatterGlass(Vector3 position, string GlassID)
    {
        PV.RPC(nameof(RPC_ShatterGlass), RpcTarget.All, position, GlassID);
    }

    [PunRPC]
    void RPC_ShatterGlass(Vector3 position, string GlassID)
    {
        var glasses = FindObjectsOfType<Glass>();
        var glass = glasses.FirstOrDefault(g => g.glassID == GlassID);
        glass.SyncShatter(position);
    }

    public void ToggleDoorRPC(string doorId, bool open)
    {
        PV.RPC(nameof(RPC_ToggleDoor), RpcTarget.All, doorId, open);
    }

    [PunRPC]
    void RPC_ToggleDoor(string doorId, bool open)
    {
        var doors = FindObjectsOfType<DoorInteractable>();
        var door = doors.FirstOrDefault(d => d.doorID == doorId);
        door.ToggleDoor(open);
    }

    public void ExplodeBarrelRPC(Vector3 position, string BarrelID)
    {
        PV.RPC(nameof(RPC_ExplodeBarrel), RpcTarget.All, position, BarrelID);
    }

    [PunRPC]
    void RPC_ExplodeBarrel(Vector3 position, string BarrelID)
    {
        var barrels = FindObjectsOfType<ExplosiveBarrel>();
        var barrel = barrels.FirstOrDefault(b => b.barrelID == BarrelID);
        barrel.SyncExplosion();
    }
}