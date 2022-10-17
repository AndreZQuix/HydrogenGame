using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FPS/New gun")]
public class GunInfo : ItemInfo
{
    public float damage;
    public float fireRate; // time between shots
    public float fireRange;
    public float hitForce; // amount of force applied to rigid body that has been shot
    public AudioClip shotSound;
    public AudioClip reloadSound;
    public float reloadDuration;
    public float shotDuration;
    public int maxAmmo;
    public int maxInMagazine;
}
