using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

namespace Entities
{
    public class EntityHitBox : MonoBehaviour, IEntityLinkable
    {
        protected IEntity m_linkedEntity;

        public IEntity Entity { get { return m_linkedEntity; } }

        public void LinkToEntity(IEntity entity)
        {
            m_linkedEntity = entity;
            DBGLogger.Log(string.Format("Linked to entity {0}<{1}>", entity.GetObjectName(), entity.GetType()), this, this);
        }

        protected virtual void Awake()
        {
            IEntity entity = gameObject.GetComponentInParent<IEntity>();
            if (entity == null)
            {
                DBGLogger.LogError(string.Format("No {0} to link to!", typeof(IEntity)), DBGLogger.Mode.Everything);
                return;
            }

            LinkToEntity(entity);
        }

        public void Hit(Damage damage)
        {
            if (m_linkedEntity == null)
            {
                DBGLogger.LogError(string.Format("No {0} linked, unable to apply {1} damage!",
                    typeof(IEntity), damage), DBGLogger.Mode.Everything);
                return;
            }

            m_linkedEntity.OnHit(damage);
        }
    }
}