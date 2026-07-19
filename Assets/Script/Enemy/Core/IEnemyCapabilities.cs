using UnityEngine;

namespace Enemy
{
    public interface IMeele
    {
        float MeeleAttackRange { get; }
        void MeeleAttack();
    }

    public interface IShooter
    {
        float FireRate { get; }
        Transform FirePoint { get; }
        GameObject BulletPrefab { get; }
        void ShootAttack();
    }
}