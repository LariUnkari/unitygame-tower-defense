using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New TowerPreset.asset", menuName = "ScriptableObject/Presets/TowerPreset")]
public class TowerPreset : ScriptableObject
{
    public string towerName;
    public GameObject baseModel;
    public GameObject weaponModel;
    public TowerWeaponPreset weaponPreset;
}
