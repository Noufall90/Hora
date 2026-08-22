using System.Collections;
using UnityEngine;

namespace Enemy
{
    public class EnemyBoss : EnemyMeeleShooter
    {
        protected override IEnumerator ShootSequence()
        {
            Shoot(firePoint);
            yield break;
        }
    }
}