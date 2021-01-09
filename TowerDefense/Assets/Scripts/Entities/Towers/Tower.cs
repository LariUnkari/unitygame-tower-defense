﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

namespace Entities
{
    public class Tower : MonoBehaviour, IEntity
    {
        public enum Mode { Init, Idle, Tracking }

        protected Mode m_mode;

        public TowerModel m_model;

        public float m_initDuration = 1;
        protected float m_spawnTime;

        public LayerMask m_hitMask;

        public float m_range = 6f;
        protected Pawn m_target;
        protected float m_distance;

        public int m_damage = 50;
        public float m_interval = 1f;
        protected float m_attackT;

        public GameObject m_projectilePrefab;
        public float m_projectileSpeed = 10f;
        public float m_projectileLifetime = 1f;

        public GameObject m_muzzleFlashVFXPrefab;
        public AudioClip m_muzzleFlashSFXClip;
        public float m_muzzleFlashScale = 1;
        public float m_muzzleFlashTime = 0.1f;
        public bool m_shootAllMuzzles = false;

        public AudioSource m_audioSource;
        public AudioClip m_sfxOnSpawned;
        public AudioClip m_sfxInit;
        public AudioClip m_sfxOnInit;
        public AudioClip m_sfxIdle;
        public AudioClip m_sfxOnTargetAcquired;
        public AudioClip m_sfxTracking;

        public Pawn Target { get { return m_target; } }
        public Vector3 TrackingTarget { get { return m_target != null ? m_target.TrackingPoint : Vector3.zero; } }

        private void OnEnable()
        {
            EventManager.OnMissionStarted += OnMissionStarted;
        }

        private void OnDisable()
        {
            EventManager.OnMissionStarted -= OnMissionStarted;
        }

        protected virtual void Awake()
        {
            m_model.LinkToEntity(this);
        }

        protected virtual void Start()
        {
            if (m_model != null && m_model.m_weaponModel != null)
            {
                m_model.m_weaponModel.SetMuzzleFlash(m_muzzleFlashSFXClip, m_muzzleFlashVFXPrefab,
                    m_muzzleFlashScale, m_muzzleFlashTime, m_shootAllMuzzles);
            }
        }

        public virtual string GetObjectName()
        {
            return name;
        }

        public virtual void OnSpawned(float spawnTime)
        {
            m_spawnTime = spawnTime;

            if (m_sfxOnSpawned != null)
                PlaySFX(m_sfxOnSpawned, true);

            StartInit();
        }

        public void OnMissionUpdate(float deltaTime)
        {
            switch (m_mode)
            {
                case Mode.Init: OnInitUpdate();
                    break;
                case Mode.Tracking: OnTrackingUpdate(deltaTime);
                    break;
                default: OnIdleUpdate();
                    break;
            }

            if (m_model != null)
                m_model.OnUpdate(deltaTime);
        }

        public virtual void StartInit()
        {
            m_mode = Mode.Init;

            if (m_sfxInit != null)
                PlaySFX(m_sfxInit);

            m_model.StartInit();
        }

        protected virtual void OnInitUpdate()
        {
            if (MissionManager.GetInstance().MissionTime > m_spawnTime + m_initDuration)
            {
                if (m_sfxOnInit != null)
                    PlaySFX(m_sfxOnInit, true);

                StartIdle();
            }
        }

        public virtual void StartIdle()
        {
            m_mode = Mode.Idle;
            m_target = null;

            if (m_sfxIdle != null)
                PlaySFX(m_sfxIdle);

            m_model.StartIdle();
        }

        protected virtual void OnIdleUpdate()
        {
            SpatialSearch.Result<Pawn> result = SpatialSearch.FindClosest(transform.position, m_range,
                MissionManager.GetInstance().GetAllEnemies(), (Pawn pawn) => { return pawn.transform; });

            if (result.item != null)
                StartTracking(result.item, result.distance);
        }

        public virtual void StartTracking(Pawn target, float distance = -1f)
        {
            m_mode = Mode.Tracking;
            m_target = target;
            m_distance = distance >= 0 ? distance : (m_target.transform.position - transform.position).magnitude;
            m_attackT = 0f;

            if (m_sfxOnTargetAcquired != null)
                PlaySFX(m_sfxOnTargetAcquired, true);

            if (m_sfxTracking != null)
                PlaySFX(m_sfxTracking);

            m_model.StartTracking();
        }

        protected virtual void OnTrackingUpdate(float deltaTime)
        {
            if (m_target == null || !m_target.IsAlive)
            {
                StartIdle();
                return;
            }

            m_distance = (m_target.transform.position - transform.position).magnitude;
            if (m_distance > m_range)
            {
                StartIdle();
                return;
            }

            m_attackT += deltaTime / m_interval;
            if (m_attackT >= 1f)
            {
                m_attackT -= 1f;
                m_model.Attack(m_projectilePrefab, m_target.TrackingPoint, new ProjectileSettings(m_hitMask, m_damage, m_projectileLifetime, m_projectileSpeed));
            }
        }

        public virtual void OnHit(Damage damage)
        {
            DBGLogger.LogWarning(string.Format("Hit by {0} via for damage from {1} by {2}",
                damage.amount, damage.source.GetObjectName(), damage.instigator.GetObjectName()), this, this);
        }

        public virtual void Kill(Damage damage)
        {
            OnDeath(damage);
        }

        protected virtual void OnDeath(Damage damage)
        {
            DBGLogger.LogWarning(string.Format("Died due to {0} damage from {1} via {2}",
                damage.amount, damage.source, damage.instigator), this, this);
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

        protected virtual void OnMissionStarted()
        {
            EventManager.EmitOnTowerSpawned(this);
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, m_range);
            if (m_target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine((m_model != null ? m_model.transform : transform).position, m_target.TrackingPoint);
            }
        }
    }
}