using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] UnityEngine.UI.Image healthbarImage;
    [SerializeField] TMP_Text healthbarText;
    [SerializeField] TMP_Text ammo;
    [SerializeField] TMP_Text waveText;
    [SerializeField] GameObject ui;

    [SerializeField] GameObject cameraHolder;

    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;

    [SerializeField] Item[] items;

    int itemIndex;
    int previousItemIndex = -1;

    float verticalLookRotation;

    bool grounded;
    bool reloadState;

    string currentAmmoString;
    string maxAmmoString;

    string currentState;
    string currentWave;
    string currentCountdown;

    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    Rigidbody rb;

    PhotonView PV;

    const float maxHealth = 100f;
    float currentHealth = maxHealth;

    PlayerManager playerManager;

    void Awake()
    {
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();    
    }

    void Start()
    {
        if (PV.IsMine)
        {

            EquipItem(0);
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
            Destroy(ui);
        }
    }

    void Update()
    {
        if (!PV.IsMine)
            return;

        Look();
        Move();
        Jump();

        currentState = EnemySpawner.Instance.GetState();
        currentWave = EnemySpawner.Instance.GetWave();
        currentCountdown = EnemySpawner.Instance.GetCountdown();
        if (currentState == "COMPLETED")
        {
            waveText.text = "ALL WAVES COMPLETED";
        }
        else if (currentState == "COUNTING")
        {
            waveText.text = currentCountdown;
        }
        else
        {
            waveText.text = "WAVE " + currentWave;
        }

        //UI Updates
        currentAmmoString = items[itemIndex].GetAmmo().ToString();
        maxAmmoString = items[itemIndex].GetMaxAmmo().ToString();
        ammo.text = currentAmmoString + '/' + maxAmmoString;

        reloadState = items[itemIndex].GetReloadState();

        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i+1).ToString()) && !reloadState)
            {
                EquipItem(i);
                break;
            }
        }

        if(!reloadState)
        {
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                if (itemIndex >= items.Length - 1)
                {
                    EquipItem(0);
                }
                else
                {
                    EquipItem(itemIndex + 1);
                }
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                if (itemIndex <= 0)
                {
                    EquipItem(items.Length - 1);
                }
                else
                {
                    EquipItem(itemIndex - 1);
                }
            }
        }


        if (items[itemIndex].GetButtonHold())
        {
            if (Input.GetMouseButton(0))
            {
                items[itemIndex].Use();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                items[itemIndex].Use();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            items[itemIndex].Reload();
        }
        


        if (transform.position.y < -15f)
        {
            Die();
        }

    }

    void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }

    void EquipItem(int _index)
    {
        if(_index == previousItemIndex)
            return;

        itemIndex = _index;

        items[itemIndex].itemGameObject.SetActive(true);

        //ammo.text = items[itemIndex].GetAmmo().ToString();

        if(previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    void FixedUpdate()
    {
        if (!PV.IsMine)
            return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        if (!PV.IsMine)
            return;

        currentHealth -= damage;
        healthbarImage.fillAmount = currentHealth / maxHealth;
        healthbarText.text = currentHealth.ToString();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        playerManager.Die();
    }


}