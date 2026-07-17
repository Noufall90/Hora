using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HFSM
{
    public abstract class EnemyBrain : MonoBehaviour
    {
        public float moveSpeed;
        public float attackDamage;
        public float health;
        public float detectRange;
        public float attackRange;
    }
}
