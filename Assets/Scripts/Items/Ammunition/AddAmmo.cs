using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddAmmo : MonoBehaviour
{
    [SerializeField] private int amountOfAmmo;
    [SerializeField] private GunInfo gun;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if(string.Equals(other.tag, playerTag))
        {
            Ammo[] ammo = other.transform.root.GetComponentsInChildren<Ammo>(true);
            foreach (var a in ammo)
            {
                if (string.Equals(a.tag, gun.name))
                {
                    a.AddAmmo(amountOfAmmo);
                    Destroy(gameObject);
                }
            }
        }
    }
}
