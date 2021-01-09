using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

namespace Entities
{
    public class ProjectileModel : EntityModel
    {
        public Animator m_animator;

        public Projectile Projectile { get; set; }
        public override IEntity Entity { get { return Projectile; } }

        public override void LinkToEntity(IEntity entity)
        {
            Projectile = (Projectile)entity;
            DBGLogger.Log(string.Format("Linked to entity {0}<{1}>", entity.GetObjectName(), entity.GetType()), this, this);
        }

        public override void OnUpdate(float deltaTime)
        {

        }
    }
}