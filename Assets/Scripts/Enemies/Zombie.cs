using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : Enemy
{
    [SerializeField] int PointReward;

    public float minAngle = 0f;
    public float maxAngle = 0f;
    public NavMeshAgent agent;
    public PhotonView PV;
    [HideInInspector] public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    // Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    // States
    public float sightRange, attackRange;
    public int damage;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void Update()
    {
        if (isDead) return;
        if (currentHealth <= 0 && !isDead) Die();

        FindAndSetNearestPlayer();

        if (player == null)
        {
            Patrolling();
        }
        else
        {
            if (IsPlayerNextToEnemy()) AttackPlayer();
            else ChasePlayer();
        }
    }

    void FindAndSetNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        player = FindNearestPlayer(players);
    }

    private bool IsPlayerNextToEnemy()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            return distanceToPlayer <= attackRange;
        }
        return false;
    }

    private bool IsPlayerValid(Transform player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        return playerController != null && !playerController.isDowned && playerController.state != PlayerController.PlayerState.DEAD;
    }

    private Transform FindNearestPlayer(GameObject[] players)
    {
        Transform nearestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (GameObject playerObj in players)
        {
            Transform playerTransform = playerObj.transform;

            if (IsPlayerValid(playerTransform))
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPlayer = playerTransform;
                }
            }
        }

        return nearestPlayer;
    }

    [PunRPC]
    void PatrollingRPC()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    void Patrolling()
    {
        PV.RPC("PatrollingRPC", RpcTarget.AllBuffered);
    }

    void SearchWalkPoint()
    {
        // Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    [PunRPC]
    void ChasePlayerRPC()
    {
        if (PV != null && PV.IsMine && player != null) agent.SetDestination(player.position);
    }

    private void ChasePlayer()
    {
        PV.RPC("ChasePlayerRPC", RpcTarget.AllBuffered);
    }

    void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        PV.RPC("AttackPlayerRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void AttackPlayerRPC()
    {
        if (!PV.IsMine || player == null) return;

        Vector3 eulerAngles = transform.rotation.eulerAngles;
        float clampedZ = Mathf.Clamp(eulerAngles.z, minAngle, maxAngle);
        float clampedY = Mathf.Clamp(eulerAngles.y, minAngle, maxAngle);
        float clampedX = Mathf.Clamp(eulerAngles.x, minAngle, maxAngle);
        transform.rotation = Quaternion.Euler(clampedX, eulerAngles.y, clampedZ);

        if (!alreadyAttacked)
        {
            player.GetComponent<PlayerController>().TakeDamage(damage);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public override void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.AllBuffered, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info)
    {
        if (!PV.IsMine)
            return;

        currentHealth -= damage;

        if (!isDead && currentHealth <= 0)
        {
            isDead = true;
            Die();
            PlayerManager.Find(info.Sender).GetKill(PointReward);
        }
    }

    [PunRPC]
    public void DieRPC()
    {
        isDead = true;
        agent.speed = 0;
        agent.SetDestination(transform.position);

        Destroy(gameObject, 3f);
    }

    public override void Die()
    {
        PV.RPC("DieRPC", RpcTarget.AllBuffered);
    }

    public static PlayerManager Find(Player player)
    {
        return FindObjectsOfType<PlayerManager>().SingleOrDefault(x => x.PV.Owner == player);
    }

    public override float GetHealth()
    {
        return currentHealth;
    }

    public void SetSpeed(float baseSpeed, int waveNumber)
    {
        agent.speed = baseSpeed * Mathf.Pow(1.01f, waveNumber);
    }
}
