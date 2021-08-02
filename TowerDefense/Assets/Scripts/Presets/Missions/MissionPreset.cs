using System.Collections.Generic;
using UnityEngine;

namespace Presets
{
    [CreateAssetMenu(fileName = "New MissionPreset.asset", menuName = "ScriptableObject/Presets/MissionPreset")]
    public class MissionPreset : ScriptableObject
    {
        public MissionSettings settings;
        public List<MissionWave> waves;
    }
}