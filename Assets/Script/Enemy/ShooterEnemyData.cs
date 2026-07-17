using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Enemy Data")]
public class ShooterEnemyData : ScriptableObject
{
    [Header("Stats")]
    public float maxHealth;
    public float attackDamage;
    public float moveSpeed;
    public float detectRange;
    public float attackRange;

    [Header("Shooter")]
    public float projectileSpeed;
    public float fireRate;
    public GameObject projectilePrefab;
}
