using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HFSM.Enemy
{
    public abstract class EnemyBrain : MonoBehaviour
    {
        [SerializeField] protected float moveSpeed;
        [SerializeField] protected float attackDamage;
        [SerializeField] protected float detectRange;
        [SerializeField] protected float attackRange;
        protected Transform playerTarget;
    }
}
