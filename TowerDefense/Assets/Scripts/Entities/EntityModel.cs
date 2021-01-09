using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public abstract class EntityModel : MonoBehaviour, IEntityLinkable
    {
        public float m_modelScale = 1f;

        public abstract IEntity Entity { get; }
        public abstract void LinkToEntity(IEntity entity);

        protected virtual void Start()
        {
            transform.localScale = Vector3.one * m_modelScale;
        }

        public abstract void OnUpdate(float deltaTime);
    }
}