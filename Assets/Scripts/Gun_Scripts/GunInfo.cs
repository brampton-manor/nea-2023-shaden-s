using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FPS/New Gun")]
public class GunInfo : ItemInfo
{
    [Header("Info")]
    public bool allowButtonHold;

    [Header("Graphics")]
    public GameObject projectile;
    public GameObject bulletTrail;
    public GameObject muzzleFlash;
    public GameObject bulletImpact;
    public GameObject bulletShell;
    public GameObject bloodImpact;


    [Header("Shooting")]
    public float damage;
    public float maxDistance;

    [Header("Reloading")]
    public int magSize;

    [Header("Recoil")]
    public float recoilX; // Normal Recoil
    public float recoilY;
    public float recoilZ;

    public float aimRecoilX; // ADS Recoil
    public float aimRecoilY;
    public float aimRecoilZ;

    public float snappiness;
    public float returnSpeed;

    [Tooltip("Time Between Shots")] public float timeBetweenShots;
    public float reloadTime;

    [Tooltip("Audio")]
    public AudioClip shootSound;

}
