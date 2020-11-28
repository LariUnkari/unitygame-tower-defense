using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    [System.Serializable]
    public struct ProjectileSettings
    {
        public LayerMask hitMask;
        public int damage;
        public float lifetime;
        public float speed;
        public AudioClip sfxOnSpawned;
        public AudioClip sfxOnHit;
        public AudioClip sfxOnDeath;

        public ProjectileSettings(LayerMask hitMask, int damage, float lifetime, float speed,
            AudioClip sfxOnSpawned = null, AudioClip sfxOnHit = null, AudioClip sfxOnDeath = null)
        {
            this.hitMask = hitMask;
            this.damage = damage;
            this.lifetime = lifetime;
            this.speed = speed;
            this.sfxOnSpawned = sfxOnSpawned;
            this.sfxOnHit = sfxOnHit;
            this.sfxOnDeath = sfxOnDeath;
        }
    }
}