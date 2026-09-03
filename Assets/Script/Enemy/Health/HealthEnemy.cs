using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class EnemyHealth : Health
    {
        [Header("Damage Feedback Settings")]
        public GameObject damageParticle;
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float damageFlashDuration = 0.1f;

        [Header("Death Dissolve Settings")]
        [SerializeField] private int coinValue;
        [SerializeField] private Renderer[] enemyRenderers;
        [SerializeField] private float dissolveDuration = 2f;
        private static readonly int DissolvePropertyHash = Shader.PropertyToID("_Dissolve");
        private static readonly int BaseColorPropertyHash = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorPropertyHash = Shader.PropertyToID("_Color");
        private static readonly int BaseColorAltPropertyHash = Shader.PropertyToID("_Base_Color");

        [Header("Death Slowmotion Settings")]
        [SerializeField] private bool enableDeathSlowMotion = true;
        [SerializeField] private float slowMotionTimeScale = 0.5f;
        [SerializeField] private float slowMotionDuration = 1f;

        private static Coroutine activeSlowMotionCoroutine;
        private static EnemyHealth slowMotionHost;

        private Coroutine _damageFlashCoroutine;
        private bool isDead = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            isDead = false;
            SetEnemyColor(Color.white);
            ResetDissolveValue();
        }

        private void OnDisable()
        {
            if (_damageFlashCoroutine != null)
            {
                StopCoroutine(_damageFlashCoroutine);
                _damageFlashCoroutine = null;
            }

            SetEnemyColor(Color.white);

            if (slowMotionHost == this)
            {
                if (PauseSystem.Instance == null || !PauseSystem.Instance.IsPaused)
                {
                    Time.timeScale = 1f;
                    Time.fixedDeltaTime = 0.02f;
                }
                activeSlowMotionCoroutine = null;
                slowMotionHost = null;
            }
        }

        public override void TakeDamage(int amount)
        {
            if (isDead || currentHealth <= 0) return;

            base.TakeDamage(amount);

            Debug.Log($"[EnemyHealth] {gameObject.name} took {amount} damage. Remaining Health: {currentHealth}/{maxHealth}");

            if (!isDead && currentHealth > 0)
            {
                TriggerDamageFlash();
                TriggerDamageParticle();
            }
        }

        private void TriggerDamageFlash()
        {
            if (_damageFlashCoroutine != null)
            {
                StopCoroutine(_damageFlashCoroutine);
            }

            _damageFlashCoroutine = StartCoroutine(DamageFlashRoutine());
        }

        private IEnumerator DamageFlashRoutine()
        {
            SetEnemyColor(damageFlashColor);
            yield return new WaitForSeconds(damageFlashDuration);
            SetEnemyColor(Color.white);
            _damageFlashCoroutine = null;
        }

        private void SetEnemyColor(Color color)
        {
            if (enemyRenderers == null || enemyRenderers.Length == 0)
            {
                enemyRenderers = GetComponentsInChildren<Renderer>(true);
            }

            if (enemyRenderers == null) return;

            foreach (var rend in enemyRenderers)
            {
                if (rend == null || rend is ParticleSystemRenderer || rend is TrailRenderer || rend is LineRenderer) continue;

                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                rend.GetPropertyBlock(mpb);
                mpb.SetColor(BaseColorPropertyHash, color);
                mpb.SetColor(ColorPropertyHash, color);
                mpb.SetColor(BaseColorAltPropertyHash, color);
                rend.SetPropertyBlock(mpb);

                Material[] mats = rend.materials;
                if (mats != null)
                {
                    foreach (var mat in mats)
                    {
                        if (mat == null) continue;
                        if (mat.HasProperty(BaseColorPropertyHash)) mat.SetColor(BaseColorPropertyHash, color);
                        if (mat.HasProperty(ColorPropertyHash)) mat.SetColor(ColorPropertyHash, color);
                        if (mat.HasProperty(BaseColorAltPropertyHash)) mat.SetColor(BaseColorAltPropertyHash, color);
                    }
                }
            }
        }

        private void TriggerDamageParticle()
        {
            if (damageParticle == null) return;

            // Jika damageParticle merupakan child / objek di scene
            if (damageParticle.scene.IsValid() || damageParticle.transform.IsChildOf(transform))
            {
                damageParticle.SetActive(true);
                ParticleSystem[] systems = damageParticle.GetComponentsInChildren<ParticleSystem>(true);
                if (systems != null && systems.Length > 0)
                {
                    foreach (var ps in systems)
                    {
                        if (ps == null) continue;
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        ps.Play(true);
                    }
                }
            }
            else
            {
                Vector3 spawnPos = transform.position + Vector3.up * 1f;
                if (enemyRenderers != null && enemyRenderers.Length > 0 && enemyRenderers[0] != null)
                {
                    spawnPos = enemyRenderers[0].bounds.center;
                }

                GameObject spawned = Instantiate(damageParticle, spawnPos, Quaternion.identity);
                spawned.SetActive(true);

                ParticleSystem[] systems = spawned.GetComponentsInChildren<ParticleSystem>(true);
                if (systems != null && systems.Length > 0)
                {
                    foreach (var ps in systems)
                    {
                        if (ps == null) continue;
                        ps.Play(true);
                    }
                }
                Destroy(spawned, 2f);
            }
        }

        protected override void Die()
        {
            if (isDead) return;
            isDead = true;

            if (_damageFlashCoroutine != null)
            {
                StopCoroutine(_damageFlashCoroutine);
                _damageFlashCoroutine = null;
            }

            SetEnemyColor(Color.white);

            if (CoinCounter.Instance != null)
            {
                CoinCounter.Instance.IncreaseCoin(coinValue);
            }

            StopEnemy();

            if (enableDeathSlowMotion)
            {
                TriggerSlowMotion();
            }

            StartCoroutine(AnimateDeathDissolve());
        }

        private void TriggerSlowMotion()
        {
            if (activeSlowMotionCoroutine != null && slowMotionHost != null)
            {
                slowMotionHost.StopCoroutine(activeSlowMotionCoroutine);
            }

            slowMotionHost = this;
            activeSlowMotionCoroutine = StartCoroutine(SlowMotionRoutine());
        }

        private IEnumerator SlowMotionRoutine()
        {
            Time.timeScale = slowMotionTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            yield return new WaitForSecondsRealtime(slowMotionDuration);

            if (PauseSystem.Instance == null || !PauseSystem.Instance.IsPaused)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
            }

            activeSlowMotionCoroutine = null;
            slowMotionHost = null;
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