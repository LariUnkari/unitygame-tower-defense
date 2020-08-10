using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

public class Pawn : MonoBehaviour, IEntity
{
    protected PawnMovement m_movement;

    public virtual string GetObjectName()
    {
        return name;
    }
}
