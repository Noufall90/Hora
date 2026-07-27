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
            TransitionManager.Instance().Transition(sceneName, transition, startDelay);
        }   
    }
}