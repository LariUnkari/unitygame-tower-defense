﻿using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New MissionPreset.asset", menuName = "ScriptableObject/MissionPreset")]
public class MissionPreset : ScriptableObject
{
    public MissionSettings settings;
    public List<MissionWave> waves;
}
