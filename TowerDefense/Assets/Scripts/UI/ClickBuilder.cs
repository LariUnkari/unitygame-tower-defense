using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities;
using DebugUtilities;

public class ClickBuilder : MonoBehaviour
{
    public Camera m_camera;

    private int m_presetIndex = -1;
    private Presets.TowerPreset m_towerPreset;

    private bool m_isPlacingPreset;
    private bool m_doUpdatePresetSelection;
    private bool m_areGUIStylesInitialized;

    private int m_uiSelectButtonSize = 100;
    private int m_uiBuildCostLabelHeight = 22;
    private int m_uiCycleButtonSize = 30;
    private int m_uiElementMargin = 5;
    private int m_uiAreaPadding = 10;
    private int m_uiStateLabelHeight = 22;

    private Rect m_towerAreaRect;
    private GUIStyle m_styleStateLabel;

    private Vector3 m_targetPosition;

    private Dictionary<int, float> m_towerBuildCooldowns;

    private void Awake()
    {
        InitTowerDatabase();

        m_towerAreaRect = new Rect();
        m_towerAreaRect.width = m_uiSelectButtonSize + m_uiElementMargin * 4 + m_uiCycleButtonSize * 2;
        m_towerAreaRect.height = m_uiSelectButtonSize + m_uiBuildCostLabelHeight + m_uiStateLabelHeight + m_uiElementMargin * 4;
    }

    private void Start()
    {
        m_presetIndex = GetTowerPresetIndex(0);
    }

    private void Update()
    {
        foreach (Presets.TowerPreset towerPreset in TowerPresetDatabase.GetInstance().towerPresets)
        {
            if (m_towerBuildCooldowns[towerPreset.GetInstanceID()] > 0f)
                m_towerBuildCooldowns[towerPreset.GetInstanceID()] -= Time.deltaTime;
        }

        if (m_isPlacingPreset && Input.GetMouseButtonDown(0))
        {
            Map map = MissionManager.GetInstance().Map;
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
            if (Math3D.VectorPlaneIntersect(ray.origin, ray.direction, map.transform.position, map.transform.up, out m_targetPosition))
            {
                Debug.DrawLine(ray.origin, m_targetPosition, Color.white, 1f);
                DBGLogger.Log(string.Format("Hit {0}<{1}> at {2:F3}", map.name, map.GetType(), m_targetPosition), this, this);

                if (MissionManager.GetInstance().MissionState == MissionState.Active)
                {
                    m_towerPreset = TowerPresetDatabase.GetInstance().GetPreset(m_presetIndex);

                    if (m_towerPreset == null)
                        return;

                    float cooldown = 0f;
                    m_towerBuildCooldowns.TryGetValue(m_towerPreset.GetInstanceID(), out cooldown);

                    if (cooldown > 0f)
                    {
                        DBGLogger.LogWarning(string.Format("Can't build preset, cooldown remains: {0}s", cooldown), this, this);
                        return;
                    }

                    if (m_towerPreset.buildCost > MissionManager.GetInstance().PlayerFunds)
                    {
                        DBGLogger.LogWarning(string.Format("Can't build preset, cost {0} > {1} funds", m_towerPreset.buildCost, MissionManager.GetInstance().PlayerFunds), this, this);
                        return;
                    }

                    PlaceTowerFromPreset(m_towerPreset, m_targetPosition);
                }
            }
        }
    }

