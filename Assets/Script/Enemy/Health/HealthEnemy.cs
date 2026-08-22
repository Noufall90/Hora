using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class EnemyHealth : Health
    {
        [Header("Death Dissolve Settings")]
        [SerializeField] private int coinValue;
        [SerializeField] private Renderer[] enemyRenderers;
        [SerializeField] private float dissolveDuration = 2f;
        private static readonly int DissolvePropertyHash = Shader.PropertyToID("_Dissolve");

        private bool isDead = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            isDead = false;
            ResetDissolveValue();
        }

        public override void TakeDamage(int amount)
        {
            if (isDead || currentHealth <= 0) return;

            base.TakeDamage(amount);

            Debug.Log($"[EnemyHealth] {gameObject.name} took {amount} damage. Remaining Health: {currentHealth}/{maxHealth}");
        }

        protected override void Die()
        {
            if (isDead) return;
            isDead = true;

            if (CoinCounter.Instance != null)
            {
                CoinCounter.Instance.IncreaseCoin(coinValue);
            }

            StopEnemy();

            StartCoroutine(AnimateDeathDissolve());
        }

        private void StopEnemy()
        {
            // Stop NavMeshAgent movement
            NavMeshAgent agent = GetComponent<NavMeshAgent>() ?? GetComponentInChildren<NavMeshAgent>();
            if (agent != null)
            {
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }
                agent.enabled = false;
            }

            // Disable Enemy AI Brain
            EnemyBrain brain = GetComponent<EnemyBrain>() ?? GetComponentInChildren<EnemyBrain>();
            if (brain != null)
            {
                brain.enabled = false;
            }

            // Disable other enemy scripts so enemy stays completely still
            MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script != this && script != null && !(script is HealthEnemyUI))
                {
                    script.enabled = false;
                }
            }

            // Disable colliders so it won't block player or bullets while dissolving
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                if (col != null) col.enabled = false;
            }

            // Freeze/disable Animator if any
            Animator anim = GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.enabled = false;
            }
        }

        private void ResetDissolveValue()
        {
            if (enemyRenderers == null || enemyRenderers.Length == 0)
            {
                enemyRenderers = GetComponentsInChildren<Renderer>(true);
            }

            if (enemyRenderers != null)
            {
                foreach (var rend in enemyRenderers)
                {
                    if (rend == null) continue;

                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    rend.GetPropertyBlock(mpb);
                    mpb.SetFloat(DissolvePropertyHash, 1f);
                    rend.SetPropertyBlock(mpb);

                    Material[] mats = rend.materials;
                    foreach (var mat in mats)
                    {
                        if (mat != null && mat.HasProperty(DissolvePropertyHash))
                        {
                            mat.SetFloat(DissolvePropertyHash, 1f);
                        }
                    }
                }
            }
        }

        private IEnumerator AnimateDeathDissolve()
        {
            if (enemyRenderers == null || enemyRenderers.Length == 0)
            {
                enemyRenderers = GetComponentsInChildren<Renderer>(true);
            }

            float elapsed = 0f;
            while (elapsed < dissolveDuration)
            {
                elapsed += Time.deltaTime;
                float currentDissolve = Mathf.Lerp(1f, 0f, elapsed / dissolveDuration);

                if (enemyRenderers != null)
                {
                    foreach (var rend in enemyRenderers)
                    {
                        if (rend == null) continue;

                        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                        rend.GetPropertyBlock(mpb);
                        mpb.SetFloat(DissolvePropertyHash, currentDissolve);
                        rend.SetPropertyBlock(mpb);

                        Material[] mats = rend.materials;
                        foreach (var mat in mats)
                        {
                            if (mat != null && mat.HasProperty(DissolvePropertyHash))
                            {
                                mat.SetFloat(DissolvePropertyHash, currentDissolve);
                            }
                        }
                    }
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}