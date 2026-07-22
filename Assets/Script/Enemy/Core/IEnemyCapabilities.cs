using UnityEngine;

namespace Enemy
{
    public interface IMeele
    {
        void MeeleAttack();
    }

    public interface IShooter
    {
        float FireRate { get; }
        Transform FirePoint { get; }
        Transform FirePoint2 { get; }
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

    public interface ITower
    {
        float FireRate { get; }
        Transform FirePoint { get; }
        Transform FirePoint2 { get; }
        GameObject BulletPrefab { get; }
        void ShootAttack();
    }
}