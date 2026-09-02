using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTransition
{
    public class LoadScene : MonoBehaviour
    {
        public TransitionSettings transition;
        public float startDelay = 1f;
        public string sceneName;

        public void LoadedScene()
        {
            if (TransitionManager.Instance() != null && transition != null)
            {
                TransitionManager.Instance().Transition(sceneName, transition, startDelay);
            }
            else if (!string.IsNullOrEmpty(sceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }   
    }
}