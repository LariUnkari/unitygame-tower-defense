using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;
using Pixelplacement;

public class PawnMovement : MonoBehaviour, IEntityLinkable
{
    protected IEntity m_linkedEntity;

    public Spline m_pathSpline;
    protected float m_pathLength;
    //public Spline2DComponent m_pathSpline;
    
    public float m_moveSpeed = 1f;
    protected float m_pathDistanceMoved;
    protected float m_pathLengthMoved;

    public float m_lookAheadDistance = 0.1f;
    protected float m_pathLookAtDistance;
    protected float m_pathLookAtLength;

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
        if (m_pathSpline != null)
        {
            m_pathSpline.CalculateLength();
            m_pathLength = m_pathSpline.Length;
            DBGLogger.Log(string.Format("Path distance set to {0:F3}", m_pathLength), this, this);
        }
    }

    protected virtual void Update()
    {
        if (m_moveSpeed > 0f && m_pathLength > 0f && m_pathLengthMoved < 1f)
        {
            MoveOnPath();
        }
    }

    protected virtual void MoveOnPath()
    {
        float movement = m_moveSpeed * Time.deltaTime;
        m_pathDistanceMoved += movement;
        m_pathLengthMoved = m_pathDistanceMoved / m_pathLength;
        transform.position = m_pathSpline.GetPosition(m_pathLengthMoved, true);

        if (m_pathLengthMoved < 1f - m_lookAheadDistance)
        {
            m_pathLookAtDistance = m_pathDistanceMoved + m_lookAheadDistance;
            m_pathLookAtLength = m_pathLookAtDistance / m_pathLength;
            transform.LookAt(m_pathSpline.GetPosition(m_pathLookAtLength, true), Vector3.up);
        }

        //DBGLogger.Log(string.Format("Moved {0:F2} to position {1:F2} units along path {2}",
        //    movement, m_pathDistanceMoved, m_pathSpline.name), this, this);
    }
}
