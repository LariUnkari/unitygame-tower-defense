using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

public class Pawn : MonoBehaviour, IEntity
{
    protected PawnMovement m_movement;

    public int m_health = 100;

    public virtual string GetObjectName()
    {
        return name;
    }

    public virtual void Hit(int damage)
    {
        ApplyDamage(damage);
    }

    protected virtual void ApplyDamage(int damage)
    {
        m_health -= damage;
        DBGLogger.Log(string.Format("Applied {0} damage, current health is {1}",
            damage, m_health), this, this);

        if (m_health <= 0)
        {
            OnDeath();
        }
    }

    protected virtual void OnDeath()
    {
        DBGLogger.LogWarning("Died", this, this);
        gameObject.SetActive(false);
    }
}
