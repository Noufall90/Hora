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

    public interface IBomber
    {
        float FireRate { get; }
        float ThrowForce { get; }
        Transform ThrowPosition { get; }
        GameObject GranadePrefab { get; }
        void ThrowGranade();
    }
}