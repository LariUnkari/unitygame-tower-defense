using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

namespace Entities
{
    public class Pawn : MonoBehaviour, IEntity
    {
        public PawnModel m_model;
        protected PawnMovement m_movement;

        protected AudioSource m_audioSource;
        public AudioClip m_sfxOnSpawned;
        public AudioClip m_sfxOnDamage;
        public AudioClip m_sfxOnDeath;

        protected bool m_isAlive;
        protected float m_spawnTime;
        protected float m_timeAlive;
        public int m_health = 100;
        public int m_playerDamage = 100;

        public Transform m_trackingPoint;

        public float SpawnTime { get { return m_spawnTime; } }
        public float TimeAlive { get { return m_timeAlive; } }
        public bool IsAlive { get { return m_isAlive; } }

        private void Awake()
        {
            m_audioSource = gameObject.GetComponent<AudioSource>();
        }

        public virtual string GetObjectName()
        {
            return name;
        }

        public void LinkMovement(PawnMovement movement)
        {
            m_movement = movement;
        }

        public virtual void OnSpawned(Map map, int pathIndex, float spawnTime)
        {
            m_isAlive = true;
            m_spawnTime = spawnTime;

            if (m_movement != null)
            {
                m_movement.SetMap(map);
                m_movement.SetPath(map.m_paths[pathIndex]);

                Location location = map.GetSpawnLocation(pathIndex);
                transform.position = location.position;
                transform.rotation = location.rotation;
            }

            if (m_sfxOnSpawned != null)
                PlaySFX(m_sfxOnSpawned);
        }

        public void OnMissionUpdate(float deltaTime)
        {
            m_timeAlive = MissionManager.GetInstance().MissionTime - m_spawnTime;

            if (m_movement != null)
            {
                if (m_movement.ShouldMove())
                    m_movement.MoveOnPath();
            }
        }

        public virtual void Hit(int damage)
        {
            ApplyDamage(damage);
        }

        protected virtual void ApplyDamage(int damage)
        {
            m_health -= damage;
            DBGLogger.Log(string.Format("Applied {0} damage, current health is {1}",
                damage, m_health), this, this);

            if (m_health > 0)
            {
                if (m_sfxOnDamage != null)
                    PlaySFX(m_sfxOnDamage);
            }
            else
            {
                Kill();
            }
        }

        public virtual void Kill()
        {
            if (!m_isAlive)
                return;

            m_isAlive = false;
            OnDeath();
        }

        protected virtual void OnDeath()
        {
            DBGLogger.LogWarning("Died", this, this);

            if (m_model != null)
                m_model.gameObject.SetActive(false);
            if (m_movement != null)
                m_movement.enabled = false;
            if (m_model != null && m_movement != null)
                m_model.SetMovementSpeedModifier(m_movement.m_moveSpeed);

            if (m_sfxOnDeath != null)
                PlaySFX(m_sfxOnDeath);

            MissionManager.GetInstance().OnEnemyDied(this);
        }

        protected virtual void PlaySFX(AudioClip audioClip)
        {
            if (m_audioSource == null)
            {
                DBGLogger.LogError(string.Format("No {0} component found!", typeof(AudioSource)), this, this);
                return;
            }

            m_audioSource.PlayOneShot(audioClip);
            /*if (m_audioSource.isPlaying)
                m_audioSource.Stop();

            m_audioSource.clip = audioClip;
            m_audioSource.Play();*/
        }
    }
}