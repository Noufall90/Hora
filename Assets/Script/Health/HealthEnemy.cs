using UnityEngine;

namespace Enemy
{
    public class EnemyHealth : Health
    {
        protected override void Die()
        {
            // Tambahkan efek mati enemy di sini

            base.Die();
        }
    }
}