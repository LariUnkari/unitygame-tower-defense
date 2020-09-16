using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MissionWave
{
    public float startTime;
    public GameObject pawnPrefab;
    public int spawnCount;
    public float spawnInterval;
    public int spawnPathIndex;

    public MissionWave(float startTime, GameObject pawnPrefab, int spawnCount, float spawnInterval, int spawnPathIndex)
    {
        this.startTime = startTime;
        this.pawnPrefab = pawnPrefab;
        this.spawnCount = spawnCount;
        this.spawnInterval = spawnInterval;
        this.spawnPathIndex = spawnPathIndex;
    }
}