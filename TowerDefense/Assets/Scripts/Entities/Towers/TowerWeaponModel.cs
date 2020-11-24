using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

namespace Entities
{
    public class TowerWeaponModel : EntityModel
    {
        public Animator m_animator;
        public string m_animParamInitName = "IsInitializing";
        protected int m_animParamInitID;
        public string m_animParamIdleName = "IsIdle";
        protected int m_animParamIdleID;
        public string m_animParamChargingName = "IsCharging";
        protected int m_animParamChargingID;
        public string m_animParamChargedName = "IsCharged";
        protected int m_animParamChargedID;
        public string m_animParamAttackName = "DoAttack";
        protected int m_animParamAttackID;

        public AudioSource m_audioSource;
        public AudioClip m_sfxOnAttack;

        protected virtual void Awake()
        {
            m_animParamInitID = Animator.StringToHash(m_animParamInitName);
            m_animParamIdleID = Animator.StringToHash(m_animParamIdleName);
            m_animParamChargingID = Animator.StringToHash(m_animParamChargingName);
            m_animParamChargedID = Animator.StringToHash(m_animParamChargedName);
            m_animParamAttackID = Animator.StringToHash(m_animParamAttackName);
        }

        public override void OnUpdate(float deltaTime)
        {

        }

        public virtual void StartInit()
        {
            if (m_animator != null)
                m_animator.SetTrigger(m_animParamInitID);
        }

        public virtual void StartIdle()
        {
            if (m_animator != null)
                m_animator.SetTrigger(m_animParamIdleID);
        }

        public virtual void StartCharging()
        {
            if (m_animator != null)
                m_animator.SetTrigger(m_animParamChargingID);
        }

        public virtual void SetCharged()
        {
            if (m_animator != null)
                m_animator.SetTrigger(m_animParamChargedID);
        }

        public virtual void LookAt(Vector3 position)
        {
            transform.LookAt(position);
        }

        public virtual void Attack()
        {
            if (m_animator != null)
                m_animator.SetTrigger(m_animParamAttackID);

            if (m_sfxOnAttack != null)
                PlaySFX(m_sfxOnAttack);
        }

        protected virtual void PlaySFX(AudioClip audioClip)
        {
            if (m_audioSource == null)
            {
                DBGLogger.LogError(string.Format("No {0} component found!", typeof(AudioSource)), this, this);
                return;
            }

            m_audioSource.PlayOneShot(audioClip);
        }
    }
}