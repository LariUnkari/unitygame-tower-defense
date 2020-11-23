using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public abstract class EntityModel : MonoBehaviour
    {
        public Animator m_animator;
        public float m_modelScale = 1f;

        protected virtual void Start()
        {
            transform.localScale = Vector3.one * m_modelScale;
        }

        public abstract void OnUpdate(float deltaTime);
    }
}