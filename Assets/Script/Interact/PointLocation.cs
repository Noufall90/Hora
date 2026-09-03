using System.Collections;
using UnityEngine;
using PlayerData;

public class PointLocation : MonoBehaviour
{
    [Header("Spawn Location Settings")]
    [Tooltip("ID unik lokasi spawn ini (contoh: SpawnID1, SpawnID2, HomePoint, atau nama scene asal).")]
    [SerializeField] private string spawnID;

    [Tooltip("Tandai jika titik ini adalah lokasi spawn default ketika tidak ada target spawn yang diset.")]
    [SerializeField] private bool isDefaultPoint = false;

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
        if (IsTargetLocation())
        {
            if (!TryTeleportImmediate())
            {
                StartCoroutine(TeleportPlayerDelayed());
            }
        }
    }

    private bool IsTargetLocation()
    {
        // 1. Jika NextSpawnID ada, hanya point dengan spawnID yang cocok yang boleh spawn
        if (!string.IsNullOrEmpty(NextSpawnID))
        {
            return string.Equals(spawnID, NextSpawnID, System.StringComparison.OrdinalIgnoreCase);
        }

        // 2. Jika NextSpawnID kosong tetapi PreviousSceneName ada, cek apakah cocok dengan spawnID
        if (!string.IsNullOrEmpty(PreviousSceneName))
        {
            if (string.Equals(spawnID, PreviousSceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // 3. Jika tidak ada target spawn sama sekali, gunakan point yang ditandai sebagai default
        if (string.IsNullOrEmpty(NextSpawnID) && string.IsNullOrEmpty(PreviousSceneName))
        {
            return isDefaultPoint;
        }

        return false;
    }

    private GameObject GetTargetPlayer()
    {
        if (player != null) return player;

        if (PlayerController.Instance != null)
        {
            return PlayerController.Instance.gameObject;
        }

        return GameObject.FindWithTag("Player");
    }

    private bool TryTeleportImmediate()
    {
        if (!IsTargetLocation())
        {
            return false;
        }

        GameObject targetPlayer = GetTargetPlayer();
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

        if (!IsTargetLocation())
        {
            yield break;
        }

        GameObject targetPlayer = GetTargetPlayer();
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
        
        // Reset target spawn setelah teleport berhasil agar tidak terpanggil ulang oleh point lain
        NextSpawnID = null;
        PreviousSceneName = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isDefaultPoint ? Color.green : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
}