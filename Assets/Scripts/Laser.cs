using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] PhotonView PV;
    [SerializeField] Camera cam;
    [SerializeField] Transform laserOrigin;
    LineRenderer laser;

    private void OnEnable()
    {
        laser = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        Vector3 endPoint = transform.position + transform.forward * 50;
        laser.SetPosition(0, transform.position);
        if (PV.IsMine)
        {

            if (cam != null)
            {
                Vector3 rayOrigin = cam.transform.position + cam.transform.forward * 0.15f; //Cam is used for the player so the laser points to the centre of the screen for accurate aiming
                Ray ray = new Ray(rayOrigin, cam.transform.forward);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    endPoint = hit.point;
                }
            }

        }
        else
        {
            Vector3 rayOrigin = laserOrigin.transform.position + laserOrigin.transform.forward * 0.15f; //Laserorigin is simply the laser origin and its used so other clients wont see the laser go through objects
            Ray ray = new Ray(rayOrigin, laserOrigin.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                endPoint = hit.point;
            }
        }
        laser.SetPosition(1, endPoint);

    }
}

