using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

[CreateAssetMenu(fileName = "New TowerPresetDatabase.asset", menuName = "ScriptableObject/Databases/TowerPresetDatabase")]
public class TowerPresetDatabase : ScriptableObject
{
    private static TowerPresetDatabase s_instance;

    public GameObject prototype;
    public TowerPreset[] towerPresets;

    public static TowerPresetDatabase GetInstance()
    {
        if (s_instance == null)
        {
            ScriptableObject so = Instantiate(Resources.Load("TowerPresetDatabase") as ScriptableObject);
            s_instance = so as TowerPresetDatabase;

            if (s_instance == null)
            {
                DBGLogger.LogError("Instance has no component!", typeof(TowerPresetDatabase), so, (int)DBGLogger.Mode.Everything);
                Destroy(so);
                return null;
            }

            DBGLogger.LogWarning("Created instance!", s_instance, s_instance, (int)DBGLogger.Mode.Everything);
        }

        return s_instance;
    }

    public TowerPreset GetPreset(int index)
    {
        if (index < 0 || index > towerPresets.Length)
            return null;

        return towerPresets[index];
    }

    public int GetPresetIndexRelative(int askedIndex)
    {
        if (towerPresets.Length == 0)
            return -1;

        if (towerPresets.Length == 1)
            return 0;

        return askedIndex % towerPresets.Length;
    }
}
