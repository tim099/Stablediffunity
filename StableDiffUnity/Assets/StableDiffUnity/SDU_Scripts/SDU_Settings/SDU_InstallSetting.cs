using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace SDU
{
    [System.Serializable]
    public class InstallSetting
    {
        public static string DefaultInstallRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "StableDiffUnity_V1");
        public string EnvInstallRoot = Path.Combine(DefaultInstallRoot, "Env");
        public string WebUIInstallRoot = Path.Combine(DefaultInstallRoot, "WebUI");
        public string PythonInstallRoot = Path.Combine(DefaultInstallRoot, "Python");
        public string CommandlindArgs = "--api --xformers";

        public string m_EnvZipFileName = "Env_V1.0.zip";
        public string m_WebUIZipFileName = "WebUI1.2.1.zip";
        public string m_PythonZipFileName = "Python_310.zip";

        public string InstallStableDiffusionRoot => Path.Combine(Application.streamingAssetsPath, "InstallStableDiffUnity");
        public string EnvZipPath => Path.Combine(InstallStableDiffusionRoot, m_EnvZipFileName);
        public string WebUIZipPath => Path.Combine(InstallStableDiffusionRoot, m_WebUIZipFileName);
        public string PythonZipPath => Path.Combine(InstallStableDiffusionRoot, m_PythonZipFileName);
        public string RunPythonPath => Path.Combine(EnvInstallRoot, "run.py");

        public string OutputPath => Path.Combine(EnvInstallRoot, "Output");

        //public string ConfigFilePath => Path.Combine(EnvInstallRoot, "Config.json");
        public string PythonInstallPathFilePath => Path.Combine(EnvInstallRoot, "PythonRoot.txt");
        public string WebUIInstallPathFilePath => Path.Combine(EnvInstallRoot, "WebUIInstallPath.txt");
        public string CommandlindArgsFilePath => Path.Combine(EnvInstallRoot, "CommandlindArgs.txt");

        /// <summary>
        /// inside Env folder
        /// </summary>
        public string RunBatPath => Path.Combine(EnvInstallRoot, "run.bat");
        /// <summary>
        /// PythonExePath inside Env folder
        /// </summary>
        public string PythonExePath => Path.Combine(PythonInstallRoot, @"python.exe");
    }
}