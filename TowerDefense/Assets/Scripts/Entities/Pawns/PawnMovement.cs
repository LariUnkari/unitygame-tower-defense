using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

public class PawnMovement : MonoBehaviour, IEntityLinkable
{
    protected IEntity m_linkedEntity;

    public Spline2DComponent m_pathSpline;
    public float m_moveSpeed = 1f;
    protected float m_pathDistanceMoved;

    public void LinkToEntity(IEntity entity)
    {
        m_linkedEntity = entity;
        DBGLogger.Log(string.Format("Linked to entity {0}<{1}>", entity.GetObjectName(), entity.GetType()), this, this);
    }

    protected virtual void Awake()
    {
        Pawn pawn = GetComponent<Pawn>();
        if (pawn == null)
        {
            DBGLogger.LogError(string.Format("No {0} to link to!", typeof(Pawn)), DBGLogger.Mode.Everything);
            return;
        }

        LinkToEntity(pawn);
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        if (m_pathSpline && m_moveSpeed > 0f)
        {
            MoveOnPath();
        }
    }

    protected virtual void MoveOnPath()
    {
        float movement = m_moveSpeed * Time.deltaTime;
        m_pathDistanceMoved += movement;
        transform.position = m_pathSpline.InterpolateDistanceWorldSpace(m_pathDistanceMoved);
        //DBGLogger.Log(string.Format("Moved {0:F2} to position {1:F2} units along path {2}",
        //    movement, m_pathDistanceMoved, m_pathSpline.name), this, this);
    }
}
