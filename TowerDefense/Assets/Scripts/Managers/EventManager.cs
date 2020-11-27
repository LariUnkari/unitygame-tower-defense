using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    public static event Action OnMissionStarted;
    public static void EmitOnMissionStarted() { OnMissionStarted(); }

    public static event Action<Entities.Tower> OnTowerSpawned;
    public static void EmitOnTowerSpawned(Entities.Tower tower) { OnTowerSpawned(tower); }
}
