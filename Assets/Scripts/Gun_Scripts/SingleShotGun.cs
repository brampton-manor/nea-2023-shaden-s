using Photon.Pun;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Net.Mail;
using System.Collections.Generic;

public class SingleShotGun : Gun
{

    int currentAmmo;
    int magsize;
    float damage;

    float spreadFactor = 2f; // Adjust as needed
    float aimingSpreadFactor = 0.5f;

    private float currentDelay;
    private TrailRenderer BulletTrail;

    PhotonView PV;
    [SerializeField] PhotonView PlayerPV;
    [SerializeField] PlayerController player;

    [SerializeField] private List<List<GameObject>> attachmentsByOrb;

    [SerializeField] Camera cam;

    [SerializeField] GameObject orbs;

    [SerializeField] Transform firePoint;
    [SerializeField] Transform aimpoint;

    [SerializeField] TMP_Text reloadText;
    [SerializeField] Image reloadObject;
    [SerializeField] Image reloadBar;
    [SerializeField] Image hitmarker;
    [SerializeField] Image crithitmarker;

    [SerializeField] Transform cameraHolder;

    [SerializeField] Transform activeWeapon;
    [SerializeField] Vector3 weaponPosition;
    
    [SerializeField] AudioClip hitmarkerSound;
    [SerializeField] AudioClip killSound;

    [SerializeField] Dictionary<char, int> attachmentIndexes = new Dictionary<char, int>();
    [SerializeField] GameObject sights;
    [SerializeField] GameObject muzzles;
    [SerializeField] GameObject lasers;

    int orbLayerMask = 1 << 17;

    Vector3 currentRotation;
    Vector3 targetRotation;

    AudioSource audioSource;

    bool hitEnemy = false;
    bool hitCrit = false;
    bool hitDead = false;
    bool hitPlayer = false;
    bool hitItem = false;
    bool shooting, readyToShoot, reloading, aiming;

    public bool isInspecting;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        damage = ((GunInfo)itemInfo).damage;
        magsize = ((GunInfo)itemInfo).magSize;
        currentAmmo = magsize;
        allowButtonHold = ((GunInfo)itemInfo).allowButtonHold;
        BulletTrail = ((GunInfo)itemInfo).bulletTrail.GetComponent<TrailRenderer>();

        readyToShoot = true;

        audioSource = GetComponent<AudioSource>();

