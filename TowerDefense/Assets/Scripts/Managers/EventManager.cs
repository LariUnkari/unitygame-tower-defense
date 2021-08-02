using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    public static event Action OnMissionStarted;
    public static void EmitOnMissionStarted() { OnMissionStarted(); }

    public static event Action<Presets.TowerPreset, Entities.Tower> OnTowerSpawned;
    public static void EmitOnTowerSpawned(Presets.TowerPreset preset, Entities.Tower tower) { OnTowerSpawned(preset, tower); }

    public static event Action<Entities.Projectile> OnProjectileSpawned;
    public static void EmitOnProjectileSpawned(Entities.Projectile projectile) { OnProjectileSpawned(projectile); }
}
