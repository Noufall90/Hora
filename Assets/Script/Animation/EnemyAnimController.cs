using UnityEngine;
using UnityEngine.AI;

namespace EnemyAnimation
{
    [RequireComponent(typeof(EnemyAnim))] 
    public class EnemyAnimController : MonoBehaviour
    {
        private EnemyAnim _anim;
        private NavMeshAgent _agent;

        private const float MaxNavMeshDistance = 1.0f;

        private void Awake()
        {
            _anim = GetComponent<EnemyAnim>();
            _agent = GetComponent<NavMeshAgent>() ?? GetComponentInParent<NavMeshAgent>() ?? GetComponentInChildren<NavMeshAgent>();
        }

        private void Start()
        {
            SnapToNavMesh();
        }

        public bool IsMoving => _anim != null && _anim.IsMoving;
        public bool IsAttacking => _anim != null && _anim.IsAttacking;
        public bool PlayerDetected => _anim != null && _anim.PlayerDetected;
        public bool IsOnNavMesh => _agent != null && _agent.enabled ? _agent.isOnNavMesh : NavMesh.SamplePosition(transform.position, out _, 0.5f, NavMesh.AllAreas);

        public void TriggerAttack() => _anim?.TriggerAttack();
        public void StopAttack() => _anim?.StopAttack();

        public bool SnapToNavMesh(float searchRadius = 2.0f)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
            {
                if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                {
                    _agent.Warp(hit.position);
                }
                else
                {
                    transform.position = hit.position;
                }
                return true;
            }
            return false;
        }

        private void OnAnimatorMove()
        {
            if (_anim == null || _anim.Animator == null) return;

            Animator animator = _anim.Animator;
            if (!animator.applyRootMotion) return;

            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                float dt = Time.deltaTime > 0.0001f ? Time.deltaTime : 0.02f;
                _agent.velocity = animator.deltaPosition / dt;
                transform.rotation = animator.rootRotation;
            }
            else
            {
                Vector3 targetPos = transform.position + animator.deltaPosition;
                if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, MaxNavMeshDistance, NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                }
                else
                {
                    transform.position = targetPos;
                }
                transform.rotation = animator.rootRotation;
            }
        }

        private void LateUpdate()
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                return;
            }

            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, MaxNavMeshDistance, NavMesh.AllAreas))
            {
                if (Vector3.Distance(transform.position, hit.position) > 0.05f)
                {
                    transform.position = Vector3.Lerp(transform.position, hit.position, Time.deltaTime * 15f);
                }
            }
        }
    }
}
