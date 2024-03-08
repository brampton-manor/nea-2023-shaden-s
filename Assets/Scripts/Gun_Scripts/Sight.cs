using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sight : MonoBehaviour
{
    [SerializeField] private float customAimSpeed;
    [SerializeField] private Vector3 customAimOffset; 
    [SerializeField] private float zoomRatio;

    [SerializeField] Transform aimPoint;

    [SerializeField] ProceduralADS proceduralADS;
    [SerializeField] SingleShotGun gun;

    private void OnEnable()
    {
        proceduralADS.SetAimParameters(customAimSpeed, customAimOffset, zoomRatio);
        gun.UpdateAimPoint(aimPoint);
    }
}
