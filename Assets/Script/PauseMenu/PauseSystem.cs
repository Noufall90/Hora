using System;
using System.Collections;
using UnityEngine;

public class PauseSystem : MonoBehaviour
{
    private static PauseSystem instance;
    public static PauseSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PauseSystem>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PauseSystem");
                    instance = go.AddComponent<PauseSystem>();
                }
            }
            return instance;
        }
    }

    public bool IsPaused { get; private set; }

    public event Action OnPaused;
    public event Action OnResumed;

    private Coroutine timeStopCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    public void Pause()
    {
        if (timeStopCoroutine != null)
        {
            StopCoroutine(timeStopCoroutine);
            timeStopCoroutine = null;
        }

        if (IsPaused)
            return;

        IsPaused = true;
        Time.timeScale = 0f;
        OnPaused?.Invoke();
    }

    public void Resume()
    {
        if (timeStopCoroutine != null)
        {
            StopCoroutine(timeStopCoroutine);
            timeStopCoroutine = null;
        }

        if (!IsPaused)
            return;

        IsPaused = false;
        Time.timeScale = 1f;
        OnResumed?.Invoke();
    }

    public void Toggle()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void StopTime(float duration)
    {
        if (duration <= 0f)
            return;

        if (timeStopCoroutine != null)
        {
            StopCoroutine(timeStopCoroutine);
        }

        timeStopCoroutine = StartCoroutine(TimeStopRoutine(duration));
    }

    private IEnumerator TimeStopRoutine(float duration)
    {
        float previousScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        if (!IsPaused)
        {
            IsPaused = true;
            Time.timeScale = 0f;
            OnPaused?.Invoke();
        }

        yield return new WaitForSecondsRealtime(duration);

        IsPaused = false;
        if (SpawnEnemy.Instance != null && SpawnEnemy.Instance.IsSlowMotionActive)
        {
            Time.timeScale = SpawnEnemy.Instance.SlowMotionTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else
        {
            Time.timeScale = previousScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        OnResumed?.Invoke();

        timeStopCoroutine = null;
    }
}

public static class Pause
{
    public static bool IsPaused =>
        PauseSystem.Instance != null && PauseSystem.Instance.IsPaused;

    public static void On()
    {
        PauseSystem.Instance?.Pause();
    }

    public static void Off()
    {
        PauseSystem.Instance?.Resume();
    }

    public static void Toggle()
    {
        PauseSystem.Instance?.Toggle();
    }

    public static void ForceResume()
    {
        PauseSystem.Instance?.Resume();
    }

    public static void StopTime(float duration)
    {
        PauseSystem.Instance?.StopTime(duration);
    }

    public static void TimeStop(float duration)
    {
        PauseSystem.Instance?.StopTime(duration);
    }

    public static void For(float duration)
    {
        PauseSystem.Instance?.StopTime(duration);
    }
}