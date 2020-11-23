using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public interface IEntity
    {
        string GetObjectName();
        void OnSpawned(Map map, int pathIndex, float spawnTime);
        void Hit(int damage);
        void Kill();
    }
}