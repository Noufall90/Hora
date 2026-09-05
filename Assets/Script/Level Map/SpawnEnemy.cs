using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private LevelSpawn[] levelSpawns;
    [SerializeField] private InteractScene interactScene;
    [SerializeField] private InteractScenePanel interactScenePanel;
    [SerializeField] private GameObject levelPanelNotif;

    private readonly List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine spawnCoroutine;
    private bool isSpawning = false;

    public bool IsSpawning => isSpawning;

    private void Awake()
    {
        SetInteractActive(false);

        if (levelPanelNotif != null)
        {
            levelPanelNotif.SetActive(false);
        }
    }

    private void Start()
    {
        if (levelSpawns == null || levelSpawns.Length == 0)
        {
            SetInteractActive(true);
            return;
        }

        StartWaveSpawn();
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

        isSpawning = false;
    }

    private IEnumerator ProcessLevelSpawnsRoutine()
    {
        isSpawning = true;
        SetInteractActive(false);

        // Loop melalui setiap wave (Wave 0 -> Wave 1 -> dst.)
        for (int levelIndex = 0; levelIndex < levelSpawns.Length; levelIndex++)
        {
            LevelSpawn level = levelSpawns[levelIndex];
            if (level == null || level.EnemyWaves == null || level.EnemyWaves.Length == 0)
            {
                continue;
            }

            level.CurrentWave = levelIndex;
            activeEnemies.Clear();

            // Spawn seluruh musuh yang ada di wave ini (1 EnemyWave = 1 Prefab + 1 Location)
            for (int i = 0; i < level.EnemyWaves.Length; i++)
            {
                EnemyWave enemyData = level.EnemyWaves[i];
                if (enemyData != null && enemyData.EnemyPrefab != null)
                {
                    SpawnSingleEnemy(enemyData.EnemyPrefab, enemyData.EnemyLocation);
                }
            }

            // Tunggu hingga semua musuh di wave ini mati/hancur
            while (HasAliveEnemies())
            {
                yield return new WaitForSeconds(0.5f);
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

    private void SpawnSingleEnemy(GameObject prefab, GameObject location)
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
                };
            }
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
}
