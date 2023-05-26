using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core;
using UnityEngine;


namespace SDU
{
    public enum FolderEnum
    {
        Env,
        WebUI,
        Python,

        /// <summary>
        /// CheckPoint
        /// </summary>
        CheckPoints,

        Lora,

        Tex2ImgPreset,

        ControlNetModel,
    }
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
        #region DownloadSettings
        public string DownloadSettingsPath => Path.Combine(EnvInstallRoot, "InstallSettings");

        #endregion
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
                        foreach(FolderEnum aEnum in Enum.GetValues(typeof(FolderEnum)))
                        {
                            if (GUILayout.Button($"Open {aEnum} Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                            {
                                OpenFolder(aEnum);
                            }
                        }
                        //C:\Users\Public\Documents\StableDiffUnity_V1\WebUI\models\Stable-diffusion
                    }
                }
                GUILayout.EndHorizontal();
            }

            //m_ControlNetSettings.OnGUI(iDataDic.GetSubDic("ControlNetSettings"));
            return this;
        }
        public string GetDownloadSettingsFolderPath(FolderEnum iFolderEnum)
        {
            switch (iFolderEnum)
            {
                case FolderEnum.CheckPoints: return Path.Combine(DownloadSettingsPath, "CheckPoint");
                case FolderEnum.Lora: return Path.Combine(DownloadSettingsPath, "Lora");
            }
            return Path.Combine(DownloadSettingsPath, iFolderEnum.ToString());
        }
        public string GetFolderPath(FolderEnum iFolderEnum)
        {
            switch (iFolderEnum)
            {
                case FolderEnum.Env: return EnvInstallRoot;
                case FolderEnum.WebUI: return WebUIInstallRoot;
                case FolderEnum.Python: return PythonInstallRoot;
                case FolderEnum.CheckPoints: return StableDiffusionModelsPath;
                case FolderEnum.Lora: return StableDiffusionLoraPath;
                case FolderEnum.Tex2ImgPreset: return Path.Combine(EnvInstallRoot, "Preset", "Tex2Img");
                case FolderEnum.ControlNetModel: return Path.Combine(WebUIInstallRoot, "extensions", "sd-webui-controlnet", "models");
            }
            return string.Empty;
        }
        public void OpenDownloadSettingsFolder(FolderEnum iFolderEnum)
        {
            string aPath = GetDownloadSettingsFolderPath(iFolderEnum);
            if (string.IsNullOrEmpty(aPath))
            {
                Debug.LogError($"OpenDownloadSettingsFolder iFolderEnum:{iFolderEnum},string.IsNullOrEmpty(aPath)");
                return;
            }
            System.Diagnostics.Process.Start(aPath);
        }
        public void OpenFolder(FolderEnum iFolderEnum)
        {
            string aPath = GetFolderPath(iFolderEnum);
            if (string.IsNullOrEmpty(aPath))
            {
                Debug.LogError($"OpenEnvFolder iFolderEnum:{iFolderEnum},string.IsNullOrEmpty(aPath)");
                return;
            }
            System.Diagnostics.Process.Start(aPath);
        }
    }
}