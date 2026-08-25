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
            EquipPistol(activeShooterIndex, false);
            EquipMelee(activeMeleeIndex, false);
        }

        private PistolItem GetPistolProvider()
        {
            if (pistolItem != null) return pistolItem;
            if (PistolItem.Instance != null) return PistolItem.Instance;
            pistolItem = FindObjectOfType<PistolItem>();
            return pistolItem;
        }

        private MeeleItem GetMeleeProvider()
        {
            if (meeleItem != null) return meeleItem;
            if (MeeleItem.Instance != null) return MeeleItem.Instance;
            meeleItem = FindObjectOfType<MeeleItem>();
            return meeleItem;
        }

        public void EquipPistol(int index, bool forceShow = true)
        {
            PistolItem provider = GetPistolProvider();
            if (provider == null || provider.Count == 0)
            {
                Debug.LogWarning("[WeaponsManager] PistolItem provider tidak ditemukan atau meeleItems/pistolItems kosong!");
                return;
            }

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
                currentPistolObject.transform.localScale = Vector3.one;

                PlayerData.PlayerAnimAttack animAttack = PlayerData.PlayerAnimAttack.Instance != null ? PlayerData.PlayerAnimAttack.Instance : GetComponentInParent<PlayerData.PlayerAnimAttack>();
                if (animAttack == null) animAttack = FindObjectOfType<PlayerData.PlayerAnimAttack>();

                if (animAttack != null)
                {
                    animAttack.HideDefaultDissolvePistol();
                    animAttack.InitializeWeaponDissolve(currentPistolObject, forceShow);
                }
                else
                {
                    currentPistolObject.SetActive(forceShow);
                }
                Debug.Log($"[WeaponsManager] Berhasil Equip Pistol Index {activeShooterIndex} ({data.weaponName})");
            }
            else
            {
                Debug.LogWarning($"[WeaponsManager] Prefab Pistol Index {activeShooterIndex} ({data.weaponName}) masih NULL di PistolItem script!");
            }

            if (WeaponsActions.Instance != null)
            {
                WeaponsActions.Instance.SyncPistolEnergyWithData();
            }
        }

        public void EquipMelee(int index, bool forceShow = true)
        {
            MeeleItem provider = GetMeleeProvider();
            if (provider == null || provider.Count == 0)
            {
                Debug.LogWarning("[WeaponsManager] MeeleItem provider tidak ditemukan atau meeleItems array kosong!");
                return;
            }

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
                currentMeleeObject.transform.localScale = Vector3.one;

                PlayerData.PlayerAnimAttack animAttack = PlayerData.PlayerAnimAttack.Instance != null ? PlayerData.PlayerAnimAttack.Instance : GetComponentInParent<PlayerData.PlayerAnimAttack>();
                if (animAttack == null) animAttack = FindObjectOfType<PlayerData.PlayerAnimAttack>();

                if (animAttack != null)
                {
                    animAttack.HideDefaultDissolvePedang();
                    animAttack.InitializeWeaponDissolve(currentMeleeObject, forceShow);
                }
                else
                {
                    currentMeleeObject.SetActive(forceShow);
                }
                Debug.Log($"[WeaponsManager] Berhasil Equip Melee Index {activeMeleeIndex} ({data.weaponName})");
            }
            else
            {
                Debug.LogWarning($"[WeaponsManager] Prefab Melee Index {activeMeleeIndex} ({data.weaponName}) masih NULL di MeeleItem script!");
            }
        }

        public void NextPistol()
        {
            PistolItem provider = GetPistolProvider();
            if (provider == null || provider.Count == 0) return;
            int nextIndex = (activeShooterIndex + 1) % provider.Count;
            EquipPistol(nextIndex, true);
        }

        public void PreviousPistol()
        {
            PistolItem provider = GetPistolProvider();
            if (provider == null || provider.Count == 0) return;
            int prevIndex = (activeShooterIndex - 1 + provider.Count) % provider.Count;
            EquipPistol(prevIndex, true);
        }

        public void NextMelee()
        {
            MeeleItem provider = GetMeleeProvider();
            if (provider == null || provider.Count == 0) return;
            int nextIndex = (activeMeleeIndex + 1) % provider.Count;
            EquipMelee(nextIndex, true);
        }

        public void PreviousMelee()
        {
            MeeleItem provider = GetMeleeProvider();
            if (provider == null || provider.Count == 0) return;
            int prevIndex = (activeMeleeIndex - 1 + provider.Count) % provider.Count;
            EquipMelee(prevIndex, true);
        }
    }
}
