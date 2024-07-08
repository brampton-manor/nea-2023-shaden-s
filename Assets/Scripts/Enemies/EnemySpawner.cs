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
    public enum SpawnState { SPAWNING, WAITING, COUNTING, COMPLETED };

    [SerializeField] private Wave[] waves;

    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] public float waveCountDown = 0;

    public SpawnState state = SpawnState.COUNTING;

    public MapGenerator mapGenerator;

    public int currentWave;
    int enemyCount;
    int pointReward;
    int bruteZombiesToSpawn = 1;
    int bruteZombiesSpawned = 0;

    public List<Enemy> enemyList;

    PhotonView PV;

    public List<Vector3> spawners;

    bool spawnsGenerated = false;

    void Awake()
    {
        Instance = this;
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        waveCountDown = timeBetweenWaves;
        currentWave = 0;
        enemyCount = 3;
        pointReward = 100;

        mapGenerator = FindObjectOfType<MapGenerator>();
    }

    IEnumerator EnsureMapGeneratedBeforeSpawning()
    {
        while (!mapGenerator.mapGenerated)
        {
            yield return new WaitForSeconds(1);
        }

        spawners = mapGenerator.GetEdgeCellPositions();
        spawnsGenerated = true;
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
        if (!spawnsGenerated) StartCoroutine(EnsureMapGeneratedBeforeSpawning());
        if (PhotonNetwork.IsMasterClient)
        {
            if (state == SpawnState.COMPLETED) return;
            if (state == SpawnState.WAITING)
            {
                if (!EnemiesAreDead())
                    return;
                else
                    CompleteWave();
            }

            if (waveCountDown <= 0)
            {
                if (state != SpawnState.SPAWNING)
                {
                    PV.RPC("SpawnWaveRPC", RpcTarget.All);
                }
            }
            else
            {
                waveCountDown -= Time.deltaTime;
            }
        }
    }

    [PunRPC]
    public void SpawnWaveRPC()
    {
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        state = SpawnState.SPAWNING;

        enemyList.Clear();

        bruteZombiesSpawned = 0;

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnZombie();
            yield return new WaitForSeconds(2);
        }

        state = SpawnState.WAITING;

        yield break;
    }

    public void SpawnZombie()
    {
        if (PV.IsMine)
        {
            int randomInt = UnityEngine.Random.Range(1, spawners.Count);
            PV.RPC("SpawnZombieRPC", RpcTarget.MasterClient, randomInt, currentWave);
        }
    }

    [PunRPC]
    public void SpawnZombieRPC(int randomInt, int waveNumber)
    {
        Vector3 randomSpawner = spawners[randomInt];
        GameObject newEnemy = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Zombie"), randomSpawner, Quaternion.identity);
        Zombie newEnemyStats = newEnemy.GetComponent<Zombie>();
        newEnemyStats.SetSpeed(newEnemyStats.agent.speed, waveNumber); // Adjust speed based on wave number
        enemyList.Add(newEnemyStats);

        // Spawn brute zombies starting from wave 3 and every third wave thereafter
        if (currentWave >= 3 && (currentWave - 3) % 3 == 0 && bruteZombiesSpawned < bruteZombiesToSpawn)
        {
            GameObject bruteZombie = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BruteZombie"), randomSpawner, Quaternion.identity);
            enemyList.Add(bruteZombie.GetComponent<Enemy>());
            bruteZombiesSpawned++;
        }
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
                playerManager.GetPoints(pointReward);
            }
        }
        enemyCount += 3;
        pointReward += 150;

        state = SpawnState.COUNTING;
        waveCountDown = timeBetweenWaves;

        currentWave++;
        if (currentWave >= 3 && currentWave % 3 == 0) bruteZombiesToSpawn++;
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
