using UnityEngine;

namespace Enemy
{
    public class EnemyTower : EnemyShooter, ITower
    {
        [Header("IK Aiming Settings")]
        [SerializeField] private Transform headIKTarget; 

        private Vector3 initialForward;

        protected override void Awake()
        {
            base.Awake();
            moveSpeed = 0f;
            initialForward = transform.forward;
        }

        protected override bool IsPlayerInViewCone(float maxDistance)
        {
            if (playerTarget == null) return false;

            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;

            float dot = Vector3.Dot(initialForward, directionToPlayer);
            if (dot <= 0f) return false;

            float angle = Vector3.Angle(initialForward, directionToPlayer);
            return angle <= fieldOfView * 0.5f;
        }

        public override void RotateTowardsPlayer()
        {
            if (playerTarget != null && headIKTarget != null && IsPlayerDetected())
            {
                headIKTarget.position = Vector3.Lerp(headIKTarget.position, playerTarget.position, rotationSpeed * Time.deltaTime);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!IsPlayerDetected() && headIKTarget != null)
            {
                Vector3 defaultLookPos = transform.position + initialForward * 5f + transform.up * 1f;
                headIKTarget.position = Vector3.Lerp(headIKTarget.position, defaultLookPos, rotationSpeed * Time.deltaTime);
            }
        }
    }
}