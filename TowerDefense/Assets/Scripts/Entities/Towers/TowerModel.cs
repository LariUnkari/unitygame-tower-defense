using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

namespace Entities
{
    public class TowerModel : EntityModel
    {
        public Transform m_baseParent;
        public TowerBaseModel m_baseModel;
        public Transform m_weaponParent;
        public TowerWeaponModel m_weaponModel;

        public Tower Tower { get; set; }
        public override IEntity Entity { get { return Tower; } }

        public override void LinkToEntity(IEntity entity)
        {
            Tower = (Tower)entity;
            DBGLogger.Log(string.Format("Linked to entity {0}<{1}>", entity.GetObjectName(), entity.GetType()), this, this);

            if (m_baseModel != null)
                m_baseModel.LinkToEntity(Tower);
            if (m_weaponModel != null)
                m_weaponModel.LinkToEntity(Tower);
        }


        protected virtual void Awake()
        {
            if (m_baseModel == null)
            {
                TowerBaseModel baseModel = GetComponentInChildren<TowerBaseModel>();
                if (baseModel != null)
                    SetBaseModel(baseModel);
            }
            else
            {
                OnBaseModelSet();
            }

            if (m_weaponModel == null)
            {
                TowerWeaponModel weaponModel = GetComponentInChildren<TowerWeaponModel>();
                if (weaponModel != null)
                    SetWeapoModel(weaponModel);
            }
            else
            {
                OnWeaponModelSet();
            }
        }

        public virtual void SetBaseModel(TowerBaseModel baseModel)
        {
            m_baseModel = baseModel;
            TransformHelper.SetParent(baseModel.transform, m_baseParent);

            OnBaseModelSet();
        }

        protected virtual void OnBaseModelSet()
        {
            
        }

        public virtual void SetWeapoModel(TowerWeaponModel weaponModel)
        {
            m_weaponModel = weaponModel;
            TransformHelper.SetParent(weaponModel.transform, m_weaponParent);

            OnWeaponModelSet();
        }

        protected virtual void OnWeaponModelSet()
        {
            if (m_baseModel != null)
                TransformHelper.SetParent(m_weaponParent, m_baseModel.m_mountWeapon);
        }

        public override void OnUpdate(float deltaTime)
        {
            if (m_baseModel != null)
                m_baseModel.OnUpdate(deltaTime);

            if (m_weaponModel != null)
            {
                m_weaponModel.OnUpdate(deltaTime);

                if (Tower.Target != null)
                    m_weaponModel.LookAt(Tower.TrackingTarget);
            }
        }

        public void StartInit()
        {
            if (m_baseModel != null)
                m_baseModel.StartInit();
            if (m_weaponModel != null)
                m_weaponModel.StartInit();
        }

        public void StartIdle()
        {
            if (m_baseModel != null)
                m_baseModel.StartIdle();
            if (m_weaponModel != null)
                m_weaponModel.StartIdle();
        }

        public void StartTracking()
        {
            if (m_weaponModel != null)
                m_weaponModel.StartCharging();
        }

        public void Attack(GameObject projectilePrefab, Vector3 target, ProjectileSettings settings)
        {
            if (m_weaponModel != null)
                m_weaponModel.Attack(projectilePrefab, target, settings);
        }
    }
}