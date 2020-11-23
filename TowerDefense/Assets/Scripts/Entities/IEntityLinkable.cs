using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public interface IEntityLinkable
    {
        void LinkToEntity(IEntity entity);
    }
}