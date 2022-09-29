using Photon.Pun;
using System.Collections;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;

    PhotonView PV;
    GunInfo info;

    private float nextFire;
    private LineRenderer laserLine;
    private WaitForSeconds shotDuration;
    public Transform gunEnd;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        laserLine = GetComponent<LineRenderer>();
        info = (GunInfo)itemInfo;
        shotDuration = new WaitForSeconds(info.shotDuration);
    }

    public override void Use()
    {
        if (Time.time > nextFire)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        nextFire = Time.time + info.fireRate;
        StartCoroutine(ShotEffect());
        Vector3 ray = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
        laserLine.SetPosition(0, gunEnd.position);
        if (Physics.Raycast(ray, cam.transform.forward, out RaycastHit hit, info.fireRange))
        {
            laserLine.SetPosition(1, hit.point);
            hit.collider.gameObject.GetComponentInParent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
            PV.RPC(nameof(RPC_Shoot), RpcTarget.All, hit.point, hit.normal);
        }
        else
        {
            laserLine.SetPosition(1, ray + (cam.transform.forward * info.fireRange));
        }
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
        if(info.gunAudio != null)
            info.gunAudio.Play();

        laserLine.enabled = true;
        yield return shotDuration;
        laserLine.enabled = false;
    }    
}
