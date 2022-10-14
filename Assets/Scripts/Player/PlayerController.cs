using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] private float equipCooldownTime;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Item[] items;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject weaponCamera;
    [SerializeField] private int itemLayerNumber;

    private int itemIndex;
    private int previousItemIndex = -1;
    private Cooldown equipCooldown;

    private float currentHealth;

    private PhotonView PV;
    private Rigidbody rb;
    private PlayerMovement playerMovement;
    private PlayerManager playerManager;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        equipCooldown = new Cooldown(equipCooldownTime);
        currentHealth = maxHealth;
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>(); // get the player manager of controller to set respawn method

        if (!PV.IsMine)
        {
            playerMovement.enabled = false;
        }
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        TryToEquip();
        TryToUseItem();
        CheckOutOfBounds();
    }

    private void Start()
    {
        EquipItem(0); // equip start item

        if (!PV.IsMine)
        {
            Destroy(mainCamera);
            Destroy(weaponCamera);
            Destroy(rb);
            Destroy(UI);
        }
        else
        {
            SetItemLevel();
        }
    }

    private void SetItemLevel()
    {
        foreach (var item in items) // set special item level for item camera to avoid clipping (only for PV owner)
        {
            foreach (Transform child in item.transform.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = itemLayerNumber;
        }
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
