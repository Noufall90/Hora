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

        public float FireRate => fireRate;
        public float ThrowForce => throwForce;
        public Transform ThrowPosition => throwPosition;
        public GameObject GranadePrefab => granadePrefab;

        public void ThrowGranade()
        {
            if (granadePrefab == null || throwPosition == null || PlayerTarget == null) return;
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