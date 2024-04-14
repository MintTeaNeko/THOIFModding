using System;
using System.Collections.Generic;
using UnityEngine;

namespace THOIF_Mod_Loader
{
    public class THOIF_Event_Manager : MonoBehaviour
    {
        private string currentSceneName = "";

        public static THOIF_Event_Manager Instance;

        public delegate void SceneChanged(string scene);
        public static event SceneChanged sceneChanged;

        private void Start()
        {
            Instance = this;
        }

        private void Update ()
        {
            if (currentSceneName != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                NotifySceneChanged(currentSceneName);
            }
        }

        public void NotifySceneChanged(string sceneName)
        {
            sceneChanged?.Invoke(sceneName);
        }
    }
}
