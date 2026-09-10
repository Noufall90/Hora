using System.Collections.Generic;
using Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace BT.Core
{
    public class EnemyBT : BehaviourTree
    {
        protected EnemyBrain brain;

        private float idleTimer = 0f;
        private float idleDuration = 1.5f;

        private float attackCooldown = 1.5f;
        private float attackCooldownTimer = 0f;

        private bool isPatrolling = false;
        private Vector3 currentPatrolPoint;

        private bool hasInvestigateTarget = false;
        private Vector3 investigateTarget;

        public EnemyBT(EnemyBrain brain, float attackCooldown = 1.5f, float idleDuration = 1.5f)
        {
            this.brain = brain;
            this.attackCooldown = attackCooldown;
            this.idleDuration = idleDuration;
            this.attackCooldownTimer = attackCooldown; // Siap serang saat pertama masuk combat

            BuildTree();
        }

        public bool IsInvestigating => hasInvestigateTarget;

        public void SetInvestigateTarget(Vector3 position)
        {
            investigateTarget = position;
            hasInvestigateTarget = true;
        }

        public void ClearInvestigateTarget()
        {
            hasInvestigateTarget = false;
        }

        protected virtual void BuildTree()
        {
            // === 1. COMBAT BRANCH ===
            // Combat Sequence: Jika player terdeteksi -> (Attack jika dalam range, atau Chase)
            SequenceNode combatSequence = new SequenceNode(
                new ConditionNode(CheckPlayerDetected),
                new SelectorNode(
                    // Attack Sequence: Jika dalam jangkauan -> Serang
                    new SequenceNode(
                        new ConditionNode(CheckInAttackRange),
                        new ActionNode(ExecuteAttack)
                    ),
                    // Chase Action: Jika di luar jangkauan -> Kejar
                    new ActionNode(ExecuteChase)
                )
            );

            // === 2. INVESTIGATE BRANCH ===
            // Investigate Sequence: Jika ada target investigasi -> Bergerak ke sana
            SequenceNode investigateSequence = new SequenceNode(
                new ConditionNode(() => hasInvestigateTarget),
                new ActionNode(ExecuteInvestigate)
            );

            // === 3. PATROL & IDLE BRANCH ===
            // Patrol / Idle Selector: Patroli jika bisa, atau Idle
            SelectorNode patrolIdleSelector = new SelectorNode(
                new ActionNode(ExecutePatrol),
                new ActionNode(ExecuteIdle)
            );

            // === ROOT SELECTOR ===
            // Prioritas: Combat -> Investigate -> Patrol/Idle
            rootNode = new SelectorNode(
                combatSequence,
                investigateSequence,
                patrolIdleSelector
            );
        }

        private bool wasInCombat = false;

        // ==========================================
        // CONDITIONS
        // ==========================================

        private bool CheckPlayerDetected()
        {
            bool detected = brain != null && brain.IsPlayerDetected();
            if (detected)
            {
                wasInCombat = true;
                hasInvestigateTarget = false;
            }
            else
            {
                // Hanya picu investigasi jika musuh sebelumnya sedang dalam combat lalu kehilangan target
                if (wasInCombat)
                {
                    wasInCombat = false;
                    if (brain != null && brain.LastKnownPlayerPosition != Vector3.zero)
                    {
                        SetInvestigateTarget(brain.LastKnownPlayerPosition);
                    }
                }
            }
            return detected;
        }

        private bool CheckInAttackRange()
        {
            if (brain == null) return false;
            float effectiveAttackRange = brain.AttackRange > 0 ? brain.AttackRange : brain.MeeleRange;
            return brain.IsPlayerInDistance(effectiveAttackRange);
        }

        // ==========================================
        // ACTIONS
        // ==========================================

        private NodeState ExecuteAttack()
        {
            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = true;
            }

            brain.RotateTowardsPlayer();

            attackCooldownTimer += Time.deltaTime;
            if (attackCooldownTimer >= attackCooldown)
            {
                if (brain is IMeele meele)
                {
                    meele.MeeleAttack();
                }
                else if (brain is IShooter shooter)
                {
                    shooter.ShootAttack();
                }
                else if (brain is IBomber bomber)
                {
                    bomber.ThrowGranade();
                }
                attackCooldownTimer = 0f;
            }

            return NodeState.Running;
        }

        private NodeState ExecuteChase()
        {
            if (brain.PlayerTarget == null) return NodeState.Failure;

            if (brain is EnemyMeele enemyMeele)
            {
                enemyMeele.StopAttack();
            }

            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = false;
                brain.Agent.speed = brain.MoveSpeed;
                brain.Agent.SetDestination(brain.PlayerTarget.position);
            }

            brain.RotateTowardsPlayer();
            return NodeState.Running;
        }

        private NodeState ExecuteInvestigate()
        {
            if (brain == null) return NodeState.Failure;

            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = false;
                brain.Agent.speed = brain.MoveSpeed;
                brain.Agent.SetDestination(investigateTarget);
            }

            brain.RotateTowardsTarget(investigateTarget);

            if (!brain.CanMove)
            {
                ClearInvestigateTarget();
                return NodeState.Success;
            }

            if (brain.HasActiveNavMeshAgent && !brain.Agent.pathPending && brain.Agent.remainingDistance <= brain.Agent.stoppingDistance)
            {
                ClearInvestigateTarget();
                if (brain.HasActiveNavMeshAgent) brain.Agent.ResetPath();
                return NodeState.Success;
            }

            return NodeState.Running;
        }

        private NodeState ExecutePatrol()
        {
            if (brain == null || !brain.CanMove) return NodeState.Failure;

            if (!isPatrolling)
            {
                Vector3 centre = brain.PatrolCentrePoint.position;
                Vector3 randomPoint = centre + Random.insideUnitSphere * brain.PatrolRange;

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                {
                    currentPatrolPoint = hit.position;
                    isPatrolling = true;

                    if (brain.HasActiveNavMeshAgent)
                    {
                        brain.Agent.isStopped = false;
                        brain.Agent.speed = brain.MoveSpeed;
                        brain.Agent.SetDestination(currentPatrolPoint);
                    }
                }
                else
                {
                    return NodeState.Failure;
                }
            }

            if (brain.HasActiveNavMeshAgent && !brain.Agent.pathPending && brain.Agent.remainingDistance <= brain.Agent.stoppingDistance)
            {
                isPatrolling = false;
                if (brain.HasActiveNavMeshAgent) brain.Agent.ResetPath();
                return NodeState.Success;
            }

            return NodeState.Running;
        }

        private NodeState ExecuteIdle()
        {
            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = true;
            }

            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDuration)
            {
                idleTimer = 0f;
                return NodeState.Success;
            }

            return NodeState.Running;
        }
    }
}
