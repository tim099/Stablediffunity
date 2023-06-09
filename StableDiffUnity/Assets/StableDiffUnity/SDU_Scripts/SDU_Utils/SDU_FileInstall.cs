using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core.JsonLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace SDU
{
    public static class SDU_FileInstall
    {
        public class SDUWebUIExtensionVersion : UCL.Core.JsonLib.UnityJsonSerializable
        {
            public Dictionary<string,string> m_ExtensionVersions = new Dictionary<string,string>();
            public string m_EnvVersion;
        }
        const string EnvVersion = "1.0.1";
        public class InstallData
        {
            public InstallData() { }
            public InstallData(string iInstallTarget, string iInstallRoot, string iZipAbsolutePath,
                List<string> iRequiredFiles = null)
            {
                m_InstallTarget = iInstallTarget;
                m_InstallRoot = iInstallRoot;
                m_ZipAbsolutePath = iZipAbsolutePath;
                m_RequiredFiles = iRequiredFiles;
            }
            /// <summary>
            /// Identify install taget(Env,Python,WebUI)
            /// </summary>
            public string m_InstallTarget;
            public string m_InstallRoot;
            public string m_ZipAbsolutePath;
            public List<string> m_RequiredFiles;
        }

        public static void CheckAndInstall(InstallSetting iInstallSetting)
        {
            InstallData aPythonData = new InstallData("Python", iInstallSetting.PythonInstallRoot, iInstallSetting.PythonZipPath,
                 InstallSetting.PythonRequiredFiles);
            if (CheckRequireInstall(aPythonData))
            {
                Install(aPythonData);
            }

            InstallData aEnvData = new InstallData("Env", iInstallSetting.EnvInstallRoot, iInstallSetting.EnvZipPath,
                InstallSetting.EnvRequiredFiles);
            bool aIsRequireInstallEnv = false;
            if (CheckRequireInstall(aEnvData))//Env require Install
            {
                aIsRequireInstallEnv = true;
            }
            else
            {
                string aEnvVersion = GetVersion(iInstallSetting.EnvVersionFilePath);
                if (aEnvVersion != EnvVersion)//Env need update
                {
                    aIsRequireInstallEnv = true;
                    Debug.LogWarning($"Env need update,Cur Ver:{EnvVersion},Install Ver:{aEnvVersion}");
                }
                else
                {
                    Debug.Log($"Env up to date,Cur Ver:{EnvVersion}");
                }
            }
            if (aIsRequireInstallEnv)
            {
                Install(aEnvData);
                SaveEnvVersion(iInstallSetting);//Save Env version after Install
            }

            InstallData aWebUIData = new InstallData("WebUI", iInstallSetting.WebUIInstallRoot, iInstallSetting.WebUIZipPath,
                InstallSetting.WebUIRequiredFiles);
            if (CheckRequireInstall(aWebUIData))
            {
                Install(aWebUIData);
            }
            CheckAndInstallWebUIExtension(iInstallSetting);

        }
        public static void CheckAndInstallWebUIExtension(InstallSetting iInstallSetting)
        {
            //Debug.LogError($"CheckAndInstallWebUIExtension WebUIExtensionSourcePath:{iInstallSetting.WebUIExtensionSourcePath}");
            if (!Directory.Exists(iInstallSetting.WebUIExtensionSourcePath))
            {
                Debug.LogError($"CheckAndInstallWebUIExtension WebUIExtensionSourcePath:{iInstallSetting.WebUIExtensionSourcePath}" +
                    $", !Directory.Exists");
                return;
            }
            SDUWebUIExtensionVersion aSDUWebUIExtensionVersion = new SDUWebUIExtensionVersion();
            string aExtensionVersionFilePath = Path.Combine(iInstallSetting.WebUIExtensionInstallPath, "SDU_ExtensionVersion.json");
            if(File.Exists(aExtensionVersionFilePath))
            {
                string aJsonStr = File.ReadAllText(aExtensionVersionFilePath);
                JsonData aJson = JsonData.ParseJson(aJsonStr);
                aSDUWebUIExtensionVersion.DeserializeFromJson(aJson);
            }

            var aSourceExtensions = UCL.Core.FileLib.Lib.GetFilesName(iInstallSetting.WebUIExtensionSourcePath,
                "*.zip", SearchOption.TopDirectoryOnly, true);
            foreach(var aExtensionZipName in aSourceExtensions)
            {
                var aExtensionName = aExtensionZipName.Replace(".zip", string.Empty);
                //Debug.LogWarning($"SourceExtension:{aExtensionName}");
                string aInstallPath = Path.Combine(iInstallSetting.WebUIExtensionInstallPath, aExtensionName);
                string aInstallZipPath = Path.Combine(iInstallSetting.WebUIExtensionSourcePath, $"{aExtensionName}.zip");
                bool aRequireInstall = false;
                string aSourceVer = GetVersion(Path.Combine(iInstallSetting.WebUIExtensionSourcePath, $"{aExtensionName}_Version.txt"));
                if (Directory.Exists(aInstallPath))//Installed
                {
                    Debug.Log($"Extension:{aExtensionName}, Installed");
                    if (aSDUWebUIExtensionVersion.m_ExtensionVersions.ContainsKey(aExtensionName))//CheckVersion
                    {
                        string aInstallVer = aSDUWebUIExtensionVersion.m_ExtensionVersions[aExtensionName];
                        if (aInstallVer != aSourceVer)
                        {
                            aRequireInstall = true;
                            Debug.LogWarning($"Extension:{aExtensionName}, InstallVer:{aInstallVer},SourceVer:{aSourceVer}");
                        }
                    }
                }
                else//Not Installed
                {
                    Debug.LogWarning($"Extension:{aExtensionName},Not Installed");
                    aRequireInstall = true;
                }
                if(aRequireInstall)
                {
                    aSDUWebUIExtensionVersion.m_ExtensionVersions[aExtensionName] = aSourceVer;
                    Debug.LogWarning($"Start Install Extension:{aExtensionName}," +
                        $"\nInstallSourcePath:{aInstallZipPath}" +
                            $"\nInstallPath:{aInstallPath}");
                    InstallData aInstallData = new InstallData($"Extension[{aExtensionName}]",
                        aInstallPath, aInstallZipPath);
                    Install(aInstallData);
                    //UCL.Core.FileLib.Lib.CopyDirectory(aInstallSourcePath, aInstallPath);
                }
            }

            {
                aSDUWebUIExtensionVersion.m_EnvVersion = EnvVersion;
                string aJsonStr = aSDUWebUIExtensionVersion.SerializeToJson().ToJsonBeautify();
                File.WriteAllText(aExtensionVersionFilePath, aJsonStr);
            }
        }
        public static void SaveEnvVersion(InstallSetting iInstallSetting)
        {
            File.WriteAllText(iInstallSetting.EnvVersionFilePath, EnvVersion);
        }
        public static string GetVersion(string aPath)
        {
            //Debug.LogWarning($"GetEnvVersion aPath:{aPath}");
            if (File.Exists(aPath))
            {
                return File.ReadAllText(aPath);
            }
            return "0.0.0";//No version find
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iInstallData"></param>
        /// <returns>true if need to Install</returns>
        public static bool CheckRequireInstall(InstallData iInstallData)
        {
            bool aRequireInstall = true;
            if (Directory.Exists(iInstallData.m_InstallRoot))//Install done
            {
                Debug.Log($"CheckInstall Directory.Exists iInstallRoot:{iInstallData.m_InstallRoot}" +
                    $"\n,iInstallTarget:{iInstallData.m_InstallTarget}");
                aRequireInstall = false;
                if (!iInstallData.m_RequiredFiles.IsNullOrEmpty())
                {
                    foreach(var aFile in iInstallData.m_RequiredFiles)
                    {
                        string aPath = Path.Combine(iInstallData.m_InstallRoot, aFile);
                        //Debug.LogWarning($"CheckInstall aPath:{aPath}");
                        if (!File.Exists(aPath))
                        {
                            aRequireInstall = true;
                            Debug.LogWarning($"CheckInstall iInstallTarget:{iInstallData.m_InstallTarget},!File.Exists:{aPath}");
                            break;
                        }
                    }
                }
                //return iInstallRoot;
            }
            //if (aRequireInstall)
            //{
            //    Install(iInstallData);
            //}
            
            return aRequireInstall;
        }
        public static string Install(InstallData iInstallData)
        {
            try
            {
                Debug.LogWarning($"Installing {iInstallData.m_InstallTarget}");
                Debug.LogWarning($"zipAbsolutePath:{iInstallData.m_ZipAbsolutePath}");
                if (!File.Exists(iInstallData.m_ZipAbsolutePath))
                {
                    Debug.LogError($"ZipAbsolutePath:{iInstallData.m_ZipAbsolutePath},not found.");
                    return iInstallData.m_InstallRoot;
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(iInstallData.m_ZipAbsolutePath, iInstallData.m_InstallRoot, true);
                Debug.Log($"{iInstallData.m_InstallTarget} installation finished");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            return iInstallData.m_InstallRoot;
        }
        public static string GetEnvInstallSourcePath(string iInstallRoot)
        {
            return iInstallRoot.Replace(RunTimeData.InstallSetting.EnvInstallRoot,
                Application.streamingAssetsPath + @"/InstallStableDiffUnity/.Env");
        }
        public static void CheckInstallEnv(string iInstallRoot)
        {
            if (!Directory.Exists(iInstallRoot))
            {
                var aInstallFrom = GetEnvInstallSourcePath(iInstallRoot);
                Debug.LogWarning($"iInstallRoot:{iInstallRoot},aInstallFrom:{aInstallFrom}");
                if (!Directory.Exists(aInstallFrom))
                {
                    Directory.CreateDirectory(aInstallFrom);
                    Debug.LogError($"CheckInstallEnv iInstallRoot:{iInstallRoot}," +
                        $" !Directory.Exists(aInstallFrom) aInstallFrom:{aInstallFrom}");
                    return;
                }
                UCL.Core.FileLib.Lib.CopyDirectory(aInstallFrom, iInstallRoot);
            }
        }
        public static void LoadInstallEnvFromStreammingAssets(string iInstallRoot)
        {
            var aInstallFrom = GetEnvInstallSourcePath(iInstallRoot);
            if (!Directory.Exists(aInstallFrom))
            {
                return;
            }
            //if (Directory.Exists(iInstallRoot))
            //{
            //    Directory.Delete(iInstallRoot, true);
            //}
            UCL.Core.FileLib.Lib.CopyDirectory(aInstallFrom, iInstallRoot);
        }
        public static void SaveInstallEnvToStreammingAssets(string iInstallRoot)
        {
            if (!Directory.Exists(iInstallRoot))
            {
                return;
            }

            var aInstallTo = GetEnvInstallSourcePath(iInstallRoot);
            if (Directory.Exists(aInstallTo))
            {
                Directory.Delete(aInstallTo, true);
            }
            Debug.LogWarning($"iInstallRoot:{iInstallRoot},aInstallTo:{aInstallTo}");

            UCL.Core.FileLib.Lib.CopyDirectory(iInstallRoot, aInstallTo);
        }
    }
}