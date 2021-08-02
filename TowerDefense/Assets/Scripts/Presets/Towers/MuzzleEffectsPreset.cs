using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Presets
{
    [CreateAssetMenu(fileName = "New MuzzleEffectsPreset.asset", menuName = "ScriptableObject/Presets/MuzzleEffectsPreset")]
    public class MuzzleEffectsPreset : ScriptableObject
    {
        public GameObject flashVFXPrefab;
        public AudioClip flashSFXClip;
        public float flashScale = 1f;
        public float flashTime = 0.1f;
    }
}