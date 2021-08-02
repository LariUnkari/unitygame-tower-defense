using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

public class MissionManager : MonoBehaviour
{
    private static MissionManager s_instance;

    public static MissionManager GetInstance()
    {
        if (s_instance == null)
        {
            GameObject go = Instantiate(Resources.Load("MissionManager") as GameObject);
            s_instance = go.GetComponent<MissionManager>();

            if (s_instance == null)
            {
                DBGLogger.LogError("Instance has no component!", typeof(MissionManager), go, (int)DBGLogger.Mode.Everything);
                Destroy(go);
                return null;
            }

            DBGLogger.LogWarning("Created instance!", s_instance, s_instance, (int)DBGLogger.Mode.Everything);
        }

        return s_instance;
    }

    public MissionPreset m_testMission;

    public int m_initialFunds = 500;
    private int m_currentFunds;

    private Map m_map;

    private MissionState m_missionState;
    private MissionData m_data;

    private Coroutine m_missionRoutine;
    private Dictionary<int, Coroutine> m_waveRoutines;

    public Map Map { get { return m_map; } }
    public MissionState MissionState { get { return m_missionState; } }
    public float MissionTime { get { return m_data.time; } }
    public float PlayerFunds { get { return m_currentFunds; } }

    private void OnEnable()
    {
        EventManager.OnTowerSpawned += OnTowerSpawned;
        EventManager.OnProjectileSpawned += OnProjectileSpawned;
    }

    private void OnDisable()
    {
        EventManager.OnTowerSpawned -= OnTowerSpawned;
        EventManager.OnProjectileSpawned -= OnProjectileSpawned;
    }

    private void OnDestroy()
    {
        if (s_instance == this) s_instance = null;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        {
            GUILayout.Label("Mouse position " + Input.mousePosition.ToString("F0"));
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 200f, Screen.height - 60f, 400f, 40f));
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (m_missionState == MissionState.Init)
                {
                    if (m_map == null)
                    {
                        GUILayout.Label("No map set!");
                    }
                    else if (m_testMission == null)
                    {
                        GUILayout.Label("No mission preset!");
                    }
                    else
                    {
                        if (GUILayout.Button("Start mission"))
                        {
                            StartMission(m_testMission);
                        }
                    }
                }
                else if (m_missionState == MissionState.Active)
                {
                    // Do nothing
                }
                else
                {
                    if (m_data.playerHealth > 0)
                    {
                        GUILayout.Label("VICTORY");
                    }
                    else
                    {
                        GUILayout.Label("DEFEAT");
                    }
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }

    private void Awake()
    {
        if (s_instance != null)
        {
            DBGLogger.LogError("Instance already existed!", this, this, (int)DBGLogger.Mode.Everything);
            Destroy(gameObject);
            return;
        }

        s_instance = this;
        m_waveRoutines = new Dictionary<int, Coroutine>();
    }

    private void Update()
    {
        if (m_missionState == MissionState.Active && m_data.playerHealth > 0)
            m_data.MissionUpdate(Time.deltaTime);
    }

    public void SetMap(Map map)
    {
        m_map = map;
    }

    public void StartMission(MissionPreset preset)
    {
        m_currentFunds = m_initialFunds;

        m_missionState = MissionState.Active;
        m_data = new MissionData(m_map, preset.settings, preset.waves);
        m_data.StartMission();

        m_missionRoutine = StartCoroutine(MissionRoutine());

        EventManager.EmitOnMissionStarted();
    }

    public IEnumerator MissionRoutine()
    {
        float timeToNext;
        while (m_data.nextWaveIndex < m_data.waves.Count)
        {
            timeToNext = m_data.waves[m_data.nextWaveIndex].startTime - m_data.time;
            if (timeToNext > 0f)
            {
                DBGLogger.Log(string.Format("Time {0} to start next wave {1} at {2} is {3}!", m_data.time,
                    m_data.nextWaveIndex, m_data.waves[m_data.nextWaveIndex].startTime, timeToNext),
                    this, this, DBGLogger.Mode.Everything);

                yield return new WaitForSeconds(timeToNext);
            }

            StartWave(m_data.nextWaveIndex);
            m_data.nextWaveIndex += 1;
        }

        m_data.nextWaveIndex = -1;
    }

    public void EndMission()
    {
        if (m_missionRoutine != null)
        {
            StopCoroutine(m_missionRoutine);
            m_missionRoutine = null;
        }

        if (m_waveRoutines.Count > 0)
        {
            foreach (Coroutine wave in m_waveRoutines.Values)
            {
                if (wave != null) StopCoroutine(wave);
            }

            m_waveRoutines.Clear();
        }

        DBGLogger.LogWarning("Mission ended!", this, this, DBGLogger.Mode.Everything);
        m_missionState = MissionState.Ended;
    }

    public void StartWave(int index)
    {
        m_data.activeWaveIndex = index;
        MissionWave wave = m_data.waves[index];
        m_waveRoutines.Add(index, StartCoroutine(WaveRoutine(index, wave)));

        DBGLogger.LogWarning(string.Format("Starting wave {0} at time {1}!", index, m_data.time), this, this, DBGLogger.Mode.Everything);
    }

    public void EndWave(int index)
    {
        DBGLogger.LogWarning(string.Format("Ending wave {0} at time {1}!", index, m_data.time), this, this, DBGLogger.Mode.Everything);
    }

    public IEnumerator WaveRoutine(int index, MissionWave wave)
    {
        float nextSpawnTime;
        int spawnIndex = 0;
        int pathIndex = wave.spawnPathIndex % m_map.m_paths.Length;
        while (spawnIndex < wave.spawnCount)
        {
            nextSpawnTime = wave.startTime + spawnIndex * wave.spawnInterval;
            if (nextSpawnTime > m_data.time)
                yield return new WaitForSeconds(nextSpawnTime - m_data.time);

            m_data.SpawnEnemy(wave.pawnPrefab, pathIndex, nextSpawnTime);
            spawnIndex++;
        }

        EndWave(index);
    }

    public void OnEnemyReachedPathEnd(Entities.Pawn enemyPawn)
    {
        if (m_missionState == MissionState.Active)
        {
            if (m_data.ApplyPlayerDamage(enemyPawn.m_playerDamage))
            {
                DBGLogger.LogWarning("Player died!", this, this, DBGLogger.Mode.Everything);
                EndMission();
            }
        }

        enemyPawn.Kill(new Entities.Damage(-1, null, null));
    }

    public void OnEnemyDied(Entities.Pawn enemyPawn)
    {
        m_data.RemoveEnemy(enemyPawn);

        if (m_missionState == MissionState.Active)
        {
            if (m_data.activeWaveIndex >= 0 && m_data.nextWaveIndex < 0 && m_data.EnemiesAlive == 0)
            {
                DBGLogger.LogWarning("All enemies dead!", this, this, DBGLogger.Mode.Everything);
                EndMission();
            }
        }
    }

    public ICollection<Entities.Pawn> GetAllEnemies()
    {
        return m_data.AllEnemies;
    }

    private void OnTowerSpawned(Presets.TowerPreset preset, Entities.Tower tower)
    {
        if (preset != null)
        {
            m_currentFunds -= preset.buildCost;
        }

        m_data.OnTowerSpawned(tower);
    }

    private void OnProjectileSpawned(Entities.Projectile projectile)
    {
        m_data.OnProjectileSpawned(projectile);
    }
}
