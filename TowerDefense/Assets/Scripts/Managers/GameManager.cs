using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

public class GameManager : MonoBehaviour
{
    private static GameManager s_instance;

    public static GameManager GetInstance()
    {
        if (s_instance == null)
        {
            GameObject go = Instantiate(Resources.Load("GameManager") as GameObject);
            s_instance = go.GetComponent<GameManager>();

            if (s_instance == null)
            {
                DBGLogger.LogError("Instance has no component!", typeof(GameManager), go, (int)DBGLogger.Mode.Everything);
                Destroy(go);
                return null;
            }

            DBGLogger.LogWarning("Created instance!", s_instance, s_instance, (int)DBGLogger.Mode.Everything);
        }

        return s_instance;
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
    }

    private void OnDestroy()
    {
        if (s_instance == this) s_instance = null;
    }

    public void OnMapLoaded(Map map)
    {
        DBGLogger.LogWarning("Map loaded!", this, this, (int)DBGLogger.Mode.Everything);
        MissionManager.GetInstance().SetMap(map);
    }
}
