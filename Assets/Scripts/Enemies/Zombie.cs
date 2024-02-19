using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : Enemy
{
    public float speed;

    public float minAngle = 0f;
    public float maxAngle = 0f;

    public NavMeshAgent agent;

    public PhotonView PV;

    [HideInInspector] public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;


    //Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    bool prepareToAttack = false;
    public GameObject projectile;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    public int damage;

    bool dead = false;


    void Awake()
    {
        PV = GetComponent<PhotonView>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void Update()
    {
        if (currentHealth <= 0 && !isDead) Die();
        if (isDead) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 0) Patrolling();

        player = FindNearestPlayer(players);

        if (player == null || !IsPlayerValid(player))
        {
            Patrolling();
            return;
        }

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange && IsPlayerNextToEnemy()) AttackPlayer();
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

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    void Patrolling()
    {
        PV.RPC("PatrollingRPC", RpcTarget.All);
    }

    void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    [PunRPC]
    void ChasePlayerRPC()
    {
        if (PV != null && PV.IsMine && player != null) // Check if the PhotonView exists, is owned by the local player, and player is not null
        {
            agent.SetDestination(player.position);
            // Add your animation control here if needed
        }
    }

    private void ChasePlayer()
    {
        PV.RPC("ChasePlayerRPC", RpcTarget.AllBuffered);
    }

    void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        PV.RPC("AttackPlayerRPC", RpcTarget.All);

    }

    [PunRPC]
    void AttackPlayerRPC()
    {
        if (!PV.IsMine || player == null)
            return;

        // Make sure enemy doesn't move
        //GetComponent<Animator>().Play("Z_Attack");

        transform.LookAt(player);
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
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info)
    {
        if (!PV.IsMine)
            return;

        currentHealth -= damage;

        if (!dead && currentHealth <= 0)
        {
            dead = true;
            Die();
            PlayerManager.Find(info.Sender).GetKill(25);
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
        PV.RPC("DieRPC", RpcTarget.All);
    }

    public static PlayerManager Find(Player player)
    {
        return FindObjectsOfType<PlayerManager>().SingleOrDefault(x => x.PV.Owner == player);
    }

    public override float GetHealth()
    {
        return currentHealth;
    }
}