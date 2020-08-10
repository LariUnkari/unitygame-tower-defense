using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntity
{
    string GetObjectName();
    void Hit(int damage);
}
