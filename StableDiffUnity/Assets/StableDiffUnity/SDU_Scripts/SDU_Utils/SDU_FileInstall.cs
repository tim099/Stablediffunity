using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace SDU
{
    public static class SDU_FileInstall
    {
        public static string Install(string iInstallRoot, string iZipAbsolutePath, string iInstallTarget)
        {
            try
            {
                Debug.LogWarning($"Installing {iInstallTarget}");
                Debug.LogWarning($"zipAbsolutePath:{iZipAbsolutePath}");
                if (!File.Exists(iZipAbsolutePath))
                {
                    Debug.LogError($"ZipAbsolutePath:{iZipAbsolutePath},not found.");
                    return iInstallRoot;
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(iZipAbsolutePath, iInstallRoot, true);
                Debug.Log($"{iInstallTarget} installation finished");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            return iInstallRoot;
        }
        public static string CheckInstall(string iInstallRoot, string iZipAbsolutePath, string iInstallTarget
            ,List<string> iRequiredFiles = null)
        {
            bool aRequireInstall = true;
            if (Directory.Exists(iInstallRoot))//Install done
            {
                Debug.LogWarning($"CheckInstall Directory.Exists iInstallRoot:{iInstallRoot}" +
                    $"\n,iInstallTarget:{iInstallTarget}");
                aRequireInstall = false;
                if (!iRequiredFiles.IsNullOrEmpty())
                {
                    foreach(var aFile in iRequiredFiles)
                    {
                        string aPath = Path.Combine(iInstallRoot, aFile);
                        Debug.LogWarning($"CheckInstall aPath:{aPath}");
                        if (!File.Exists(aPath))
                        {
                            aRequireInstall = true;
                            Debug.LogWarning($"CheckInstall iInstallTarget:{iInstallTarget},!File.Exists:{aPath}");
                            break;
                        }
                    }
                }
                //return iInstallRoot;
            }
            if (aRequireInstall)
            {
                Install(iInstallRoot, iZipAbsolutePath, iInstallTarget);
            }
            
            return iInstallRoot;
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