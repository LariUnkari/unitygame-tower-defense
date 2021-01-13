using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

namespace Entities
{
    public class TowerWeapon : MonoBehaviour, IEntityLinkable
    {
        public TowerWeaponModel m_model;

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

            if (m_model != null)
            {
                m_model.SetMuzzleFlash(m_muzzleFlashSFXClip, m_muzzleFlashVFXPrefab,
                    m_muzzleFlashScale, m_muzzleFlashTime, m_shootAllMuzzles);
            }
        }

        public void StartInit()
        {
            if (m_model != null)
                m_model.StartInit();
        }

        public void StartIdle()
        {
            m_targetPawn = null;

            if (m_model != null)
                m_model.StartIdle();
        }

        public virtual void OnUpdate(float deltaTime)
        {
            if (m_model != null)
            {
                m_model.OnUpdate(deltaTime);

                if (Target != null)
                    m_model.LookAt(TrackingTarget);
            }
        }

        public void StartTracking(Pawn target, float distance = -1f)
        {
            if (m_model != null)
                m_model.StartCharging();

            m_targetPawn = target;
            UpdateTargetingSolution();
            m_attackT = 0f;

            if (m_sfxOnTargetAcquired != null)
                Tower.PlaySFX(m_sfxOnTargetAcquired, true);

            if (m_sfxTracking != null)
                Tower.PlaySFX(m_sfxTracking);

            m_model.StartTracking();
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

        protected Vector3 GetTrackingPosition()
        {
            return m_model != null ? m_model.TrackingPoint : transform.position;
        }

        protected void UpdateTargetingSolution()
        {
            Vector3 pos = GetTrackingPosition();
            if (Math3D.TryGetInterception(pos, Vector3.zero, m_projectileSpeed, m_targetPawn.TrackingPoint, m_targetPawn.Velocity, 0.001f, out m_targetPosition, out m_targetTimeToIntercept))
                m_targetDistance = (m_targetPosition - pos).magnitude;
            else
                m_targetDistance = -1f;
        }

        public void Attack(GameObject projectilePrefab, Vector3 target, ProjectileSettings settings)
        {
            if (m_model != null)
                m_model.Attack(projectilePrefab, target, settings);
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