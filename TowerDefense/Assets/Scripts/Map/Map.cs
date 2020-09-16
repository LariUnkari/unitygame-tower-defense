using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class Map : MonoBehaviour
{
    public Spline[] m_paths;

    private void Start()
    {
        GameManager.GetInstance().OnMapLoaded(this);
    }

    public Location GetSpawnLocation(int pathIndex)
    {
        if (m_paths.Length == 0) { return Location.Default; }

        pathIndex %= m_paths.Length;
        return new Location(m_paths[pathIndex].GetPosition(0f, true), Quaternion.FromToRotation(Vector3.forward, m_paths[pathIndex].GetDirection(0f, true)));
    }
}
