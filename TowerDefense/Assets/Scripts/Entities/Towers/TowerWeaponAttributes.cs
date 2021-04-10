using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TowerWeaponAttributes
{
    public float range;
    public int damage;
    public float interval;
    public float projectileSpeed;
    public float projectileLifetime;
    public bool shootAllMuzzles;

    public TowerWeaponAttributes(float range, int damage, float interval, float projectileSpeed, float projectileLifetime, bool shootAllMuzzles)
    {
        this.range = range;
        this.damage = damage;
        this.interval = interval;
        this.projectileSpeed = projectileSpeed;
        this.projectileLifetime = projectileLifetime;
        this.shootAllMuzzles = shootAllMuzzles;
    }
}
