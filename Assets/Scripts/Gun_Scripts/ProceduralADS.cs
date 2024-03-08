using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralADS : MonoBehaviour
{
    [Header("Weapon / Camera")]
    [SerializeField] private Transform WeaponADSLayer;
    [SerializeField] Camera cam;

    [Header("Variables")]
    [SerializeField] private float smoothTime;
    // ADS
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;
    [SerializeField] private float offsetZ;
    // Inspect
    [SerializeField] private float inspectOffsetX;
    [SerializeField] private float inspectOffsetY;
    [SerializeField] private float inspectOffsetZ;
    // Inspect Rotation Offsets
    [SerializeField] private float inspectRotationX;
    [SerializeField] private float inspectRotationY;
    [SerializeField] private float inspectRotationZ;

    [SerializeField] private bool IsAiming = false;
    [SerializeField] private bool IsInspecting = false;

    [SerializeField] private float aimSpeed; // time to enter ADS
    [SerializeField] private float _defaultFOV; // FOV in degrees
    [SerializeField] private float zoomRatio; // 1/zoom times

    [Header("Keys")]
    [SerializeField] private KeyCode ADSKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode InspectKey = KeyCode.I;

    PhotonView PV;

    private Vector3 originalWeaponPosition;
    private Quaternion originalWeaponRotation;

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        originalWeaponPosition = WeaponADSLayer.localPosition;
        originalWeaponRotation = WeaponADSLayer.localRotation;
    }

    private void Update()
    {
        if (PV.IsMine)
        {
            myInput();
            HandleAimingAndInspecting();
        }
    }

    public void SetAimParameters(float speed, Vector3 offset, float zoom)
    {
        aimSpeed = speed;
        offsetX = offset.x;
        offsetY = offset.y;
        offsetZ = offset.z;
        zoomRatio = zoom;
    }

    private void HandleAimingAndInspecting()
    {
        if (IsAiming && !IsInspecting) // Ensure we cannot aim while inspecting
        {
            Vector3 aimPosition = new Vector3(offsetX, offsetY, offsetZ);
            WeaponADSLayer.localPosition = Vector3.Lerp(WeaponADSLayer.localPosition, aimPosition, Time.deltaTime * smoothTime);
            SetFieldOfView(Mathf.Lerp(cam.fieldOfView, _defaultFOV * zoomRatio, aimSpeed * Time.deltaTime));
        }
        else if (IsInspecting)
        {
            Vector3 inspectPosition = new Vector3(inspectOffsetX, inspectOffsetY, inspectOffsetZ);
            Quaternion inspectRotation = Quaternion.Euler(inspectRotationX, inspectRotationY, inspectRotationZ);
            WeaponADSLayer.localPosition = Vector3.Lerp(WeaponADSLayer.localPosition, inspectPosition, Time.deltaTime * 10f);
            WeaponADSLayer.localRotation = Quaternion.Lerp(WeaponADSLayer.localRotation, inspectRotation, Time.deltaTime * 10f);
        }
        else
        {
            WeaponADSLayer.localPosition = Vector3.Lerp(WeaponADSLayer.localPosition, originalWeaponPosition, Time.deltaTime * 10f);
            WeaponADSLayer.localRotation = Quaternion.Lerp(WeaponADSLayer.localRotation, originalWeaponRotation, Time.deltaTime * 10f);
            SetFieldOfView(Mathf.Lerp(cam.fieldOfView, _defaultFOV, aimSpeed * Time.deltaTime));
        }
    }

    private void myInput()
    {
        if (Input.GetKeyDown(InspectKey) && !IsAiming)
        {
            IsInspecting = !IsInspecting;
            if (IsInspecting) IsAiming = false; // Automatically disable aiming if we start inspecting
        }

        if (!IsInspecting)
        {
            if (Input.GetKeyDown(ADSKey)) IsAiming = true;
            if (Input.GetKeyUp(ADSKey)) IsAiming = false;
        }
    }

    void SetFieldOfView(float fov)
    {
        cam.fieldOfView = fov;
    }
}
