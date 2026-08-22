using UnityEngine;
using System.Collections;

namespace Enemy
{
    public class EnemyBomber : EnemyBrain, IBomber
    {
        [Header("Bomber Settings")]
        [SerializeField] private float fireRate = 3f;
        
        [Header("Granade")]
        [SerializeField] private GameObject granadePrefab;
        [SerializeField] private Transform throwPosition;
        [SerializeField] private float throwForce = 10f; // Kecepatan lemparan horizontal
        [SerializeField] private float maxForce;

        [Header("Trajectory Line")]
        [SerializeField] private LineRenderer trajectoryLine;

        [Header("Knockback Settings")]
        [SerializeField] private int requiredComboHits = 3;
        [SerializeField] private float comboResetTime = 1.5f;
        [SerializeField] private float knockbackForce = 10f;
        [SerializeField] private float knockbackDuration = 0.3f;

        private int currentComboHits = 0;
        private float comboResetTimer = 0f;
        private bool isKnockedBack = false;

        public float FireRate => fireRate;
        public float ThrowForce => throwForce;
        public Transform ThrowPosition => throwPosition;
        public GameObject GranadePrefab => granadePrefab;
        public bool IsKnockedBack => isKnockedBack;

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void Update()
        {
            base.Update();

            if (currentComboHits > 0)
            {
                comboResetTimer -= Time.deltaTime;
                if (comboResetTimer <= 0f)
                {
                    currentComboHits = 0;
                }
            }
        }

        protected override void OnDamageTakenHandler(int damageAmount)
        {
            base.OnDamageTakenHandler(damageAmount);

            currentComboHits++;
            comboResetTimer = comboResetTime;

            if (currentComboHits >= requiredComboHits)
            {
                currentComboHits = 0;
                ApplyKnockback();
            }
        }

        private void ApplyKnockback()
        {
            if (isKnockedBack) return;

            Vector3 knockbackDir = Vector3.zero;
            if (playerTarget != null)
            {
                knockbackDir = (transform.position - playerTarget.position);
                knockbackDir.y = 0f;
            }

            if (knockbackDir.sqrMagnitude < 0.001f)
            {
                knockbackDir = -transform.forward;
                knockbackDir.y = 0f;
            }

            knockbackDir.Normalize();
            StartCoroutine(KnockbackRoutine(knockbackDir));
        }

        private IEnumerator KnockbackRoutine(Vector3 direction)
        {
            isKnockedBack = true;
            float elapsed = 0f;

            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = false;
            }

            Animator anim = GetComponentInChildren<Animator>() ?? GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Hit");
            }

            while (elapsed < knockbackDuration)
            {
                elapsed += Time.deltaTime;
                float currentForce = Mathf.Lerp(knockbackForce, 0f, elapsed / knockbackDuration);
                Vector3 moveStep = direction * currentForce * Time.deltaTime;

                if (HasActiveNavMeshAgent)
                {
                    agent.Move(moveStep);
                }
                else
                {
                    transform.position += moveStep;
                }

                yield return null;
            }

            isKnockedBack = false;
        }

        public void ThrowGranade()
        {
            if (isKnockedBack || granadePrefab == null || throwPosition == null || PlayerTarget == null) return;
            StartCoroutine(AimAndThrowRoutine());
        }

        private IEnumerator AimAndThrowRoutine()
        {
            Vector3 originPos = throwPosition.position;
            Vector3 targetPos = PlayerTarget.position;
            
            // Kalkulasi waktu tempuh berdasarkan jarak dan throwForce
            Vector3 distanceXZ = new Vector3(targetPos.x - originPos.x, 0f, targetPos.z - originPos.z);
            float distance = distanceXZ.magnitude;
            float timeToTarget = Mathf.Clamp(distance / throwForce, 0.5f, 3f); 
            
            // Hitung velocity awal
            Vector3 velocity = CalculateVelocityToTarget(originPos, targetPos, timeToTarget);

            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = true;
                DrawTrajectory(originPos, velocity);
            }

            // Waktu delay membidik
            yield return new WaitForSeconds(1f);

            if (trajectoryLine != null)
                trajectoryLine.enabled = false;

            if (isKnockedBack) yield break;

            // Instantiate dan lempar setelah membidik
            GameObject granade = Instantiate(granadePrefab, throwPosition.position, throwPosition.rotation);
            Rigidbody rb = granade.GetComponent<Rigidbody>();
            
            if (rb != null)
            {
                rb.velocity = velocity;
            }
        }
        
        private void DrawTrajectory(Vector3 startPos, Vector3 velocity)
        {
            int lineSegments = 30;
            float timeStep = 0.1f;
            trajectoryLine.positionCount = lineSegments;

            Vector3 currentPos = startPos;
            Vector3 currentVelocity = velocity;

            for (int i = 0; i < lineSegments; i++)
            {
                trajectoryLine.SetPosition(i, currentPos);
                currentVelocity += Physics.gravity * timeStep;
                currentPos += currentVelocity * timeStep;
            }
        }

        private Vector3 CalculateVelocityToTarget(Vector3 origin, Vector3 target, float time)
        {
            Vector3 distance = target - origin;
            Vector3 distanceXZ = distance;
            distanceXZ.y = 0f;

            float sY = distance.y;
            float sXZ = distanceXZ.magnitude;

            // Vxz = s / t
            float Vxz = sXZ / time;
            
            // Vy = (sY / t) + (0.5 * g * t)
            float Vy = (sY / time) + (0.5f * Mathf.Abs(Physics.gravity.y) * time);

            Vector3 result = distanceXZ.normalized;
            result *= Vxz;
            result.y = Vy;

            return result;
        }
    }
}