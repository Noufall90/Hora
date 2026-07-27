using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerWeapons
{
    public class WeaponsManager : MonoBehaviour
    {
        public static WeaponsManager Instance { get; private set; }

        [Header("Weapon Data Providers")]
        public PistolItem pistolItem;
        public MeeleItem meeleItem;

        [Header("Mount Points / Weapon Slots")]
        public Transform pistolMountPoint;
        public Transform meleeMountPoint;

        [Header("Active Weapon Indices")]
        public int activeShooterIndex = 0;
        public int activeMeleeIndex = 0;

        [Header("Instantiated Active Weapons")]
        [SerializeField] private GameObject currentPistolObject;
        [SerializeField] private GameObject currentMeleeObject;

        [Header("Projectile Settings")]
        public Transform firePoint;
        public GameObject bulletPrefab;

        [Header("Melee Settings")]
        public float attackDistance = 2.5f;
        [SerializeField] [Range(0f, 180f)] public float attackRange = 60f; // seperti field of view

        [Header("Debug")]
        [SerializeField] protected bool showDebugGizmos = true;

        public GameObject CurrentPistolObject => currentPistolObject;
        public GameObject CurrentMeleeObject => currentMeleeObject;

        public PistolData CurrentPistolData => GetPistolProvider() != null ? GetPistolProvider().GetWeapon(activeShooterIndex) : default;
        public MeeleData CurrentMeleeData => GetMeleeProvider() != null ? GetMeleeProvider().GetWeapon(activeMeleeIndex) : default;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            EquipPistol(activeShooterIndex);
            EquipMelee(activeMeleeIndex);
        }

        private PistolItem GetPistolProvider()
        {
            if (pistolItem != null) return pistolItem;
            return PistolItem.Instance;
        }

        private MeeleItem GetMeleeProvider()
        {
            if (meeleItem != null) return meeleItem;
            return MeeleItem.Instance;
        }

        public void EquipPistol(int index)
        {
            PistolItem provider = GetPistolProvider();
            if (provider == null || provider.Count == 0) return;

            activeShooterIndex = Mathf.Clamp(index, 0, provider.Count - 1);
            PistolData data = provider.GetWeapon(activeShooterIndex);

            if (currentPistolObject != null)
            {
                if (currentPistolObject.scene.IsValid())
                {
                    Destroy(currentPistolObject);
                }
                currentPistolObject = null;
            }

            if (data.weaponPrefab != null)
            {
                Transform parent = pistolMountPoint != null ? pistolMountPoint : transform;
                currentPistolObject = Instantiate(data.weaponPrefab, parent);
                currentPistolObject.transform.localPosition = Vector3.zero;
                currentPistolObject.transform.localRotation = Quaternion.identity;

                PlayerData.PlayerAnimAttack animAttack = PlayerData.PlayerAnimAttack.Instance != null ? PlayerData.PlayerAnimAttack.Instance : GetComponentInParent<PlayerData.PlayerAnimAttack>();
                if (animAttack != null)
                {
                    animAttack.InitializeWeaponDissolve(currentPistolObject, false);
                }
                else
                {
                    currentPistolObject.SetActive(false);
                }
            }
        }

        public void EquipMelee(int index)
        {
            MeeleItem provider = GetMeleeProvider();
            if (provider == null || provider.Count == 0) return;

            activeMeleeIndex = Mathf.Clamp(index, 0, provider.Count - 1);
            MeeleData data = provider.GetWeapon(activeMeleeIndex);

            if (currentMeleeObject != null)
            {
                if (currentMeleeObject.scene.IsValid())
                {
                    Destroy(currentMeleeObject);
                }
                currentMeleeObject = null;
            }

            if (data.weaponPrefab != null)
            {
                Transform parent = meleeMountPoint != null ? meleeMountPoint : transform;
                currentMeleeObject = Instantiate(data.weaponPrefab, parent);
                currentMeleeObject.transform.localPosition = Vector3.zero;
                currentMeleeObject.transform.localRotation = Quaternion.identity;

                PlayerData.PlayerAnimAttack animAttack = PlayerData.PlayerAnimAttack.Instance != null ? PlayerData.PlayerAnimAttack.Instance : GetComponentInParent<PlayerData.PlayerAnimAttack>();
                if (animAttack != null)
                {
                    animAttack.InitializeWeaponDissolve(currentMeleeObject, false);
                }
                else
                {
                    currentMeleeObject.SetActive(false);
                }
            }
        }

        public void NextPistol()
        {
            PistolItem provider = GetPistolProvider();
            if (provider == null || provider.Count == 0) return;
            int nextIndex = (activeShooterIndex + 1) % provider.Count;
            EquipPistol(nextIndex);
        }

        public void PreviousPistol()
        {
            PistolItem provider = GetPistolProvider();
            if (provider == null || provider.Count == 0) return;
            int prevIndex = (activeShooterIndex - 1 + provider.Count) % provider.Count;
            EquipPistol(prevIndex);
        }

        public void NextMelee()
        {
            MeeleItem provider = GetMeleeProvider();
            if (provider == null || provider.Count == 0) return;
            int nextIndex = (activeMeleeIndex + 1) % provider.Count;
            EquipMelee(nextIndex);
        }

        public void PreviousMelee()
        {
            MeeleItem provider = GetMeleeProvider();
            if (provider == null || provider.Count == 0) return;
            int prevIndex = (activeMeleeIndex - 1 + provider.Count) % provider.Count;
            EquipMelee(prevIndex);
        }

        public Transform GetActiveFirePoint()
        {
            if (currentPistolObject != null)
            {
                Transform childFP = currentPistolObject.transform.Find("FirePoint");
                if (childFP != null) return childFP;

                Transform[] children = currentPistolObject.GetComponentsInChildren<Transform>();
                foreach (var t in children)
                {
                    if (t.name.ToLower().Contains("firepoint")) return t;
                }
            }

            if (firePoint != null) return firePoint;
            return transform;
        }

        public void Shoot()
        {
            PistolData currentPistol = CurrentPistolData;
            GameObject prefabToSpawn = currentPistol.bulletPrefab != null ? currentPistol.bulletPrefab : bulletPrefab;

            Transform spawnPoint = GetActiveFirePoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("[WeaponsManager] FirePoint tidak ditemukan atau belum di-assign!");
                return;
            }

            if (prefabToSpawn != null)
            {
                // Hitung arah lurus mendatar (sejajar tanah / bidang horizontal XZ)
                Vector3 fireDirection = spawnPoint.forward;
                fireDirection.y = 0f;

                if (fireDirection.sqrMagnitude < 0.001f)
                {
                    fireDirection = transform.forward;
                    fireDirection.y = 0f;
                }

                Quaternion straightRotation = Quaternion.LookRotation(fireDirection.normalized);
                Instantiate(prefabToSpawn, spawnPoint.position, straightRotation);
            }
            else
            {
                Debug.LogWarning("[WeaponsManager] BulletPrefab belum di-assign di WeaponsManager atau PistolItem!");
            }
        }
    }
}
