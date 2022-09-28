using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] float mouseSensitivity;
    [SerializeField] float sprintSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float smoothTime;
    [SerializeField] float equipCooldownTime;
    [SerializeField] float maxHealth = 100f;
    [SerializeField] GameObject cameraHolder;
    [SerializeField] Item[] items;
    [SerializeField] Image healthBarImage;
    [SerializeField] GameObject UI;

    int itemIndex;
    int previousItemIndex = -1;
    Cooldown equipCooldown;

    float currentHealth;

    Rigidbody _rigidbody;
    PhotonView PV;

    float verticalLookRotation;
    bool isGrounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    PlayerManager playerManager;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        equipCooldown = new Cooldown(equipCooldownTime);
        currentHealth = maxHealth;
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>(); // get the player manager of controller to set respawn method
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        Look();
        CalculateMove();
        TryToJump();
        TryToEquip();
        TryToUseItem();
        CheckOutOfBounds();
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine)
            return;

        _rigidbody.MovePosition(_rigidbody.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    private void Start()
    {

        if (PV.IsMine)
        {
            EquipItem(0); // equip start item
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(_rigidbody);
            Destroy(UI);
        }
    }

    private void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);
        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    private void CalculateMove()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
    }

    private void TryToJump()
    {
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            _rigidbody.AddForce(transform.up * jumpForce);
        }
    }

    public void SetIsGrounded(bool _isGrounded)
    {
        isGrounded = _isGrounded;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("itemIndex") && !PV.IsMine && targetPlayer == PV.Owner)
            EquipItem((int)changedProps["itemIndex"]);
    }

    private void EquipItem(int index)
    {
        if (index == previousItemIndex) // to avoid hiding items by button double click
            return;

        itemIndex = index;
        items[itemIndex].itemGameObject.SetActive(true);
        if (previousItemIndex != -1)
            items[previousItemIndex].itemGameObject.SetActive(false);
        previousItemIndex = itemIndex;

        if(PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        equipCooldown.Reset();
    }

    private void TryToEquip()
    {
        if (equipCooldown.IsReady)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    EquipItem(i);
                    break;
                }
            }

            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                if (itemIndex >= items.Length - 1)
                    EquipItem(0);
                else
                    EquipItem(itemIndex + 1);
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                if (itemIndex <= 0)
                    EquipItem(items.Length - 1);
                else
                    EquipItem(itemIndex - 1);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        PV.RPC(nameof(RPC_TakeDamage), PV.Owner, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info) // method to send damage data to other clients
    {
        currentHealth -= damage;
        healthBarImage.fillAmount = currentHealth / maxHealth;
        if (currentHealth <= 0)
        {
            Die();
            PlayerManager.Find(info.Sender).GetKill();
        }
    }

    void Die()
    {
        playerManager.Die();
    }

    void TryToUseItem()
    {
        if (Input.GetMouseButton(0))
            items[itemIndex].Use();
    }

    void CheckOutOfBounds() // if player is out of level bounds, respawn
    {
        if (transform.position.y < -15f)
            Die();
    }
}
