using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : Enemy
{
    public float speed = 1.5f;

    public float minAngle = 0f;
    public float maxAngle = 0f;

    public NavMeshAgent agent;

    public PhotonView PV;

    [HideInInspector] public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;


    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
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
        if (players.Length == 0) return;
        player = FindNearestPlayer(players);
        if (player == null)
        {
            return;
        }
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();

        

        //AI
        //if (health > 0){
        //RotateToTarget();
        //transform.Translate(Vector3.forward * Time.deltaTime * speed);
        //}
    }

    private Transform FindNearestPlayer(GameObject[] players)
    {
        Transform nearestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (GameObject playerObj in players)
        {
            float distance = Vector3.Distance(transform.position, playerObj.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPlayer = playerObj.transform;
            }
        }

        return nearestPlayer;
    }

    [PunRPC]
    void PatrolingRPC()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    void Patroling()
    {
        PV.RPC("PatrolingRPC", RpcTarget.All);
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
        if (PV != null && PV.IsMine) // Check if the PhotonView exists and is owned by the local player
        {
            agent.SetDestination(player.position);
        }
    }
    private void ChasePlayer()
    {
        PV.RPC("ChasePlayerRPC", RpcTarget.AllBuffered);
    }

    void AttackPlayer()
    {
        PV.RPC("AttackPlayerRPC", RpcTarget.All);
    }

    [PunRPC]
    void AttackPlayerRPC()
    {

        if (!PV.IsMine)
            return;
        // Make sure enemy doesn't move
        agent.SetDestination(transform.position);

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

    void DieRPC()
    {
        isDead = true;
        agent.SetDestination(transform.position);
        agent.speed = 0;
        speed = 0;

        Destroy(gameObject, 3f);
    }

    public override void Die()
    {
        // Call the DieRPC method over the network
        PV.RPC("DieRPC", RpcTarget.All);
    }

    public static PlayerManager Find(Player player)
    {
        return FindObjectsOfType<PlayerManager>().SingleOrDefault(x => x.PV.Owner == player);
    }


}