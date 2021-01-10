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

        protected bool m_shootAllMuzzles = false;

        public Transform[] m_muzzles;
        protected Vector3 m_muzzleLocalCenterPoint;
        protected GameObject[] m_muzzleFlashVFXs;

        protected GameObject m_muzzleVFXPrefab;
        protected float m_muzzleVFXScale = 1;
        protected float m_muzzleVFXTime = 0.2f;

        protected int m_nextMuzzleIndex;

        public AudioSource m_audioSource;
        protected AudioClip m_sfxOnAttack;

        public Vector3 TrackingPoint { get { return transform.TransformPoint(m_muzzleLocalCenterPoint); } }

        public Tower Tower { get; set; }
        public override IEntity Entity { get { return Tower; } }

        public override void LinkToEntity(IEntity entity)
        {
            Tower = (Tower)entity;
            DBGLogger.Log(string.Format("Linked to entity {0}<{1}>", entity.GetObjectName(), entity.GetType()), this, this);
        }

        protected virtual void Awake()
        {
            m_animParamInitID = Animator.StringToHash(m_animParamInitName);
            m_animParamIdleID = Animator.StringToHash(m_animParamIdleName);
            m_animParamChargingID = Animator.StringToHash(m_animParamChargingName);
            m_animParamChargedID = Animator.StringToHash(m_animParamChargedName);
            m_animParamAttackID = Animator.StringToHash(m_animParamAttackName);

            m_nextMuzzleIndex = m_muzzles.Length > 0 ? 0 : -1;

            if (m_muzzles.Length == 0)
                m_muzzleLocalCenterPoint = Vector3.zero;
            else if (m_muzzles.Length == 1)
                m_muzzleLocalCenterPoint = transform.InverseTransformPoint(m_muzzles[0].position);
            else
            {
                for (int i = 0; i < m_muzzles.Length; i++)
                    m_muzzleLocalCenterPoint += m_muzzles[i].position;
                m_muzzleLocalCenterPoint = transform.InverseTransformPoint(m_muzzleLocalCenterPoint / m_muzzles.Length);
            }
        }

        public override void OnUpdate(float deltaTime)
        {

        }

        public virtual void SetMuzzleFlash(AudioClip sfxClip, GameObject vfxPrefab, float vfxScale, float vfxTime, bool shootAllMuzzles)
        {
            m_shootAllMuzzles = shootAllMuzzles;
            m_sfxOnAttack = sfxClip;
            m_muzzleVFXPrefab = vfxPrefab;
            m_muzzleVFXScale = vfxScale;
            m_muzzleVFXTime = vfxTime;

            if (m_muzzleVFXPrefab)
            {
                m_muzzleFlashVFXs = new GameObject[m_muzzles.Length];

                GameObject go;
                for (int i = 0; i < m_muzzles.Length; i++)
                {
                    go = Instantiate(m_muzzleVFXPrefab);
                    TransformHelper.SetParent(go.transform, m_muzzles[i]);
                    go.transform.localScale = Vector3.one * m_muzzleVFXScale;
                    m_muzzleFlashVFXs[i] = go;
                    go.SetActive(false);
                }
            }
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

        public Transform GetNextMuzzle()
        {
            return m_muzzles[m_nextMuzzleIndex];
        }

        public virtual void Attack(GameObject projectilePrefab, Vector3 target, ProjectileSettings settings)
        {
            if (m_animator != null)
                m_animator.SetTrigger(m_animParamAttackID);

            if (m_sfxOnAttack != null)
                PlaySFX(m_sfxOnAttack);

            if (m_shootAllMuzzles)
            {
                for (m_nextMuzzleIndex = 0; m_nextMuzzleIndex < m_muzzleFlashVFXs.Length; m_nextMuzzleIndex++)
                {
                    if (projectilePrefab)
                        FireProjectile(GetNextMuzzle(), target, projectilePrefab, settings);
                    if (m_muzzleVFXPrefab != null)
                        PlayMuzzleVFX(m_nextMuzzleIndex);
                }
            }
            else
            {
                if (projectilePrefab)
                    FireProjectile(GetNextMuzzle(), target, projectilePrefab, settings);
                if (m_muzzleVFXPrefab != null)
                    PlayMuzzleVFX(m_nextMuzzleIndex);

                if (++m_nextMuzzleIndex >= m_muzzleFlashVFXs.Length)
                    m_nextMuzzleIndex = 0;
            }
        }

        protected virtual void FireProjectile(Transform muzzle, Vector3 target, GameObject projectilePrefab, ProjectileSettings settings)
        {
            GameObject go = Instantiate(projectilePrefab);
            Projectile projectile = go.GetComponent<Projectile>();
            if (projectile == null)
            {
                DBGLogger.LogError(string.Format("Unable to find component of type {0} in prefab '{1}'",
                    typeof(Projectile), projectilePrefab.name), this, this, DBGLogger.Mode.Everything);

                Destroy(go);
                return;
            }

            TransformHelper.Align(projectile.transform, muzzle);
            projectile.transform.LookAt(target);

            projectile.OnSpawned(MissionManager.GetInstance().MissionTime);
            projectile.Init(settings);
        }

        protected virtual void PlayMuzzleVFX(int muzzleIndex)
        {
            StartCoroutine(MuzzleFlashRoutine(muzzleIndex, m_muzzleVFXTime));
        }

        protected IEnumerator MuzzleFlashRoutine(int index, float time)
        {
            if (m_muzzleFlashVFXs[index] != null) m_muzzleFlashVFXs[index].SetActive(true);
            yield return new WaitForSeconds(time);
            if (m_muzzleFlashVFXs[index] != null) m_muzzleFlashVFXs[index].SetActive(false);
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