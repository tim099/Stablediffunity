using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core;
using UCL.Core.UI;
using UnityEngine;


namespace SDU
{
    [System.Serializable]
    public class InstallSetting : UCL.Core.UI.UCLI_FieldOnGUI
    {
        public string EnvInstallRoot = Path.Combine(DefaultInstallRoot, "Env");
        public string WebUIInstallRoot = Path.Combine(DefaultInstallRoot, "WebUI");
        public string PythonInstallRoot = Path.Combine(DefaultInstallRoot, "Python");

        public static string DefaultInstallRoot => Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.CommonDocuments), "StableDiffUnity_V1");
        public static List<string> EnvRequiredFiles = new List<string>() { 
            RunBatName, RunPythonName, "environment.bat", "webui-user.bat"};
        public static List<string> PythonRequiredFiles = new List<string>() { 
            PythonExeName };
        public static List<string> WebUIRequiredFiles = new List<string>() { 
            "webui.bat", "webui.py", "launch.py", "webui.sh"};

        public const string PythonExeName = "python.exe";
        public const string RunBatName = "run.bat";
        public const string RunPythonName = "run.py";

        public const string EnvZipFileName = "Env_V1.0.zip";
        public const string WebUIZipFileName = "WebUI1.2.1.zip";
        public const string PythonZipFileName = "Python_310.zip";


        public static string InstallStableDiffusionRoot => Path.Combine(Application.streamingAssetsPath, "InstallStableDiffUnity");
        public static string EnvZipPath => Path.Combine(InstallStableDiffusionRoot, EnvZipFileName);
        public static string WebUIZipPath => Path.Combine(InstallStableDiffusionRoot, WebUIZipFileName);
        public static string PythonZipPath => Path.Combine(InstallStableDiffusionRoot, PythonZipFileName);
        public static string WebUISourcePath => Path.Combine(InstallStableDiffusionRoot, ".WebUI");
        public static string WebUIExtensionSourcePath => Path.Combine(WebUISourcePath, "extensions");

        public string RunPythonPath => Path.Combine(EnvInstallRoot, RunPythonName);

        public string OutputPath => Path.Combine(EnvInstallRoot, "Output");
        #region DownloadSettings
        public string DownloadSettingsPath => Path.Combine(Application.streamingAssetsPath , "InstallStableDiffUnity", ".Env", "InstallSettings");
            //Path.Combine(EnvInstallRoot, "InstallSettings");

        #endregion
        //public string ConfigFilePath => Path.Combine(EnvInstallRoot, "Config.json");
        public string PythonInstallPathFilePath => Path.Combine(EnvInstallRoot, "PythonRoot.txt");
        public string WebUIInstallPathFilePath => Path.Combine(EnvInstallRoot, "WebUIInstallPath.txt");
        public string CommandlindArgsFilePath => Path.Combine(EnvInstallRoot, "CommandlindArgs.txt");
        public string RunBatFilePath => Path.Combine(EnvInstallRoot, "RunBatFilePath.txt");
        public string EnvVersionFilePath => Path.Combine(EnvInstallRoot, "EnvVersion.txt");


        #region WebUI
        public string WebUIModelsRootPath => Path.Combine(WebUIInstallRoot, "models");
        public string WebUIExtensionInstallPath => Path.Combine(WebUIInstallRoot, "extensions");
        public string WebUIModelsPath => Path.Combine(WebUIModelsRootPath, "Stable-diffusion");
        public string WebUILoraPath => Path.Combine(WebUIModelsRootPath, "Lora");
        public string WebUIVAEPath => Path.Combine(WebUIModelsRootPath, "VAE");
        #endregion

        /// <summary>
        /// inside Env folder
        /// </summary>
        public string RunBatPath => Path.Combine(EnvInstallRoot, RunBatName);
        /// <summary>
        /// PythonExePath inside Env folder
        /// </summary>
        public string PythonExePath => Path.Combine(PythonInstallRoot, PythonExeName);
        
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            using(var aScope = new GUILayout.VerticalScope())//"box"
            {
                var aDic = iDataDic.GetSubDic("InstallSetting");
                UCL.Core.UI.UCL_GUILayout.DrawField(this, aDic, iFieldName, false);


                GUILayout.BeginHorizontal();
                bool IsShow = aDic.GetData(UCL_GUILayout.IsShowFieldKey, false);
                if (IsShow)
                {
                    var aIsShowOpenFolder = UCL.Core.UI.UCL_GUILayout.Toggle(iDataDic, "ShowOpenFolder");

                    using (var aScope2 = new GUILayout.VerticalScope("box"))
                    {
                        GUILayout.Label("Open Folder", GUILayout.ExpandWidth(false));
                        if (aIsShowOpenFolder)
                        {
                            foreach (FolderEnum aEnum in Enum.GetValues(typeof(FolderEnum)))
                            {
                                if (GUILayout.Button($"Open {aEnum} Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                                {
                                    OpenFolder(aEnum);
                                }
                            }
                            //C:\Users\Public\Documents\StableDiffUnity_V1\WebUI\models\Stable-diffusion
                        }
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
                case FolderEnum.VAE: return Path.Combine(DownloadSettingsPath, "VAE");
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
                case FolderEnum.CheckPoints: return WebUIModelsPath;
                case FolderEnum.Lora: return WebUILoraPath;
                case FolderEnum.LyCORIS: return Path.Combine(WebUIModelsRootPath, "LyCORIS");
                case FolderEnum.VAE: return WebUIVAEPath;
                case FolderEnum.Txt2ImgPreset: return Path.Combine(EnvInstallRoot, "Preset", "Txt2Img");
                case FolderEnum.Img2ImgPreset: return Path.Combine(EnvInstallRoot, "Preset", "Img2Img");
                    
                //case FolderEnum.ControlNetModel: return Path.Combine(WebUIInstallRoot, "extensions", "sd-webui-controlnet", "models");
                case FolderEnum.ControlNetModel: return Path.Combine(WebUIInstallRoot, "models", "ControlNet");
            }
            return string.Empty;
        }
        public void OpenFolder(FolderEnum iFolderEnum)
        {
            string aPath = GetFolderPath(iFolderEnum);
            OpenFolder(aPath);
        }

        #region static
        public static void OpenFolder(string iPath)
        {
            if (string.IsNullOrEmpty(iPath))
            {
                Debug.LogError($"OpenFolder string.IsNullOrEmpty(iPath)!!");
                return;
            }
            if (!Directory.Exists(iPath))
            {
                Debug.LogError($"OpenFolder !Directory.Exists Path:{iPath},string.IsNullOrEmpty(aPath)");
                return;
            }
            System.Diagnostics.Process.Start(iPath);
        }
        #endregion
    }
}