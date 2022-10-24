using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SingleShotGun : Gun
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform gunEnd;
    [SerializeField] private Ammo ammo;
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private KeyCode reloadKey;

    private PhotonView PV;
    private GunInfo info;
    public GunInfo Info => info;
    public AudioSource sound;

    private float nextFire;
    private WaitForSeconds shotDuration;

    private static readonly int reloadAnimKey = Animator.StringToHash("IsReloading");
    private static readonly int recoilAnimKey = Animator.StringToHash("Recoil");

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        info = (GunInfo)itemInfo;
        shotDuration = new WaitForSeconds(info.shotDuration);
        sound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKey(reloadKey) && !ammo.IsFullMagazine)
            StartCoroutine(Reload());
    }

    public override void Use()
    {
        if (Time.time > nextFire)
        {
            if (ammo.InMagazine > 0)
                Shoot();
        }
    }

    private void Shoot()
    {
        nextFire = Time.time + info.fireRate;
        StartCoroutine(ShotEffect());
        Vector3 ray = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(ray, cam.transform.forward, out RaycastHit hit, info.fireRange))
        {
            hit.collider.gameObject.GetComponentInParent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
            PV.RPC(nameof(RPC_Shoot), RpcTarget.All, hit.point, hit.normal);
        }
        ammo.UpdateMagazine();
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform); // parent controls it's bullet impact to avoid channel overload (keep stable connection between clients)
        }
    }

    private IEnumerator ShotEffect()
    {
        if (info.shotSound != null)
        {
            sound.clip = info.shotSound;
            sound.Play();
        }weaponAnimator.SetTrigger(recoilAnimKey);
        
        yield return shotDuration;
    }

    public IEnumerator Reload()
    {
        weaponAnimator.SetBool(reloadAnimKey, true);

        if (info.reloadSound != null)
        {
            sound.clip = info.reloadSound;
            sound.Play();
        }
        yield return new WaitForSeconds(info.reloadDuration);

        ammo.Reload();
        weaponAnimator.SetBool(reloadAnimKey, false);
    }
}
