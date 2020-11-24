using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public class PawnModel : EntityModel
    {
        public Animator m_animator;
        public float m_runAnimationVelocity = 5f;
        private float m_runSpeedModifier;

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