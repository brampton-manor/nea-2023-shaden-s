using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Destructible : MonoBehaviourPunCallbacks, IDamageable
{
    public abstract void TakeDamage(float damage);

    public abstract void Die();

}