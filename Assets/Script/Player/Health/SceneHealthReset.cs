using UnityEngine;
using PlayerData;

namespace PlayerData
{
    public class SceneHealthReset : MonoBehaviour
    {
        [SerializeField] private bool resetHealthOnStart = true;

        private void Start()
        {
            if (resetHealthOnStart)
            {
                if (PlayerHealth.Instance != null)
                {
                    PlayerHealth.Instance.ResetToFull();
                }
            }
        }
    }
}