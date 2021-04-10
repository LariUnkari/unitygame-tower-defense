using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

namespace Entities
{
    public class TowerWeapon : MonoBehaviour, IEntityLinkable
    {
        public TowerWeaponModel m_weaponModel;

        public LayerMask m_hitMask;

        public TowerWeaponAttributes m_attributes = new TowerWeaponAttributes(5f, 10, 0.2f, 10f, 1f, false);

        protected Pawn m_targetPawn;
        protected Vector3 m_targetPosition;
        protected float m_targetDistance;
        protected float m_targetTimeToIntercept;

        protected float m_attackT;

        public GameObject m_projectilePrefab;

        public MuzzleEffectsPreset m_muzzleEffects;

        public AudioClip m_sfxOnTargetAcquired;
        public AudioClip m_sfxTracking;

        public Tower Tower { get; set; }
        public virtual IEntity Entity { get { return Tower; } }

        public Pawn Target { get { return m_targetPawn; } }
        public Vector3 TrackingTarget { get { return m_targetPawn != null ? m_targetPawn.TrackingPoint : Vector3.zero; } }

        public virtual void LinkToEntity(IEntity entity)
        {
            Tower = (Tower)entity;
            DBGLogger.Log(string.Format("Linked to entity {0}<{1}>", entity.GetObjectName(), entity.GetType()), this, this);

            if (m_weaponModel != null)
                m_weaponModel.LinkToWeapon(this);
        }

        public virtual void SetFromPreset(TowerWeaponPreset weaponPreset)
        {
            m_hitMask = weaponPreset.hitMask;
            m_attributes = weaponPreset.attributes;
            m_projectilePrefab = weaponPreset.projectilePrefab;
            m_muzzleEffects = weaponPreset.muzzleEffects;
            m_sfxOnTargetAcquired = weaponPreset.sfxOnTargetAcquired;
            m_sfxTracking = weaponPreset.sfxTracking;
        }

        public virtual void SetWeaponModel(TowerWeaponModel weaponModel)
        {
            m_weaponModel = weaponModel;
            if (m_weaponModel != null) OnWeaponModelSet();
        }

        protected virtual void OnWeaponModelSet()
        {
            TransformHelper.SetParent(m_weaponModel.transform, transform);
            if (Tower) m_weaponModel.LinkToEntity(Tower);
        }

        public void StartInit()
        {
            if (m_weaponModel != null)
                m_weaponModel.StartInit();
        }

        public void StartIdle()
        {
            m_targetPawn = null;

            if (m_weaponModel != null)
                m_weaponModel.StartIdle();
        }

        public virtual void OnUpdate(float deltaTime)
        {
            if (m_weaponModel != null)
            {
                m_weaponModel.OnUpdate(deltaTime);

                if (Target != null)
                    m_weaponModel.LookAt(TrackingTarget);
            }
        }

        public void StartTracking(Pawn target, float distance = -1f)
        {
            if (m_weaponModel != null)
                m_weaponModel.StartCharging();

            m_targetPawn = target;
            UpdateTargetingSolution();
            m_attackT = 0f;

            if (m_sfxOnTargetAcquired != null)
                Tower.PlaySFX(m_sfxOnTargetAcquired, true);

            if (m_sfxTracking != null)
                Tower.PlaySFX(m_sfxTracking);

            m_weaponModel.StartTracking();
        }

        public virtual void OnTrackingUpdate(float deltaTime)
        {
            if (m_targetPawn == null || !m_targetPawn.IsAlive)
            {
                m_targetPawn = null;
                Tower.StartIdle();
                return;
            }

            UpdateTargetingSolution();

            if (m_targetDistance > m_attributes.range)
            {
                StartIdle();
                return;
            }

            m_attackT += deltaTime / m_attributes.interval;
            if (m_attackT >= 1f)
            {
                m_attackT -= 1f;
                m_weaponModel.Attack(m_projectilePrefab, m_targetPosition, new ProjectileSettings(m_hitMask, m_attributes.damage, m_attributes.projectileLifetime, m_attributes.projectileSpeed));
            }
        }

        protected Vector3 GetTrackingPosition()
        {
            return m_weaponModel != null ? m_weaponModel.TrackingPoint : transform.position;
        }

        protected void UpdateTargetingSolution()
        {
            Vector3 pos = GetTrackingPosition();
            if (Math3D.TryGetInterception(pos, Vector3.zero, m_attributes.projectileSpeed, m_targetPawn.TrackingPoint, m_targetPawn.Velocity, 0.001f, out m_targetPosition, out m_targetTimeToIntercept))
                m_targetDistance = (m_targetPosition - pos).magnitude;
            else
                m_targetDistance = -1f;
        }

        public void Attack(GameObject projectilePrefab, Vector3 target, ProjectileSettings settings)
        {
            if (m_weaponModel != null)
                m_weaponModel.Attack(projectilePrefab, target, settings);
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, m_attributes.range);
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