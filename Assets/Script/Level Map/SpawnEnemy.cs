using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class EnemyWave
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject enemyLocation;

    public GameObject EnemyPrefab => enemyPrefab;
    public GameObject EnemyLocation => enemyLocation;
}

[System.Serializable]
public class LevelSpawn
{
    [SerializeField] private int currentWave = 0;
    [SerializeField] private float spawnDelayWave = 2f;
    [SerializeField] private EnemyWave[] enemyWaves;

    public int CurrentWave
    {
        get => currentWave;
        set => currentWave = value;
    }

    public float SpawnDelayWave => spawnDelayWave;
    public EnemyWave[] EnemyWaves => enemyWaves;
}

public class SpawnEnemy : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField] private LevelSpawn[] levelSpawns;

    [Header("Slow Motion Settings")]
    [SerializeField] private bool enableWaveSlowMotion = true;
    [SerializeField] private float slowMotionTimeScale = 0.2f;
    [SerializeField] private float slowMotionDuration = 1f;
    [SerializeField] private bool slowMotionOnlyFinalWave = true;

    [Header("UI References")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private GameObject levelPanelNotif;

    [Header("Interactions")]
    [SerializeField] private InteractScene interactScene;
    [SerializeField] private InteractScenePanel interactScenePanel;

    private readonly List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine spawnCoroutine;
    private Coroutine slowMotionCoroutine;
    private bool isSpawning = false;

    public static SpawnEnemy Instance { get; private set; }

    public bool IsSpawning => isSpawning;
    public bool IsSlowMotionActive => slowMotionCoroutine != null;
    public float SlowMotionTimeScale => slowMotionTimeScale;

    private void Awake()
    {
        Instance = this;
        SetInteractActive(false);

        if (levelPanelNotif != null)
        {
            levelPanelNotif.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        int totalWaves = levelSpawns != null ? levelSpawns.Length : 0;
        UpdateWaveText(0, totalWaves);

        if (levelSpawns == null || levelSpawns.Length == 0)
        {
            SetInteractActive(true);
            return;
        }

        StartWaveSpawn();
    }

    private void OnDisable()
    {
        if (slowMotionCoroutine != null)
        {
            StopCoroutine(slowMotionCoroutine);
            slowMotionCoroutine = null;
            ResetTimeScale();
        }
    }

    public void StartWaveSpawn()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(ProcessLevelSpawnsRoutine());
    }

    public void StopWaveSpawn()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        if (slowMotionCoroutine != null)
        {
            StopCoroutine(slowMotionCoroutine);
            slowMotionCoroutine = null;
            ResetTimeScale();
        }

        isSpawning = false;
    }

    private IEnumerator ProcessLevelSpawnsRoutine()
    {
        isSpawning = true;
        SetInteractActive(false);

        int totalWaves = levelSpawns != null ? levelSpawns.Length : 0;

        // Loop melalui setiap wave (Wave 0 -> Wave 1 -> dst.)
        for (int levelIndex = 0; levelIndex < levelSpawns.Length; levelIndex++)
        {
            LevelSpawn level = levelSpawns[levelIndex];
            if (level == null || level.EnemyWaves == null || level.EnemyWaves.Length == 0)
            {
                continue;
            }

            level.CurrentWave = levelIndex;
            UpdateWaveText(levelIndex + 1, totalWaves);
            activeEnemies.Clear();

            bool waveSlowMotionTriggered = false;
            bool isFinalWave = (levelIndex == levelSpawns.Length - 1);

            // Callback ketika salah satu musuh mati
            System.Action onEnemyDefeated = () =>
            {
                if (!HasAliveEnemies() && enableWaveSlowMotion && !waveSlowMotionTriggered)
                {
                    if (!slowMotionOnlyFinalWave || isFinalWave)
                    {
                        waveSlowMotionTriggered = true;
                        TriggerSlowMotion();
                    }
                }
            };

            // Spawn seluruh musuh yang ada di wave ini (1 EnemyWave = 1 Prefab + 1 Location)
            for (int i = 0; i < level.EnemyWaves.Length; i++)
            {
                EnemyWave enemyData = level.EnemyWaves[i];
                if (enemyData != null && enemyData.EnemyPrefab != null)
                {
                    SpawnSingleEnemy(enemyData.EnemyPrefab, enemyData.EnemyLocation, onEnemyDefeated);
                }
            }

            // Tunggu hingga semua musuh di wave ini mati/hancur
            while (HasAliveEnemies())
            {
                yield return null;
            }

            // Fallback jika belum tertrigger (misal musuh didestroy tanpa OnDeath)
            if (enableWaveSlowMotion && !waveSlowMotionTriggered)
            {
                if (!slowMotionOnlyFinalWave || isFinalWave)
                {
                    waveSlowMotionTriggered = true;
                    TriggerSlowMotion();
                }
            }

            // Jeda delay sebelum lanjut ke wave berikutnya jika masih ada wave selanjutnya
            if (level.SpawnDelayWave > 0f && levelIndex < levelSpawns.Length - 1)
            {
                yield return new WaitForSeconds(level.SpawnDelayWave);
            }
        }

        // Semua wave telah selesai
        HandleAllWavesCleared();
    }

    private void SpawnSingleEnemy(GameObject prefab, GameObject location, System.Action onEnemyDied = null)
    {
        if (prefab == null) return;

        Vector3 spawnPos = location != null ? location.transform.position : transform.position;
        Quaternion spawnRot = location != null ? location.transform.rotation : transform.rotation;

        GameObject enemy = Instantiate(prefab, spawnPos, spawnRot);
        if (enemy != null)
        {
            activeEnemies.Add(enemy);

            Health health = enemy.GetComponent<Health>() ?? enemy.GetComponentInChildren<Health>();
            if (health != null)
            {
                health.OnDeath += () =>
                {
                    if (activeEnemies.Contains(enemy))
                    {
                        activeEnemies.Remove(enemy);
                    }
                    onEnemyDied?.Invoke();
                };
            }
        }
    }

    public void TriggerSlowMotion()
    {
        if (slowMotionCoroutine != null)
        {
            StopCoroutine(slowMotionCoroutine);
        }

        slowMotionCoroutine = StartCoroutine(SlowMotionRoutine());
    }

    private IEnumerator SlowMotionRoutine()
    {
        Time.timeScale = slowMotionTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(slowMotionDuration);

        ResetTimeScale();
        slowMotionCoroutine = null;
    }

    private void ResetTimeScale()
    {
        if (PauseSystem.Instance == null || !PauseSystem.Instance.IsPaused)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    private bool HasAliveEnemies()
    {
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count > 0;
    }

    private void HandleAllWavesCleared()
    {
        isSpawning = false;
        spawnCoroutine = null;
        SetInteractActive(true);

        // Verifikasi scene jika terdapat SceneVerfied di level ini
        if (SceneVerfied.Instance != null)
        {
            SceneVerfied.Instance.VerifyLevel();
        }

        // Tampilkan levelPanelNotif selama 5 detik
        if (levelPanelNotif != null)
        {
            StartCoroutine(ShowLevelPanelNotifRoutine());
        }
    }

    private IEnumerator ShowLevelPanelNotifRoutine()
    {
        levelPanelNotif.SetActive(true);
        yield return new WaitForSeconds(5f);
        levelPanelNotif.SetActive(false);
    }

    private void SetInteractActive(bool isActive)
    {
        if (interactScene != null)
        {
            interactScene.gameObject.SetActive(isActive);
            interactScene.enabled = isActive;
        }

        if (interactScenePanel != null)
        {
            interactScenePanel.gameObject.SetActive(isActive);
            interactScenePanel.enabled = isActive;
        }
    }

    private void UpdateWaveText(int current, int total)
    {
        if (waveText != null)
        {
            waveText.text = $"{current}/{total}";
        }
    }
}
