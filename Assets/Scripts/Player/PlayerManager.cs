using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;
using System.IO;
using UnityEngine;
using static EnemySpawner;

public class PlayerManager : MonoBehaviour
{
    public PhotonView PV;

    int points;
    int kills;
    int deaths;

    GameObject controller;
    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    void Start()
    {
        if (PV.IsMine)
        {
            CreateController();
        }
    }

    void CreateController()
    {
        Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
    }

    public void Downed()
    {
        deaths++;

        Hashtable hash = new Hashtable();
        hash.Add("deaths", deaths);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void GetKill(int pointsrecieved = 0)
    {
        PV.RPC(nameof(RPC_GetKill), PV.Owner, pointsrecieved);
    }

    [PunRPC]
    void RPC_GetKill(int pointsrecieved)
    {
        kills++;
        points = points + pointsrecieved;

        Hashtable hash = new Hashtable();
        hash.Add("kills", kills);
        hash.Add("points", points);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void GetPoints(int pointsrecieved = 0)
    {
        PV.RPC(nameof(RPC_GetPoints), PV.Owner, pointsrecieved);
    }

    [PunRPC]
    void RPC_GetPoints(int pointsrecieved)
    {
        points = points + pointsrecieved;

        Hashtable hash = new Hashtable();
        hash.Add("points", points);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public static PlayerManager Find(Player player)
    {
        return FindObjectsOfType<PlayerManager>().SingleOrDefault(x => x.PV.Owner == player);
    }

    public static IEnumerable<PlayerManager> FindAll()
    {
        return FindObjectsOfType<PlayerManager>();
    }

    public int Points()
    {
        return points;
    }

    public void ReducePoints(int pointslost = 0)
    {
        PV.RPC(nameof(RPC_ReducePoints), PV.Owner, pointslost);
    }

    [PunRPC]
    void RPC_ReducePoints(int pointslost)
    {
        points = points - pointslost;

        Hashtable hash = new Hashtable();
        hash.Add("points", points);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }


}
