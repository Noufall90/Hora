using UnityEngine;

namespace Enemy
{
    public class EnemyMeele : EnemyBrain, IMeele
    {
        [Header("Meele Settings")]
        [SerializeField] private float meeleAttackRange = 1.5f;
        
        public float MeeleAttackRange => meeleAttackRange;

        public void MeeleAttack()
        {
            // Logika ayunan pedang / pukulan fisik
            Debug.Log($"{gameObject.name} melakukan serangan Meele!");
        }
    }
}