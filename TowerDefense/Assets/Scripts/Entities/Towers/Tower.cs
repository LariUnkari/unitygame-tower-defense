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

        public float m_initDuration = 1;
        protected float m_spawnTime;

        public Transform m_weaponMount;
        protected TowerWeapon m_weapon;
        public Transform m_baseMount;
        protected TowerBaseModel m_baseModel;

        public AudioSource m_audioSource;
        public AudioClip m_sfxOnSpawned;
        public AudioClip m_sfxInit;
        public AudioClip m_sfxOnInit;
        public AudioClip m_sfxIdle;
        public AudioClip m_sfxOnTargetAcquired;
        public AudioClip m_sfxTracking;

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
            if (m_baseModel == null)
                SetBaseModel(m_baseMount.GetComponentInChildren<TowerBaseModel>());
            else
                OnBaseModelSet();

            if (m_weapon == null)
                SetWeapon(m_weaponMount.GetComponent<TowerWeapon>());
            else
                OnWeaponSet();
        }

        protected virtual void Start()
        {

        }

        public virtual string GetObjectName()
        {
            return name;
        }

        public virtual void SetBaseModel(TowerBaseModel baseModel)
        {
            m_baseModel = baseModel;
            if (m_baseModel != null) OnBaseModelSet();
        }

        protected virtual void OnBaseModelSet()
        {
            TransformHelper.SetParent(m_baseModel.transform, transform);
            m_baseModel.LinkToEntity(this);
            if (m_weapon) MountWeapon();
        }

        public virtual void SetWeapon(TowerWeapon weapon)
        {
            m_weapon = weapon;
            if (m_weapon != null) OnWeaponSet();
        }

        protected virtual void OnWeaponSet()
        {
            m_weapon.LinkToEntity(this);
            if (m_baseModel) MountWeapon();
        }

        protected virtual void MountWeapon()
        {
            TransformHelper.SetParent(m_weapon.transform, m_baseModel.m_mountWeapon != null ? m_baseModel.m_mountWeapon : transform);
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

            if (m_baseModel != null)
                m_baseModel.OnUpdate(deltaTime);
            if (m_weapon != null)
                m_weapon.OnUpdate(deltaTime);
        }

        public virtual void StartInit()
        {
            m_mode = Mode.Init;

            if (m_sfxInit != null)
                PlaySFX(m_sfxInit);

            m_baseModel.StartInit();
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

            if (m_sfxIdle != null)
                PlaySFX(m_sfxIdle);

            m_baseModel.StartIdle();
            m_weapon.StartIdle();
        }

        protected virtual void OnIdleUpdate()
        {
            if (m_weapon != null)
            {
                SpatialSearch.Result<Pawn> result = SpatialSearch.FindClosest(transform.position, m_weapon.m_attributes.range,
                    MissionManager.GetInstance().GetAllEnemies(), (Pawn pawn) => { return pawn.transform; });

                if (result.item != null)
                    StartTracking(result.item, result.distance);
            }
        }

        public virtual void StartTracking(Pawn target, float distance = -1f)
        {
            m_mode = Mode.Tracking;

            m_weapon.StartTracking(target, distance);
        }

        protected virtual void OnTrackingUpdate(float deltaTime)
        {
            m_weapon.OnTrackingUpdate(deltaTime);
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

        public virtual void PlaySFX(AudioClip audioClip, bool isOneShot = false)
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
    }
}