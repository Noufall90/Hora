using System.Collections.Generic;
using UnityEngine;

namespace PlayerWeapons
{
    [System.Serializable]
    public struct MeeleData
    {
        public string weaponName;
        public GameObject weaponPrefab;
        public float damage;
        public float attackSpeed;
    }

    public class MeeleItem : MonoBehaviour
    {
        public static MeeleItem Instance { get; private set; }

        [Header("Melee Item List")]
        public MeeleData[] meeleItems;

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

        public MeeleData GetWeapon(int index)
        {
            if (meeleItems == null || meeleItems.Length == 0)
                return default;

            int safeIndex = Mathf.Clamp(index, 0, meeleItems.Length - 1);
            return meeleItems[safeIndex];
        }

        public MeeleData GetWeaponByName(string name)
        {
            if (meeleItems == null) return default;
            foreach (var item in meeleItems)
            {
                if (item.weaponName == name)
                    return item;
            }
            return default;
        }

        public int Count => meeleItems != null ? meeleItems.Length : 0;
    }
}