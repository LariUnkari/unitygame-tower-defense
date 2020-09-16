using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Location
{
    public static Location Default = new Location(Vector3.zero, Quaternion.identity);

    public Vector3 position;
    public Quaternion rotation;

    public Location(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
    }

    public Vector3 GetRelativeDirection(Vector3 relativeSelf)
    {
        return rotation * relativeSelf;
    }
}
