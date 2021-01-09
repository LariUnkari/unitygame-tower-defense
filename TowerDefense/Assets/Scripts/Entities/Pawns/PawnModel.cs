using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

namespace Entities
{
    public class PawnModel : EntityModel
    {
        public Animator m_animator;
        public float m_runAnimationVelocity = 5f;
        private float m_runSpeedModifier;

        public Collider m_mainCollider;

        public Pawn Pawn { get; set; }
        public override IEntity Entity { get { return Pawn; } }

        public override void LinkToEntity(IEntity entity)
        {
            Pawn = (Pawn)entity;
            DBGLogger.Log(string.Format("Linked to entity {0}<{1}>", entity.GetObjectName(), entity.GetType()), this, this);
        }

        public void SetMovementSpeedModifier(float movementSpeed)
        {
            m_runSpeedModifier = m_runAnimationVelocity * movementSpeed / m_modelScale;
            if (m_animator != null)
                m_animator.speed = m_runSpeedModifier;
        }

        public override void OnUpdate(float deltaTime)
        {
            
        }
    }
}