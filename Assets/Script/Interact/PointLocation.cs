using System.Collections;
using UnityEngine;
using PlayerData;

public class PointLocation : MonoBehaviour
{
    [Header("Spawn Location Settings")]
    [Tooltip("ID unik lokasi spawn ini (contoh: SpawnID1, SpawnID2, atau nama scene asal).")]
    [SerializeField] private string spawnID;

    [Tooltip("GameObject Player yang akan dipindahkan ke titik ini (opsional). Jika kosong, akan mencari Player secara otomatis.")]
    [SerializeField] private GameObject player;

    public string SpawnID => spawnID;

    public static string NextSpawnID { get; private set; }
    public static string PreviousSceneName { get; private set; }

    public static void SetSpawnTarget(string targetSpawnID, string currentSceneName = null)
    {
        NextSpawnID = targetSpawnID;
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            PreviousSceneName = currentSceneName;
        }
    }

    private void Awake()
    {
        TryTeleportImmediate();
    }

    private void OnEnable()
    {
        TryTeleportImmediate();
    }

    private void Start()
    {
        if (!TryTeleportImmediate())
        {
            StartCoroutine(TeleportPlayerDelayed());
        }
    }

    private bool TryTeleportImmediate()
    {
        bool isTargetBySpawnID = !string.IsNullOrEmpty(NextSpawnID) && 
                                 string.Equals(spawnID, NextSpawnID, System.StringComparison.OrdinalIgnoreCase);

        bool isTargetBySceneName = string.IsNullOrEmpty(NextSpawnID) && 
                                   !string.IsNullOrEmpty(PreviousSceneName) && 
                                   string.Equals(spawnID, PreviousSceneName, System.StringComparison.OrdinalIgnoreCase);

        if (!isTargetBySpawnID && !isTargetBySceneName)
        {
            return false;
        }

        GameObject targetPlayer = player;
        if (targetPlayer == null && PlayerController.Instance != null)
        {
            targetPlayer = PlayerController.Instance.gameObject;
        }
        if (targetPlayer == null)
        {
            targetPlayer = GameObject.FindWithTag("Player");
        }

        if (targetPlayer != null)
        {
            Teleport(targetPlayer);
            return true;
        }

        return false;
    }

    private IEnumerator TeleportPlayerDelayed()
    {
        yield return null;

        GameObject targetPlayer = player;
        if (targetPlayer == null && PlayerController.Instance != null)
        {
            targetPlayer = PlayerController.Instance.gameObject;
        }
        if (targetPlayer == null)
        {
            targetPlayer = GameObject.FindWithTag("Player");
        }

        if (targetPlayer != null)
        {
            Teleport(targetPlayer);
        }
    }

    private void Teleport(GameObject targetPlayer)
    {
        CharacterController controller = targetPlayer.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        Vector3 oldPos = targetPlayer.transform.position;
        targetPlayer.transform.position = transform.position;
        targetPlayer.transform.rotation = transform.rotation;

        Physics.SyncTransforms();

        if (controller != null) controller.enabled = true;

        // Snap Cinemachine cameras if present in scene to prevent camera lag/glitch
        Vector3 delta = transform.position - oldPos;
        var vcams = FindObjectsOfType<Cinemachine.CinemachineVirtualCamera>();
        foreach (var vcam in vcams)
        {
            if (vcam != null && (vcam.Follow == targetPlayer.transform || vcam.LookAt == targetPlayer.transform))
            {
                vcam.OnTargetObjectWarped(targetPlayer.transform, delta);
                vcam.PreviousStateIsValid = false;
            }
        }

        Debug.Log($"[PointLocation] Player '{targetPlayer.name}' successfully spawned at '{spawnID}' (Position: {transform.position})");
        NextSpawnID = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
}