using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public interface IEntity
    {
        string GetObjectName();
        void OnSpawned(float spawnTime);
        void OnMissionUpdate(float deltaTime);
        void OnHit(Damage damage);
        void Kill(Damage damage);
    }
}