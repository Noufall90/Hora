using HFSM.Core;
using Enemy;
using UnityEngine;

namespace HFSM.Passive
{
    public class PatrolState : EnemyBaseState
    {
        private Vector3 patrolTarget;

        public PatrolState(EnemyBrain brain, HierarchicalStateMachine stateMachine) : base(brain, stateMachine) { }

        public override void Enter()
        {
            Debug.Log($"{brain.gameObject.name} memasuki PatrolState");
            GenerateNewPatrolPoint();
        }

        public override void Update()
        {
            base.Update();

            if (IsPlayerInDistance(brain.DetectRange))
            {
                // Transisi ke combat jika mendeteksi player
                return;
            }

            // Gerakkan musuh ke arah titik patroli
            brain.transform.position = Vector3.MoveTowards(brain.transform.position, patrolTarget, brain.MoveSpeed * Time.deltaTime);

            // Jika sampai di titik patroli, kembali ke Idle
            if (Vector3.Distance(brain.transform.position, patrolTarget) < 0.2f)
            {
                stateMachine.ChangeState(new IdleState(brain, stateMachine));
            }
        }

        private void GenerateNewPatrolPoint()
        {
            float randomX = Random.Range(-5f, 5f);
            float randomZ = Random.Range(-5f, 5f);
            patrolTarget = brain.transform.position + new Vector3(randomX, 0, randomZ);
        }
    }
}