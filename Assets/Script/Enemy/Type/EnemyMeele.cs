using UnityEngine;

namespace Enemy
{
    public class EnemyMeele : EnemyBrain, IMeele
    {
        [Header("Meele Settings")]
        [SerializeField] private Animator animator;
        [SerializeField] private int damage = 10;
        [SerializeField] private float damageInterval = 2.0f;
        [SerializeField] private BoxCollider damageCollider;

        private Health playerHealthInBox;
        private float damageTimer;
        private procedural_animation.EnemyProceduralAnimator proceduralAnimator;

        protected override void Start()
        {
            base.Start();
            proceduralAnimator = GetComponentInChildren<procedural_animation.EnemyProceduralAnimator>() ?? GetComponent<procedural_animation.EnemyProceduralAnimator>();

            if (damageCollider != null)
            {
                damageCollider.isTrigger = true;
            }
        }

        public void MeeleAttack()
        {
            if (animator != null)
            {
                animator.SetTrigger("Attack");
                animator.SetBool("Attack", true);
            }

            if (proceduralAnimator != null)
            {
                proceduralAnimator.SetAttacking(true, PlayerTarget);
            }
        }

        public void StopAttack()
        {
            if (animator != null)
            {
                animator.SetBool("Attack", false);
            }

            if (proceduralAnimator != null)
            {
                proceduralAnimator.SetAttacking(false);
            }
        }

        protected override void Update()
        {
            base.Update();

            // Deal 10 damage every 2 seconds if player is inside damageCollider
            if (playerHealthInBox != null)
            {
                damageTimer += Time.deltaTime;
                if (damageTimer >= damageInterval)
                {
                    playerHealthInBox.TakeDamage(damage);
                    damageTimer = 0f;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.GetComponent<Health>() != null)
            {
                Health health = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
                if (health != null)
                {
                    playerHealthInBox = health;
                    damageTimer = 0f;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || (playerHealthInBox != null && (other.GetComponent<Health>() == playerHealthInBox || other.GetComponentInParent<Health>() == playerHealthInBox)))
            {
                playerHealthInBox = null;
                damageTimer = 0f;
            }
        }
    }
}