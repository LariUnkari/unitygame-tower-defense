using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New TowerWeaponPreset.asset", menuName = "ScriptableObject/Presets/TowerWeaponPreset")]
public class TowerWeaponPreset : ScriptableObject
{
    public LayerMask hitMask;
    public TowerWeaponAttributes attributes = new TowerWeaponAttributes(5f, 10, 0.2f, 10f, 1f, false);
    public GameObject projectilePrefab;
    public MuzzleEffectsPreset muzzleEffects;
    public AudioClip sfxOnTargetAcquired;
    public AudioClip sfxTracking;
}
