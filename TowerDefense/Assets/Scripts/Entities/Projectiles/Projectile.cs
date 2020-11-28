using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

namespace Entities
{
    public abstract class Projectile : MonoBehaviour, IEntity
    {
        public ProjectileModel m_model;
        protected ProjectileMovement m_movement;

        public float m_initDuration = 1;

        protected bool m_isAlive;
        protected float m_spawnTime;
        protected float m_timeAlive;

        protected ProjectileSettings m_settings;

        public AudioSource m_audioSource;

        protected IEntity m_shooter;

        public ProjectileSettings Settings { get { return m_settings; } }
        public IEntity Shooter { get { return m_shooter; } }

        public float SpawnTime { get { return m_spawnTime; } }
        public float TimeAlive { get { return m_timeAlive; } }
        public bool IsAlive { get { return m_isAlive; } }

        public virtual string GetObjectName()
        {
            return name;
        }

        public virtual void OnSpawned(float spawnTime)
        {
            m_isAlive = true;
            m_spawnTime = spawnTime;

            if (m_settings.sfxOnSpawned != null)
                PlaySFX(m_settings.sfxOnSpawned, true);

            EventManager.EmitOnProjectileSpawned(this);
        }

        public void OnMissionUpdate(float deltaTime)
        {
            if (!m_isAlive)
                return;

            m_timeAlive = MissionManager.GetInstance().MissionTime - m_spawnTime;

            if (m_movement != null)
                m_movement.Move(deltaTime);

            if (m_timeAlive >= m_settings.lifetime)
            {
                Kill(new Damage(1, null, null));
                return;
            }

            if (m_model != null)
                m_model.OnUpdate(deltaTime);
        }

        public void SetShooter(IEntity entity)
        {
            m_shooter = entity;
        }

        public void LinkMovement(ProjectileMovement movement)
        {
            m_movement = movement;
        }

        public void Init(ProjectileSettings settings)
        {
            m_settings = settings;

            if (m_movement != null)
            {
                m_movement.Init(m_settings.speed, m_settings.lifetime);
            }
        }

        public virtual void OnHit(Damage damage)
        {
            DBGLogger.LogWarning(string.Format("Hit by {0} via for damage from {1} by {2}", damage.amount,
                damage.source != null ? damage.source.GetObjectName() : "NULL",
                damage.instigator != null ? damage.instigator.GetObjectName() : "NULL"), this, this);
        }

        public virtual void HitTarget(IEntity target)
        {
            if (m_settings.sfxOnHit != null)
                PlaySFX(m_settings.sfxOnHit);

            target.OnHit(new Damage(m_settings.damage, this, m_shooter));
        }

        public virtual void Kill(Damage damage)
        {
            if (!m_isAlive)
                return;

            m_isAlive = false;
            OnDeath(damage);
        }

        protected virtual void OnDeath(Damage damage)
        {
            DBGLogger.LogWarning(string.Format("Died due to {0} damage from {1} by {2}", damage.amount,
                damage.source != null ? damage.source.GetObjectName() : "NULL",
                damage.instigator != null ? damage.instigator.GetObjectName() : "NULL"), this, this);

            if (m_settings.sfxOnHit != null)
                PlaySFX(m_settings.sfxOnHit);

            Destroy(gameObject, 0.1f);
        }

        protected virtual void PlaySFX(AudioClip audioClip, bool isOneShot = false)
        {
            if (m_audioSource == null)
            {
                DBGLogger.LogError(string.Format("No {0} component found!", typeof(AudioSource)), this, this);
                return;
            }

            if (isOneShot)
                m_audioSource.PlayOneShot(audioClip);
            else
            {
                m_audioSource.Stop();
                m_audioSource.clip = audioClip;
                m_audioSource.Play();
            }
        }
    }
}