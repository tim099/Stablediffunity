using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core;
using UnityEngine;


namespace SDU
{
    [System.Serializable]
    public class InstallSetting : UCL.Core.UI.UCLI_FieldOnGUI
    {
        public static string DefaultInstallRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "StableDiffUnity_V1");
        
        public string EnvInstallRoot = Path.Combine(DefaultInstallRoot, "Env");
        public string WebUIInstallRoot = Path.Combine(DefaultInstallRoot, "WebUI");
        public string PythonInstallRoot = Path.Combine(DefaultInstallRoot, "Python");
        public string CommandlindArgs = "--api --xformers";

        public string m_EnvZipFileName = "Env_V1.0.zip";
        public string m_WebUIZipFileName = "WebUI1.2.1.zip";
        public string m_PythonZipFileName = "Python_310.zip";

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public bool m_ShowOpenFolderToggle = false;
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

        public string ModelsRootPath => Path.Combine(WebUIInstallRoot, "models");
        public string StableDiffusionModelsPath => Path.Combine(ModelsRootPath, "Stable-diffusion");
        public string StableDiffusionLoraPath => Path.Combine(ModelsRootPath, "Lora");
        /// <summary>
        /// inside Env folder
        /// </summary>
        public string RunBatPath => Path.Combine(EnvInstallRoot, "run.bat");
        /// <summary>
        /// PythonExePath inside Env folder
        /// </summary>
        public string PythonExePath => Path.Combine(PythonInstallRoot, @"python.exe");

        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            using(var aScope = new GUILayout.VerticalScope())//"box"
            {
                UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic.GetSubDic("InstallSetting"), iFieldName, false);


                GUILayout.BeginHorizontal();
                
                m_ShowOpenFolderToggle = UCL.Core.UI.UCL_GUILayout.Toggle(m_ShowOpenFolderToggle);
                
                using (var aScope2 = new GUILayout.VerticalScope("box"))
                {
                    GUILayout.Label("Open Folder", GUILayout.ExpandWidth(false));
                    if (m_ShowOpenFolderToggle)
                    {
                        if (GUILayout.Button("Open Env Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                        {
                            System.Diagnostics.Process.Start(EnvInstallRoot);
                        }
                        if (GUILayout.Button("Open WebUI Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                        {
                            System.Diagnostics.Process.Start(WebUIInstallRoot);
                        }
                        if (GUILayout.Button("Open Python Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                        {
                            System.Diagnostics.Process.Start(PythonInstallRoot);
                        }
                        if (GUILayout.Button("Open Stable-diffusion models Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                        {
                            System.Diagnostics.Process.Start(StableDiffusionModelsPath);
                        }
                        if (GUILayout.Button("Open Lora Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                        {
                            System.Diagnostics.Process.Start(StableDiffusionLoraPath);
                        }
                        //C:\Users\Public\Documents\StableDiffUnity_V1\WebUI\models\Stable-diffusion
                    }
                }
                GUILayout.EndHorizontal();
            }

            //m_ControlNetSettings.OnGUI(iDataDic.GetSubDic("ControlNetSettings"));
            return this;
        }
    }
}