        orbs.gameObject.SetActive(false);
        reloadObject.gameObject.SetActive(false); // Hide UI
        HitDisable();
        CritHitDisable();
    }

    void Update()
    {
        if (PV.IsMine)
        {
            if (isInspecting)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, orbLayerMask))
                    {
                        if (hit.collider.TryGetComponent(out AttachmentOrb attachmentOrb)) attachmentOrb.OnOrbClicked();
                    }
                }
            }
            else
            {
                CheckAiming();
                if (reloading)
                {
                    currentDelay -= Time.deltaTime;
                    reloadBar.fillAmount = currentDelay / ((GunInfo)itemInfo).reloadTime;
                }
            }

        }

    }

    public void UpdateAimPoint(Transform newAimPoint)
    {
        aimpoint = newAimPoint;
    }

    public void UpdateFirePoint(Transform newFirePoint)
    {
        firePoint = newFirePoint;
    }


    void CheckAiming()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, ((GunInfo)itemInfo).returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, ((GunInfo)itemInfo).snappiness * Time.fixedDeltaTime);

        if (Input.GetMouseButton(1))
        {
            aiming = true;
        }
        else
        {
            aiming = false;
            //cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation, Quaternion.Euler(currentRotation), ((GunInfo)itemInfo).snappiness * Time.deltaTime);
            //activeWeapon.localRotation = Quaternion.Lerp(activeWeapon.transform.localRotation, Quaternion.Euler(currentRotation), ((GunInfo)itemInfo).snappiness * Time.deltaTime);
            //activeWeapon.localRotation = Quaternion.Euler(currentRotation);
        }
    }

    public void RecoilFire()
    {
        float xSpread = Random.Range(-spreadFactor, spreadFactor);
        float ySpread = Random.Range(-spreadFactor, spreadFactor);

        float aimingXSpread = Random.Range(-aimingSpreadFactor, aimingSpreadFactor);
        float aimingYSpread = Random.Range(-aimingSpreadFactor, aimingSpreadFactor);

        Vector3 recoil = aiming
            ? new Vector3(((GunInfo)itemInfo).recoilX, Random.Range(-((GunInfo)itemInfo).aimRecoilY, ((GunInfo)itemInfo).aimRecoilY), Random.Range(-((GunInfo)itemInfo).aimRecoilZ, ((GunInfo)itemInfo).aimRecoilZ))
            : new Vector3(((GunInfo)itemInfo).recoilX + (aiming ? aimingXSpread : xSpread), Random.Range(-((GunInfo)itemInfo).recoilY + (aiming ? aimingYSpread : ySpread), ((GunInfo)itemInfo).recoilY + (aiming ? aimingYSpread : ySpread)), Random.Range(-((GunInfo)itemInfo).recoilZ + (aiming ? aimingXSpread : xSpread), ((GunInfo)itemInfo).recoilZ + (aiming ? aimingXSpread : xSpread)));

        if (aiming)
        {
            targetRotation += recoil;
        }
        else
        {
            // Apply spread to rotation when not aiming
            targetRotation += recoil;
        }
    }
    void SetFieldOfView(float fov)
    {
        cam.fieldOfView = fov;
    }

    public override void Use()
    {
        if (currentAmmo == 0 && !reloading) Reload(); // Reload on click    
        if (readyToShoot && !reloading) Shoot();
    }

    public override int GetAmmo()
    {
        return currentAmmo;
    }

    public override int GetMaxAmmo()
    {
        return ((GunInfo)itemInfo).magSize;
    }

    public override bool GetReloadState()
    {
        return reloading;
    }

    public override bool GetInspectState()
    {
        return isInspecting;
    }

    public override bool GetAimState()
    {
        return aiming;
    }

    public override bool GetButtonHold()
    {
        return allowButtonHold;
    }

    void Shoot()
    {
        currentAmmo -= 1;
        readyToShoot = false;
        shooting = true;
        GetComponent<RecoilSystem>().ApplyRecoil();
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (aiming) ray.origin = aimpoint.transform.position;
        if (!aiming) ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit)) // Range?
        {
            if (hit.collider.gameObject.layer == 8) hitItem = true;
            else if (hit.collider.gameObject.layer == 19) hit.collider.GetComponent<IDamageable>().TakeDamage(damage); // Barrel Layer
            else if (hit.collider.gameObject.tag == "EnemyCrit")
            {
                hitEnemy = true;
                hitCrit = true;
            }
            else if (hit.collider.gameObject.layer == 7) hitEnemy = true;
            else if (hit.collider.gameObject.layer == 3) hitPlayer = true;
            else if (hit.collider.gameObject.layer == 12) // Glass Layer
            {
                hit.collider.gameObject.GetComponent<Glass>().Shatter(hit.point);
                player.PlayShatter();
            }

            if (hitEnemy || hitCrit)
            {
                if (hit.collider.gameObject.GetComponentInParent<Enemy>()?.GetHealth() <= 0) hitDead = true;

                if (hitCrit) hit.collider.gameObject.GetComponentInParent<IDamageable>()?.TakeDamage(damage * 1.5f);
                else hit.collider.gameObject.GetComponentInParent<IDamageable>()?.TakeDamage(damage);
            }
            
        }
        else hit.point = ray.GetPoint(50);

        PV.RPC(nameof(RPC_Shoot), RpcTarget.All, hit.point, hit.normal);

    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        PlayClip(((GunInfo)itemInfo).shootSound);
        GameObject muzzleFlash = Instantiate(((GunInfo)itemInfo).muzzleFlash, firePoint.position, Quaternion.identity);
        GameObject bulletShell = Instantiate(((GunInfo)itemInfo).bulletShell, firePoint.position, Quaternion.identity);
        TrailRenderer trail = Instantiate(BulletTrail, firePoint.position, Quaternion.identity);
        if (gameObject.activeInHierarchy) StartCoroutine(SpawnTrail(trail, hitPosition));
        Destroy(muzzleFlash, 0.5f);
        Destroy(bulletShell, 0.5f);

        if (hitItem) hitItem = false;
        else
        {
            if (hitEnemy || hitPlayer || hitCrit)
            {
                Instantiate(((GunInfo)itemInfo).bloodImpact, hitPosition, Quaternion.identity);
                if (!hitDead)
                {
                    if (hitCrit)
                    {
                        PlayClip(hitmarkerSound);
                        CritHitActive();
                        Invoke("CritHitDisable", 0.2f);
                    }
                    else
                    {
                        PlayClip(hitmarkerSound);
                        HitActive();
                        Invoke("HitDisable", 0.2f);
                    }
                }
            }
            else
            {
                Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
                if (colliders.Length != 0)
                {
                    GameObject bulletImpactObj = Instantiate(((GunInfo)itemInfo).bulletImpact, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * ((GunInfo)itemInfo).bulletImpact.transform.rotation);
                    Destroy(bulletImpactObj, 10f);
                    bulletImpactObj.transform.SetParent(colliders[0].transform);
                }
            }
        }

        // Reset hit flags and invoke ResetShot
        hitEnemy = false;
        hitPlayer = false;
        hitCrit = false;
        hitDead = false;
        Invoke("ResetShot", ((GunInfo)itemInfo).timeBetweenShots);
    }

    public void UpdateAttachments(string identifier, int index)
    {
        PV.RPC(nameof(RPC_UpdateAttachments), RpcTarget.All, identifier, index);
    }

    [PunRPC]
    void RPC_UpdateAttachments(string identifier, int index)
    {
        if (identifier == "s") UpdateAttachment(sights, index);
        else if (identifier == "m") UpdateAttachment(muzzles, index);
        else if (identifier == "l") UpdateAttachment(lasers, index);
        else return;
    }

    void UpdateAttachment(GameObject attachmentPrefabs, int index)
    { 
        attachmentPrefabs.transform.GetChild(index).gameObject.SetActive(false);
        index = (index + 1) % attachmentPrefabs.transform.childCount;
        attachmentPrefabs.transform.GetChild(index).gameObject.SetActive(true);
    }

    IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 hitPoint)
    {
        float time = 0;
        Vector3 startPosition = firePoint.position;

        while (time < 1)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, hitPoint, time);
            time += Time.deltaTime / Trail.time;

            yield return null;
        }
        Trail.transform.position = startPosition;

        Destroy(Trail.gameObject, Trail.time);
    }

    void HitActive()
    {
        hitmarker.gameObject.SetActive(true);
    }

    void HitDisable()
    {
        hitmarker.gameObject.SetActive(false);
    }

    void CritHitActive()
    {
        crithitmarker.gameObject.SetActive(true);
    }

    void CritHitDisable()
    {
        crithitmarker.gameObject.SetActive(false);
    }


    private void ResetShot()
    {
        readyToShoot = true;
        shooting = false;
    }

    public override void Reload()
    {
        if (currentAmmo < magsize)
        {
            reloading = true;
            reloadObject.gameObject.SetActive(true);
            currentDelay = ((GunInfo)itemInfo).reloadTime;
            readyToShoot = false;
            shooting = false;
            reloadText.text = "RELOADING";
            Invoke("ReloadFinished", ((GunInfo)itemInfo).reloadTime);
        }

    }

    private void ReloadFinished()
    {
        reloadObject.gameObject.SetActive(false);
        reloadBar.fillAmount = 1;
        currentAmmo = ((GunInfo)itemInfo).magSize;
        reloading = false;
        readyToShoot = true;
    }

    public void PlayClip(AudioClip clip)
    {
        audioSource.PlayOneShot(clip, 0.9f);
    }

    public override void Inspect()
    {
        orbs.gameObject.SetActive(true);

        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;

        isInspecting = true;
    }

    public override void StopInspect()
    {
        orbs.gameObject.SetActive(false);

        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        isInspecting = false;
    }
}