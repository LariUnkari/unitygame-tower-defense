using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public class PawnModel : MonoBehaviour
    {
        public Animator m_animator;
        public float m_modelScale = 0.25f;
        public float m_runAnimationVelocity = 5f;
        private float m_runSpeedModifier;

        private void Start()
        {
            transform.localScale = Vector3.one * m_modelScale;
        }

        public void SetMovementSpeedModifier(float movementSpeed)
        {
            m_runSpeedModifier = m_runAnimationVelocity * movementSpeed / m_modelScale;
            if (m_animator != null)
                m_animator.speed = m_runSpeedModifier;
        }
    }
}