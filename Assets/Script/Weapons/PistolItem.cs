using System.Collections.Generic;
using UnityEngine;

namespace PlayerWeapons
{
    [System.Serializable]
    public struct PistolData
    {
        public string weaponName;
        public GameObject weaponPrefab;
        public GameObject bulletPrefab;
        public float fireRate;
        public float magazineEnergy;
        public float energyReloadRate;
        public float energyReloadInterval;
    }

    public class PistolItem : MonoBehaviour
    {
        public static PistolItem Instance { get; private set; }

        [Header("Pistol / Shooter Item List")]
        public PistolData[] pistolItems;

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

        public PistolData GetWeapon(int index)
        {
            if (pistolItems == null || pistolItems.Length == 0)
                return default;

            int safeIndex = Mathf.Clamp(index, 0, pistolItems.Length - 1);
            return pistolItems[safeIndex];
        }

        public PistolData GetWeaponByName(string name)
        {
            if (pistolItems == null) return default;
            foreach (var item in pistolItems)
            {
                if (item.weaponName == name)
                    return item;
            }
            return default;
        }

        public int Count => pistolItems != null ? pistolItems.Length : 0;
    }
}