using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace THOIF_Mod_Loader
{
    public class Loader : MonoBehaviour
    {
        public static Loader ModLoader;

        private void Start()
        {
            ModLoader = this;
            DontDestroyOnLoad(this.gameObject);
            loadMods();

            gameObject.AddComponent<THOIF_Event_Manager>();
        }

        private void loadMods()
        {
            string[] directories = Directory.GetDirectories(getModsFolder());
            for (int i = 0; i < directories.Length; i++)
            {
                string directory = directories[i];
                string[] files = Directory.GetFiles(directory);
                for (int n = 0; n < files.Length; n++)
                {
                    string file = files[n];
                    if (file.Contains("manifest.json"))
                    {
                        try
                        {
                            THOIF_Mod_Info modInformation = JsonUtility.FromJson<THOIF_Mod_Info>(File.ReadAllText(file));
                            string[] splitType = modInformation.type.Split(',');
                            string assemblyName = splitType[1].Replace(" ", "") + ".dll";
                            Type mono = Assembly.LoadFile(Path.Combine(directory, assemblyName)).GetType(splitType[0]);
                            GameObject go = new GameObject();
                            go.AddComponent(mono);
                        }
                        catch (Exception e)
                        {
                            Log($"failed to load {file}: {e}");
                        }
                    }
                }
            }
        }

        private string getCurrentPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public string getModLogFolder()
        {
            string modLogFolderPath = Path.Combine(getCurrentPath(), @"..\..\Mod_Logs");
            
            if ( !Directory.Exists(modLogFolderPath) )
            {
                Directory.CreateDirectory(modLogFolderPath);
            }

            return modLogFolderPath;
        }

        public string getModsFolder()
        {
            string modFolderPath = Path.Combine(getCurrentPath(), @"..\..\Mods");

            if (!Directory.Exists(modFolderPath))
            {
                Directory.CreateDirectory(modFolderPath);
            }

            return modFolderPath;
        }

        public void Log(string text)
        {
            string modLogFolder = getModLogFolder();
            File.AppendAllText(Path.Combine(modLogFolder, "log.txt"), $"{text}\n\n");
        }
    }
}
