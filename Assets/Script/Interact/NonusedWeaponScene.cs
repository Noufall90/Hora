using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerData;

public class NonusedWeaponScene : MonoBehaviour
{
    public static bool IsWeaponDisabled { get; private set; } = false;

    [Header("Weapon Settings")]
    [SerializeField] private bool disableWeaponsInThisScene = true;
    [SerializeField] private bool cancelOngoingAttacks = true;

    private void Awake()
    {
        if (disableWeaponsInThisScene)
        {
            IsWeaponDisabled = true;
        }
    }

    private void Start()
    {
        if (disableWeaponsInThisScene)
        {
            ApplyWeaponRestriction();
        }
    }

    private void OnEnable()
    {
        if (disableWeaponsInThisScene)
        {
            IsWeaponDisabled = true;
            ApplyWeaponRestriction();
        }
    }

    private void OnDisable()
    {
        IsWeaponDisabled = false;
        RestoreWeaponUsage();
    }

    private void OnDestroy()
    {
        IsWeaponDisabled = false;
        RestoreWeaponUsage();
    }

    public void ApplyWeaponRestriction()
    {
        IsWeaponDisabled = true;

        if (PlayerAnimAttack.Instance != null)
        {
            PlayerAnimAttack.Instance.CanUseWeapons = false;
            if (cancelOngoingAttacks)
            {
                PlayerAnimAttack.Instance.CancelAttackAndShoot();
            }
        }
        else
        {
            var animAttack = FindFirstObjectByType<PlayerAnimAttack>();
            if (animAttack != null)
            {
                animAttack.CanUseWeapons = false;
                if (cancelOngoingAttacks)
                {
                    animAttack.CancelAttackAndShoot();
                }
            }
        }
    }

    public void RestoreWeaponUsage()
    {
        IsWeaponDisabled = false;

        if (PlayerAnimAttack.Instance != null)
        {
            PlayerAnimAttack.Instance.CanUseWeapons = true;
        }
        else
        {
            var animAttack = FindFirstObjectByType<PlayerAnimAttack>();
            if (animAttack != null)
            {
                animAttack.CanUseWeapons = true;
            }
        }
    }
}
