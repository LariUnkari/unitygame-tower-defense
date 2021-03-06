﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public interface IEntityLinkable
    {
        IEntity Entity { get; }
        void LinkToEntity(IEntity entity);
    }
}