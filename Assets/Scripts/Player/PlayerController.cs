using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using System;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] UnityEngine.UI.Image healthbarImage, armourBarImage;
    [SerializeField] TMP_Text healthbarText, armourbarText, ammo, waveText, lowReloadText, ItemName, InteractableName, PointsText, PoorText;
    [SerializeField] Image ItemIcon;
    [SerializeField] GameObject PlayerObj, ui, InteractableWindow, cameraHolder, itemHolder, AliveScreen, HitScreen;
    [SerializeField] public Canvas PauseMenu, SettingsMenu, EndGameScreen, Scoreboard, DownedScreen;
    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;

    [SerializeField] public Item[] items;

    [SerializeField] AudioClip holsterSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip shatterSound;

    AudioSource audioSource;

    public enum PlayerState {ALIVE, DOWNED, DEAD, PAUSED, UNPAUSED};
    public PlayerState state = PlayerState.ALIVE;
    public bool Controllable;

    int itemIndex;
    int previousItemIndex = -1;
    int currentAmmo;
    int maxAmmo;
    public int currentPoints;
    float climbSpeed;

    float verticalLookRotation;

    public bool grounded;
    public bool isReloading;
    public bool isAiming;
    public bool isDowned = false;
    public bool isDead = false;
    public bool isClimbing = false;
    public bool isInspecting;

    string currentAmmoString;
    string maxAmmoString;
    public string username;

    string currentState;
    string currentWave;
    string currentCountdown;

    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    Rigidbody rb;

    PhotonView PV;

    public float maxHealth = 100f;
    public float currentHealth;

    public float maxArmour = 100f;
    public float currentArmour;

    PlayerManager playerManager;

    void Awake()
    {
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();

        username = PhotonNetwork.NickName;

        Scoreboard.gameObject.SetActive(false);
        PoorDisable();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        climbSpeed = 5f;
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

        Controllable = true;
    }

    void Update()
    {
        if (!PV.IsMine)
            return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (state == PlayerState.DEAD) return;

        if (AreOtherPlayersDowned(players)) Die();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (state != PlayerState.PAUSED) Pause();
            else Resume();
        }

        if (isDowned)
        {
            Look();
        }

        if (Controllable)
        {

            Look();
            HandleClimbing();
            if (!isClimbing)
            {
                Move();
                Jump();
            }

            currentPoints = playerManager.Points();

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
            SetItemUI();
            UpdateHealthBar();
            UpdateArmourBar();
            currentAmmo = items[itemIndex].GetAmmo();
            currentAmmoString = currentAmmo.ToString();
            maxAmmo = items[itemIndex].GetMaxAmmo();
            maxAmmoString = maxAmmo.ToString();

            ammo.text = currentAmmoString + '/' + maxAmmoString;
            CheckReloadText();

            isReloading = items[itemIndex].GetReloadState();
            isAiming = items[itemIndex].GetAimState();
            isInspecting = items[itemIndex].GetInspectState();

            for (int i = 0; i < items.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()) && !isReloading && items[i].isAbleToBeUsed)
                {
                    EquipItem(i);
                    break;
                }
            }

            if (!isReloading && !isAiming && !isClimbing && !isInspecting)
            {
                if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
                {
                    int newIndex = itemIndex;
                    do
                    {
                        newIndex = (newIndex + 1) % items.Length;
                    } while (!items[newIndex].isAbleToBeUsed && newIndex != itemIndex);

                    if (items[newIndex].isAbleToBeUsed)
                    {
                        EquipItem(newIndex);
                    }
                }
                else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
                {
                    int newIndex = itemIndex;
                    do
                    {
                        newIndex = (newIndex - 1 + items.Length) % items.Length;
                    } while (!items[newIndex].isAbleToBeUsed && newIndex != itemIndex);

                    if (items[newIndex].isAbleToBeUsed)
                    {
                        
                        EquipItem(newIndex);
                    }
                }
            }

            if (!isClimbing)
            {
                if (!isInspecting)
                {
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

                    if (Input.GetKeyDown(KeyCode.R) && !isReloading)
                    {
                        items[itemIndex].Reload();
                    }
                }

                if (Input.GetKeyDown(KeyCode.I) && !isReloading && !isAiming && !isInspecting)
                {
                    items[itemIndex].Inspect();
                }

                if (Input.GetKeyDown(KeyCode.I) && isInspecting)
                {
                    items[itemIndex].StopInspect();
                }
            }

            if (Input.GetKey(KeyCode.Tab)) Scoreboard.gameObject.SetActive(true);
            else Scoreboard.gameObject.SetActive(false);



        }

        if (transform.position.y < -15f)
        {
            TeleportToYLevel(1f);
            GoDowned();
        }

    }

    void TeleportToYLevel(float targetY)
    {
        Vector3 newPosition = transform.position;
        newPosition.y = targetY;
        transform.position = newPosition;
    }

    public void Resume()
    {
        PauseMenu.gameObject.SetActive(false);
        SettingsMenu.gameObject.SetActive(false);
        state = PlayerState.UNPAUSED;
        Controllable = true;
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }

    public void Pause()
    {
        PauseMenu.gameObject.SetActive(true);
        state = PlayerState.PAUSED;
        Controllable = false;
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
    }

    public void PlayClip(AudioClip clip)
    {
        audioSource.PlayOneShot(clip, 1f);
    }

    public void PlayShatter()
    {
        audioSource.PlayOneShot(shatterSound, 1f);
    }
    public void SetBuyInteractUI(string ItemName, int PointsValue)
    {
        InteractableWindow.gameObject.SetActive(true);
        InteractableName.text = ItemName;
        PointsText.text = "Press E - Buy for " + PointsValue + " points";

    }

    public void SetInteractUI(string Item, string Text)
    {
        InteractableWindow.gameObject.SetActive(true);
        InteractableName.text = Item;
        PointsText.text = Text;

    }

    public void PoorEnable()
    {
        PoorText.gameObject.SetActive(true);
        PoorText.text = "NOT ENOUGH POINTS";
        PoorText.color = Color.red;
        Invoke("PoorDisable", 2f);
    }

    public void AlreadyMaxHealth()
    {
        PoorText.gameObject.SetActive(true);
        PoorText.text = "NOTHING TO HEAL";
        PoorText.color = Color.yellow;
        Invoke("PoorDisable", 2f);
    }

    public void PoorDisable()
    {
        PoorText.gameObject.SetActive(false);
    }

    public void BoughtEnable()
    {
        PoorText.gameObject.SetActive(true);
        PoorText.text = "Added to Inventory";
        PoorText.color = Color.green;
        Invoke("BoughtDisable", 2f);
    }

    public void BoughtDisable()
    {
        PoorText.gameObject.SetActive(false);
    }

    public void HitEnable()
    {
        if(PV.IsMine) HitScreen.gameObject.SetActive(true);
    }

    public void HitDisable()
    {
        if(PV.IsMine) HitScreen.gameObject.SetActive(false);
    }

    public void AlreadyInInventoryEnable()
    {
        PoorText.gameObject.SetActive(true);
        PoorText.text = "Already in inventory";
        PoorText.color = Color.yellow;
        Invoke("AlreadyInInventoryDisable", 2f);
    }

    public void AlreadyInInventoryDisable()
    {
        PoorText.gameObject.SetActive(false);
    }

    public void SpendPoints(int points)
    {
        playerManager.ReducePoints(points);
    }

    public void SetItemUI()
    {
        ItemIcon.sprite = items[itemIndex].itemInfo.ItemIcon;
        ItemName.text = items[itemIndex].itemInfo.ItemName;
    }

    void Look()
    {
        if (!isInspecting)
        {
            transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

            verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

            cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
        }
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
            if(isClimbing) isClimbing = false;
            PlayClip(jumpSound);
            rb.AddForce(transform.up * jumpForce);
        }
    }

    void EquipItem(int _index)
    {
        if (!isClimbing && !isDead && !isDowned) {
            if (_index == previousItemIndex)
                return;

            PlayClip(holsterSound);

            itemIndex = _index;

            items[itemIndex].itemGameObject.SetActive(true);

            if (previousItemIndex != -1)
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
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("itemIndex") && !PV.IsMine && targetPlayer == PV.Owner)
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

        if(Controllable && !isClimbing) rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    public void TakeDamage(float damage)
    {
        PV.RPC(nameof(RPC_TakeDamage), PV.Owner, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info)
    {
        if (!isDead && !isDowned)
        {
            HitEnable();
            Invoke(nameof(HitDisable), 0.5f);
            if (currentArmour > 0)
            {
                if ((currentArmour - damage) < 0) currentArmour = 0;
                else currentArmour-= damage;
            }
            else currentHealth -= damage;
        }
        if (currentHealth <= 0 && !isDowned && !isDead) GoDowned();
    }

    void UpdateHealthBar()
    {
        healthbarImage.fillAmount = currentHealth / maxHealth;
        healthbarText.text = currentHealth.ToString();
    }

    void UpdateArmourBar()
    {
        armourBarImage.fillAmount = currentArmour / maxArmour;
        armourbarText.text = currentArmour.ToString();
    }

    void Die()
    {
        state = PlayerState.DEAD;
        Controllable = false;
        isDowned = false;
        isDead = true;

        EndGameScreen.gameObject.SetActive(true);
        DownedScreen.gameObject.SetActive(false);
        AliveScreen.gameObject.SetActive(false);
        itemHolder.gameObject.SetActive(false);

        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
    }

    public void GoDowned()
    {
        playerManager.Downed();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (!AreOtherPlayersDowned(players))
        {

            PV.RPC(nameof(GoDownedRPC), RpcTarget.AllBuffered);
            isDowned = true;
            state = PlayerState.DOWNED;
            Controllable = false;

            transform.localScale = new Vector3(1f, 0.5f, 1f);
            DisableUI();
        }
    }

    [PunRPC]
    public void GoDownedRPC()
    {
        isDowned = true;
        state = PlayerState.DOWNED;
        Controllable = false;
        itemHolder.gameObject.SetActive(false);
    }

    public void ReviveUI(PlayerController targetPlayer)
    {
        InteractableWindow.gameObject.SetActive(true);
        InteractableName.text = targetPlayer.username;
        PointsText.text = "Hold E to revive";

    }

    public void Revive()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
        PV.RPC(nameof(ReviveRPC), RpcTarget.AllBuffered, transform.localScale);
        isDowned = false;
        state = PlayerState.ALIVE;
        Controllable = true;
        currentHealth = 100;
    }

    [PunRPC]
    public void ReviveRPC(Vector3 revivedScale)
    {
        isDowned = false;
        ActivateUI();
        state = PlayerState.ALIVE;
        Controllable = true;
        currentHealth = 100;
        itemHolder.gameObject.SetActive(true);
        transform.localScale = revivedScale;
    }

    bool AreOtherPlayersDowned(GameObject[] players)
    {
        foreach (GameObject playerObj in players)
        {
            PlayerController playerController = playerObj.GetComponent<PlayerController>();

            if (playerController != null && !playerController.isDowned)
            {
                return false;
            }
                
        }
        return true;

    }

    private void ActivateUI()
    {
        if (PV.IsMine)
        {
            // Activate objects here
            DownedScreen.gameObject.SetActive(false);
            AliveScreen.gameObject.SetActive(true);
            itemHolder.gameObject.SetActive(true);
        }
    }

    private void DisableUI()
    {
        if (PV.IsMine)
        {
            // Activate objects here
            DownedScreen.gameObject.SetActive(true);
            AliveScreen.gameObject.SetActive(false);
            itemHolder.gameObject.SetActive(false);
        }
    }

    public string GetState()
    {
        return state.ToString();
    }

    void CheckReloadText()
    {
        if (currentAmmo <= Math.Ceiling(maxAmmo * 0.1))
        {
            lowReloadText.gameObject.SetActive(true);
            lowReloadText.text = "RELOAD";
            lowReloadText.color = Color.red;
        }
        else if (currentAmmo <= Math.Ceiling(maxAmmo * 0.25))
        {
            lowReloadText.gameObject.SetActive(true);
            lowReloadText.text = "LOW AMMO";
            lowReloadText.color = Color.yellow;
        }
        else
        {
            lowReloadText.gameObject.SetActive(false);
        }


    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public void Climb(Transform ladderTransform)
    {
        itemHolder.gameObject.SetActive(false);
        rb.isKinematic = true;

        Vector3 inFrontOfLadder = ladderTransform.position + ladderTransform.forward * 1.5f;
        transform.position = new Vector3(inFrontOfLadder.x + 0.4f, transform.position.y, inFrontOfLadder.z - 1.4f);

        isClimbing = true;

    }

    void HandleClimbing()
    {
        if (isClimbing)
        {
            float verticalInput = Input.GetAxis("Vertical");
            transform.Translate(Vector3.up * climbSpeed * verticalInput * Time.deltaTime);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("TopLadder") && isClimbing) StopClimbing();
    }

    void StopClimbing()
    {
        isClimbing = false;
        rb.isKinematic = false;
        itemHolder.SetActive(true);
    }
}