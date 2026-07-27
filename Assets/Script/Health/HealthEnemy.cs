using UnityEngine;

namespace Enemy
{
    public class EnemyHealth : Health, IHealthData
    {
        protected override void Die()
        {
            base.Die();
        }
    }
}