using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public class TowerModel : EntityModel
    {
        public string m_animParamInitName = "IsInit";
        protected int m_animParamInitID;
        public string m_animParamIdleName = "IsIdle";
        protected int m_animParamIdleID;
        public string m_animParamTrackingName = "IsTracking";
        protected int m_animParamTrackingID;
        public string m_animParamAttackName = "DoAttack";
        protected int m_animParamAttackID;

        public Transform m_turretRoot;
        private Transform m_trackingTarget;

        public AudioSource m_audioSourceTurret;
        public AudioClip m_sfxOnAttack;

        protected virtual void Awake()
        {
            m_animParamInitID = Animator.StringToHash(m_animParamInitName);
            m_animParamIdleID = Animator.StringToHash(m_animParamIdleName);
            m_animParamTrackingID = Animator.StringToHash(m_animParamTrackingName);
            m_animParamAttackID = Animator.StringToHash(m_animParamAttackName);
        }

        public void StartInit()
        {
            m_trackingTarget = null;

            if (m_animator != null)
                m_animator.SetTrigger(m_animParamInitID);
        }

        public void StartIdle()
        {
            m_trackingTarget = null;

            if (m_animator != null)
                m_animator.SetTrigger(m_animParamIdleID);
        }

        public void StartTracking(Transform target)
        {
            m_trackingTarget = target;

            if (m_animator != null)
                m_animator.SetTrigger(m_animParamTrackingID);
        }

        public void Attack()
        {
            if (m_animator != null)
                m_animator.SetTrigger(m_animParamAttackID);

            if (m_audioSourceTurret != null && m_sfxOnAttack != null)
                m_audioSourceTurret.PlayOneShot(m_sfxOnAttack);
        }

        public override void OnUpdate(float deltaTime)
        {
            if (m_turretRoot != null && m_trackingTarget != null)
                m_turretRoot.LookAt(m_trackingTarget);
        }
    }
}