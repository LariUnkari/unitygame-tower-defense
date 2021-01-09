using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

namespace Entities
{
    public class TowerBaseModel : EntityModel
    {
        public Animator m_animator;
        public Transform m_mountWeapon;

        public string m_animParamInitName = "IsInitializing";
        protected int m_animParamInitID;
        public string m_animParamIdleName = "IsIdle";
        protected int m_animParamIdleID;

        public Tower Tower { get; set; }
        public override IEntity Entity { get { return Tower; } }

        public override void LinkToEntity(IEntity entity)
        {
            Tower = (Tower)entity;
            DBGLogger.Log(string.Format("Linked to entity {0}<{1}>", entity.GetObjectName(), entity.GetType()), this, this);
        }


        protected virtual void Awake()
        {
            m_animParamInitID = Animator.StringToHash(m_animParamInitName);
            m_animParamIdleID = Animator.StringToHash(m_animParamIdleName);
        }

        public override void OnUpdate(float deltaTime)
        {

        }

        public void StartInit()
        {
            if (m_animator != null)
                m_animator.SetTrigger(m_animParamInitID);
        }

        public void StartIdle()
        {
            if (m_animator != null)
                m_animator.SetTrigger(m_animParamIdleID);
        }
    }
}