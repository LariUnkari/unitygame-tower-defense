using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MissionData
{
    public Map map;
    public MissionSettings settings;
    public List<MissionWave> waves;

    public float time { get; set; }
    public int activeWaveIndex { get; set; }
    public int nextWaveIndex { get; set; }

    public int playerHealth { get; set; }

    private int enemiesAlive;
    private Dictionary<int, Entities.Pawn> enemiesDictionary;

    public int EnemiesAlive { get { return enemiesAlive; } }

    public MissionData(Map map, MissionSettings settings, List<MissionWave> waves)
    {
        this.map = map;
        this.settings = settings;
        this.waves = waves;
    }

    public void StartMission()
    {
        time = 0f;
        activeWaveIndex = -1;
        nextWaveIndex = 0;
        playerHealth = settings.playerHealthMax;
        enemiesAlive = 0;
        enemiesDictionary = new Dictionary<int, Entities.Pawn>();
    }

    public void SpawnEnemy(GameObject prefab, int pathIndex, float spawnTime)
    {
        GameObject instance = GameObject.Instantiate(prefab);
        Entities.Pawn pawn = instance.GetComponent<Entities.Pawn>();

        if (pawn == null)
        {
            Debug.LogError(string.Format("Unable to find {0} behaviour on an instance of {1}!", typeof(Entities.Pawn), prefab.name));
            Object.Destroy(instance);
            return;
        }

        enemiesAlive = enemiesDictionary.Count;
        enemiesDictionary.Add(pawn.GetInstanceID(), pawn);

        pawn.OnSpawned(map, pathIndex, spawnTime);
    }

    public void RemoveEnemy(Entities.Pawn pawn)
    {
        if (enemiesDictionary.ContainsKey(pawn.GetInstanceID()))
            enemiesDictionary.Remove(pawn.GetInstanceID());

        enemiesAlive = enemiesDictionary.Count;
    }

    public bool ApplyPlayerDamage(int amount)
    {
        playerHealth -= amount;
        return playerHealth <= 0;
    }
}
