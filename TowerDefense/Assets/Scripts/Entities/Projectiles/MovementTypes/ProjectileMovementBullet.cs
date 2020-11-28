using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public class ProjectileMovementBullet : ProjectileMovement
    {
        protected float m_speed;
        protected float m_range;

        protected Vector3 m_movement;

        public override void Init(float speed, float range)
        {
            m_speed = speed;
            m_range = range;
        }

        public override void Move(float deltaTime)
        {
            m_movement = transform.forward * m_speed * deltaTime;

            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, m_movement, out hitInfo,
                m_movement.magnitude, m_projectile.Settings.hitMask, QueryTriggerInteraction.Collide))
            {
                EntityHitBox target = hitInfo.collider.GetComponentInParent<EntityHitBox>();
                if (target != null)
                {
                    HitTarget(target);
                    return;
                }
            }

            transform.Translate(m_movement, Space.World);
        }

        public void HitTarget(EntityHitBox target)
        {
            target.Hit(new Damage(m_projectile.Settings.damage, m_projectile, m_projectile.Shooter));
            m_projectile.Kill(new Damage(1, target.Entity, m_projectile));
        }
    }
}