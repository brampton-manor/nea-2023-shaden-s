using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Enemy : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField]public float maxHealth;
    public float currentHealth;

    public bool isDead;
    public abstract void TakeDamage(float damage);

    public abstract void Die();

}
