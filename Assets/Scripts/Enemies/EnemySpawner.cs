using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using System.Linq;

public class EnemySpawner : MonoBehaviourPunCallbacks
{
    public static EnemySpawner Instance;
    public enum SpawnState { SPAWNING, WAITING, COUNTING, COMPLETED};
    //VARIABLES
    [SerializeField] private Wave[] waves;

    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] public float waveCountDown = 0;

    public SpawnState state = SpawnState.COUNTING;

    public int currentWave;

    //[SerializeField] TMP_Text waveText;

    PhotonView PV;

    //REFERENCES
    [SerializeField] private Transform[] spawners;
    [SerializeField] private List<Enemy> enemyList;

    void Awake()
    {
        Instance = this;
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        waveCountDown = timeBetweenWaves;
        currentWave = 0;
    }

    public string GetState()
    {
        return state.ToString();
    }

    public string GetWave()
    {
        return (currentWave + 1).ToString();
    }

    public string GetCountdown()
    {
        return (Math.Ceiling(waveCountDown)).ToString();
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (state == SpawnState.COMPLETED) return;
            if (state == SpawnState.WAITING)
            {
                //waveText.text = "WAVE " + currentWaveString;
                //CHECK IF ALL ENEMIES DEAD
                if (!EnemiesAreDead())
                    return;
                else
                    CompleteWave();
            }

            if (waveCountDown <= 0)
            {
                //Spawn Enemies
                if (state != SpawnState.SPAWNING)
                {
                    //SPAWN ENEMIES
                    PV.RPC("SpawnWaveRPC", RpcTarget.All);
                }
            }
            else
            {
                //waveText.text = waveCountDown.ToString();
                waveCountDown -= Time.deltaTime;
            }
            //if(Input.GetKeyDown(KeyCode.P))
            //{
            //SpawnZombie();
            //}
        }
    }


    [PunRPC]
    public void SpawnWaveRPC()
    {
        StartCoroutine(SpawnWave(waves[currentWave]));
    }

    IEnumerator SpawnWave(Wave wave)
    {
        state = SpawnState.SPAWNING;

        enemyList.Clear();

        for (int i = 0; i < wave.enemiesAmount; i++)
        {
            SpawnZombie();
            yield return new WaitForSeconds(wave.delay);
        }

        state = SpawnState.WAITING;

        yield break;
    }
    public void SpawnZombie()
    {
        if (PV.IsMine)
        {
            int randomInt = UnityEngine.Random.Range(1, spawners.Length);
            PV.RPC("SpawnZombieRPC", RpcTarget.MasterClient, randomInt);
        }

    }

    [PunRPC]
    public void SpawnZombieRPC(int randomInt)
    {
        Transform randomSpawner = spawners[randomInt];
        GameObject newEnemy = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Zombie"), randomSpawner.position, randomSpawner.rotation);
        Enemy newEnemyStats = newEnemy.GetComponent<Enemy>();
        enemyList.Add(newEnemyStats);
    }

    bool EnemiesAreDead()
    {
        foreach (Enemy enemy in enemyList)
        {
            if (!enemy.isDead) 
                return false;
        }
        return true;
    }

    [PunRPC]
    public void CompleteWaveRPC()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var playerManager in FindAll())
            {
                playerManager.GetPoints(waves[currentWave].pointReward);
            }
        }
        state = SpawnState.COUNTING;
        waveCountDown = timeBetweenWaves;

        if (currentWave + 1 > waves.Length - 1)
        {
            state = SpawnState.COMPLETED;
            //waveText.text = "ALL WAVES COMPLETED";
            //END GAME
        }
        else
        {
            currentWave++;
        }
    }
    void CompleteWave()
    {
        PV.RPC("CompleteWaveRPC", RpcTarget.All);
    }

    public static IEnumerable<PlayerManager> FindAll()
    {
        return FindObjectsOfType<PlayerManager>();
    }
}