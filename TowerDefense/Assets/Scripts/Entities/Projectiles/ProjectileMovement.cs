using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

namespace Entities
{
    public abstract class ProjectileMovement : MonoBehaviour, IEntityLinkable
    {
        protected Projectile m_projectile;

        public IEntity Entity { get { return Projectile; } }
        protected Projectile Projectile { get { return m_projectile; } }

        protected virtual void Awake()
        {
            Projectile projectile = gameObject.GetComponent<Projectile>();
            if (projectile == null)
            {
                DBGLogger.LogError(string.Format("No {0} to link to!", typeof(Projectile)), DBGLogger.Mode.Everything);
                return;
            }

            LinkToEntity(projectile);
            projectile.LinkMovement(this);
        }

        public virtual void LinkToEntity(IEntity entity)
        {
            m_projectile = (Projectile)entity;
            DBGLogger.Log(string.Format("Linked to entity {0}<{1}>", entity.GetObjectName(), entity.GetType()), this, this);
        }

        public abstract void Init(float speed, float range);

        public abstract void Move(float deltaTime);
    }
}