    private void OnGUI()
    {
        if (!m_areGUIStylesInitialized)
        {
            m_styleStateLabel = new GUIStyle(GUI.skin.label);
            m_styleStateLabel.alignment = TextAnchor.MiddleCenter;
            m_areGUIStylesInitialized = true;
        }

        m_towerPreset = TowerPresetDatabase.GetInstance().GetPreset(m_presetIndex);

        m_towerAreaRect.x = Screen.width - (m_towerAreaRect.width + m_uiAreaPadding);
        m_towerAreaRect.y = m_uiAreaPadding;

        GUI.Box(m_towerAreaRect, "");

        if (GUI.Button(new Rect(
            m_towerAreaRect.xMin + m_uiElementMargin,
            m_towerAreaRect.yMin + m_uiElementMargin + m_uiSelectButtonSize / 2f - m_uiCycleButtonSize / 2f,
            m_uiCycleButtonSize, m_uiCycleButtonSize), "<"))
        {
            m_doUpdatePresetSelection = true;
            m_presetIndex--;
        }

        DrawPresetButton();

        if (GUI.Button(new Rect(
            m_towerAreaRect.xMax - m_uiElementMargin - m_uiCycleButtonSize,
            m_towerAreaRect.yMin + m_uiElementMargin + m_uiSelectButtonSize / 2f - m_uiCycleButtonSize / 2f,
            m_uiCycleButtonSize, m_uiCycleButtonSize), ">"))
        {
            m_doUpdatePresetSelection = true;
            m_presetIndex++;
        }

        GUI.Label(new Rect(
            m_towerAreaRect.center.x - (m_uiSelectButtonSize + m_uiCycleButtonSize) / 2f,
            m_towerAreaRect.yMin + m_uiSelectButtonSize + m_uiBuildCostLabelHeight + m_uiElementMargin * 3,
            m_uiSelectButtonSize + m_uiCycleButtonSize, m_uiStateLabelHeight),
            m_isPlacingPreset ? "Place Tower" : "Select Tower", m_styleStateLabel);

        if (m_doUpdatePresetSelection)
        {
            m_presetIndex = GetTowerPresetIndex(m_presetIndex);
            DBGLogger.Log(string.Format("Selected tower preset index: {0}", m_presetIndex), this);
            m_doUpdatePresetSelection = false;
        }
    }

    private void DrawPresetButton()
    {
        string towerName = "ERROR_NoTower";
        float buildCooldown = 0f;
        int towerCost = -1;

        if (m_towerPreset)
        {
            towerName = m_towerPreset.towerName.Length > 0 ? m_towerPreset.towerName : m_towerPreset.name;
            towerCost = m_towerPreset.buildCost;

            if (!m_towerBuildCooldowns.TryGetValue(m_towerPreset.GetInstanceID(), out buildCooldown))
            {
                DBGLogger.LogError(string.Format("Unable to find build cooldown for tower preset {0} '{1}'", m_towerPreset.GetInstanceID(), towerName), this, this);
            }
        }

        if (GUI.Button(new Rect(
            m_towerAreaRect.center.x - m_uiSelectButtonSize / 2f,
            m_towerAreaRect.yMin + m_uiElementMargin,
            m_uiSelectButtonSize, m_uiSelectButtonSize),
            string.Format("{0}\n{1}", towerName, buildCooldown > 0 ? string.Format("({0}s)", Mathf.Ceil(buildCooldown)) : "READY")))
        {
            m_isPlacingPreset = !m_isPlacingPreset;
        }

        GUI.Label(new Rect(
            m_towerAreaRect.center.x - m_uiSelectButtonSize / 2f,
            m_towerAreaRect.yMin + + m_uiSelectButtonSize + m_uiElementMargin * 2,
            m_uiSelectButtonSize, m_uiBuildCostLabelHeight),
            towerCost >= 0 ? string.Format("Cost: {0}", towerCost) : "FREE");
    }

    private void InitTowerDatabase()
    {
        m_towerBuildCooldowns = new Dictionary<int, float>();

        foreach (Presets.TowerPreset preset in TowerPresetDatabase.GetInstance().towerPresets)
        {
            m_towerBuildCooldowns[preset.GetInstanceID()] = 0f;
        }
    }


    private int GetTowerPresetIndex(int index)
    {
        return TowerPresetDatabase.GetInstance().GetPresetIndexRelative(index);
    }

    private void PlaceTowerFromPreset(Presets.TowerPreset towerPreset, Vector3 position)
    {
        m_towerBuildCooldowns[towerPreset.GetInstanceID()] = towerPreset.buildCooldown;

        GameObject go = Instantiate(TowerPresetDatabase.GetInstance().prototype, position, Quaternion.identity);
        Tower tower = go.GetComponent<Tower>();

        go = Instantiate(towerPreset.baseModel);
        tower.SetBaseModel(go.GetComponent<TowerBaseModel>());

        TowerWeapon weapon = tower.m_weaponMount.gameObject.AddComponent<TowerWeapon>();

        go = Instantiate(towerPreset.weaponModel);
        weapon.SetWeaponModel(go.GetComponent<TowerWeaponModel>());

        weapon.SetFromPreset(towerPreset.weaponPreset);
        tower.SetWeapon(weapon);

        EventManager.EmitOnTowerSpawned(towerPreset, tower);
    }
}
