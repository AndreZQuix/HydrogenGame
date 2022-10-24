using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private SingleShotGun gun;

    public int AmmoAmount { get; private set; }
    public int InMagazine { get; private set; }
    public int InBandolier { get; private set; }

    public bool IsFullMagazine => InMagazine == gun.Info.maxInMagazine;

    private void Awake()
    {
        AmmoAmount = gun.Info.maxAmmo;
        InMagazine = gun.Info.maxInMagazine;
        InBandolier = AmmoAmount - InMagazine;
    }

    private void Update()
    {
        ammoText.text = InMagazine.ToString() + '/' + InBandolier.ToString();
    }

    public void UpdateMagazine()
    {
        InMagazine--;
        UpdateAmmoAmount();
    }

    public void Reload()
    {
        if (InBandolier >= gun.Info.maxInMagazine || InMagazine + InBandolier > gun.Info.maxInMagazine)
        {
            InBandolier -= gun.Info.maxInMagazine - InMagazine;
            InMagazine = gun.Info.maxInMagazine;
        }
        else
        {
            InBandolier = 0;
            InMagazine += InBandolier;
        }
    }

    public void AddAmmo(int ammo)
    {
        InBandolier += ammo;
        UpdateAmmoAmount();
    }

    private void UpdateAmmoAmount()
    {
        AmmoAmount = InMagazine + InBandolier;
    }
}
