using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    [System.Serializable]
    public struct Damage
    {
        public int amount;
        public IEntity source;
        public IEntity instigator;

        public Damage(int amount, IEntity source, IEntity instigator)
        {
            this.amount = amount;
            this.source = source;
            this.instigator = instigator;
        }
    }
}