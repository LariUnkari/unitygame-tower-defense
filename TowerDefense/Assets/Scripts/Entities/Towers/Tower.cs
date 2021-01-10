using System.Collections;
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
        protected Pawn m_targetPawn;
        protected Vector3 m_targetPosition;
        protected float m_targetDistance;
        protected float m_targetTimeToIntercept;

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

        public Pawn Target { get { return m_targetPawn; } }
        public Vector3 TrackingTarget { get { return m_targetPawn != null ? m_targetPawn.TrackingPoint : Vector3.zero; } }

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
            if (!enabled || !gameObject.activeInHierarchy)
                return;

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
            m_targetPawn = null;

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

        protected Vector3 GetTrackingPosition()
        {
            return m_model != null ? (m_model.m_weaponModel != null ? m_model.m_weaponModel.TrackingPoint : m_model.transform.position) : transform.position;
        }

        protected void UpdateTargetingSolution()
        {
            Vector3 pos = GetTrackingPosition();
            if (Math3D.TryGetInterception(pos, Vector3.zero, m_projectileSpeed, m_targetPawn.TrackingPoint, m_targetPawn.Velocity, 0.001f, out m_targetPosition, out m_targetTimeToIntercept))
                m_targetDistance = (m_targetPosition - pos).magnitude;
            else
                m_targetDistance = -1f;
        }

        public virtual void StartTracking(Pawn target, float distance = -1f)
        {
            m_mode = Mode.Tracking;
            m_targetPawn = target;
            UpdateTargetingSolution();
            m_attackT = 0f;

            if (m_sfxOnTargetAcquired != null)
                PlaySFX(m_sfxOnTargetAcquired, true);

            if (m_sfxTracking != null)
                PlaySFX(m_sfxTracking);

            m_model.StartTracking();
        }

        protected virtual void OnTrackingUpdate(float deltaTime)
        {
            if (m_targetPawn == null || !m_targetPawn.IsAlive)
            {
                StartIdle();
                return;
            }

            UpdateTargetingSolution();

            if (m_targetDistance > m_range)
            {
                StartIdle();
                return;
            }

            m_attackT += deltaTime / m_interval;
            if (m_attackT >= 1f)
            {
                m_attackT -= 1f;
                m_model.Attack(m_projectilePrefab, m_targetPosition, new ProjectileSettings(m_hitMask, m_damage, m_projectileLifetime, m_projectileSpeed));
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
            if (m_targetPawn != null)
            {
                Vector3 pos = GetTrackingPosition();

                Gizmos.color = Color.red;
                Gizmos.DrawLine(pos, m_targetPawn.TrackingPoint);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(pos, m_targetPosition);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(m_targetPawn.TrackingPoint, m_targetPosition);
            }
        }
    }
}