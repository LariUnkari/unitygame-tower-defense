using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

public class ClickShooter : MonoBehaviour
{
    public int m_damage = 10;
    public Camera m_camera;
    public float m_scanDistance = 6f;
    public List<string> m_hitLayers;
    private int m_hitMask;
    
    private void Awake()
    {
        m_hitMask = 0;

        int layerIndex;
        foreach (string layerName in m_hitLayers)
        {
            layerIndex = LayerMask.NameToLayer(layerName);
            if (layerIndex >= 0)
                m_hitMask |= 1 << layerIndex;
            else
                DBGLogger.LogWarning(string.Format("Unable to find layer named {0}", layerName), this, this);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, m_scanDistance, m_hitMask, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawLine(ray.origin, hitInfo.point, Color.green, 1f);
                DBGLogger.Log(string.Format("Hit {0}<{1}> at {2:F3}",
                    hitInfo.collider.name, hitInfo.collider.GetType(), hitInfo.point), this, this);

                Entities.EntityHitBox hitBox = hitInfo.collider.GetComponent<Entities.EntityHitBox>();
                if (hitBox == null) {
                    DBGLogger.LogWarning(string.Format("Target has no {0} component!", typeof(Entities.EntityHitBox)), this, this);
                    return;
                }

                hitBox.Hit(new Entities.Damage(m_damage, null, null));
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * m_scanDistance, Color.red, 1f);
                DBGLogger.Log("Pew, missed!", this, this);
            }
        }
    }
}
