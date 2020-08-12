using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntity
{
    string GetObjectName();
    void OnSpawn();
    void Hit(int damage);
}
