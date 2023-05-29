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

                System.IO.Compression.ZipFile.ExtractToDirectory(iZipAbsolutePath, iInstallRoot, false);

                Debug.Log($"{iInstallTarget} installation finished");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            return iInstallRoot;
        }
        public static string CheckInstall(string iInstallRoot, string iZipAbsolutePath, string iInstallTarget)
        {
            if (Directory.Exists(iInstallRoot))//Install done
            {
                Debug.LogWarning($"CheckInstall Directory.Exists(iInstallRoot) iInstallRoot:{iInstallRoot}" +
                    $"\n,iInstallTarget:{iZipAbsolutePath}");
                return iInstallRoot;
            }
            Install(iInstallRoot, iZipAbsolutePath, iInstallTarget);
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