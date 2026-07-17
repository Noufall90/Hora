using UnityEngine;

namespace Enemy
{
    // Mewarisi EnemyShooter karena mekanik dasarnya sama, tetapi mematikan pergerakan
    public class EnemyTower : EnemyShooter
    {
        protected override void Awake()
        {
            base.Awake();
            // Memastikan speed selalu 0 karena berupa tower statis
            moveSpeed = 0f; 
        }

        protected override void Update()
        {
            base.Update();
            // Tower tidak mengejar player, hanya fokus memutar arah senjata/turret ke player
        }
    }
}