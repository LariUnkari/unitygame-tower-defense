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
            GameObject go = GameObject.Instantiate(Resources.Load("MissionManager") as GameObject);
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

    private Map m_map;

    private MissionState m_missionState;
    private MissionData m_data;

    private Coroutine m_missionRoutine;
    private Dictionary<int, Coroutine> m_waveRoutines;

    public Map Map { get { return m_map; } }
    public MissionState MissionState { get { return m_missionState; } }
    public float MissionTime { get { return m_data.time; } }

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
            m_data.time += Time.deltaTime;
    }

    public void SetMap(Map map)
    {
        m_map = map;
    }

    public void StartMission(MissionPreset preset)
    {
        m_missionState = MissionState.Active;
        m_data = new MissionData(m_map, preset.settings, preset.waves);
        m_data.StartMission();

        m_missionRoutine = StartCoroutine(MissionRoutine());
    }

    public IEnumerator MissionRoutine()
    {
        float timeToNext;
        while (m_data.nextWaveIndex < m_data.waves.Count)
        {
            timeToNext = m_data.waves[m_data.nextWaveIndex].startTime - m_data.time;
            if (timeToNext > 0f)
                yield return new WaitForSeconds(timeToNext);

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
                StopCoroutine(wave);

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
    }

    public void EndWave(int index)
    {
        Coroutine routine;
        if (m_waveRoutines.TryGetValue(index, out routine))
        {
            m_waveRoutines.Remove(index);
            StopCoroutine(routine);
        }
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

        enemyPawn.Kill();
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
}
