using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtilities;

public class Pawn : MonoBehaviour, IEntity
{
    public GameObject m_model;
    protected PawnMovement m_movement;
    protected AudioSource m_audioSource;
    public AudioClip m_sfxOnSpawned;
    public AudioClip m_sfxOnDamage;
    public AudioClip m_sfxOnDeath;

    public int m_health = 100;

    private void Awake()
    {
        m_audioSource = gameObject.GetComponent<AudioSource>();
    }

    private void Start()
    {
        OnSpawn();
    }

    public virtual string GetObjectName()
    {
        return name;
    }

    public void LinkMovement(PawnMovement movement)
    {
        m_movement = movement;
    }

    public virtual void OnSpawn()
    {
        if (m_sfxOnSpawned != null)
            PlaySFX(m_sfxOnSpawned);
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
            OnDeath();
        }
    }

    protected virtual void OnDeath()
    {
        DBGLogger.LogWarning("Died", this, this);
        if (m_model != null)
            m_model.SetActive(false);
        if (m_movement != null)
            m_movement.enabled = false;
        if (m_sfxOnDeath != null)
            PlaySFX(m_sfxOnDeath);
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
