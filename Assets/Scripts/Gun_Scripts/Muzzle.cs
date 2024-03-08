using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muzzle : MonoBehaviour
{
    [SerializeField] Transform firepoint;

    [SerializeField] SingleShotGun gun;

    private void OnEnable()
    {
        gun.UpdateFirePoint(firepoint);
    }
}
