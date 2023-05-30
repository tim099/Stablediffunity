using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
namespace SDU
{
    public static class SDU_Server
    {
        public static int StartServer()
        {
            int aProcessID = -1;
            var aInstallSetting = RunTimeData.InstallSetting;
            var aPythonRoot = SDU_FileInstall.CheckInstall(aInstallSetting.PythonInstallRoot,
                aInstallSetting.PythonZipPath, "Python", InstallSetting.PythonRequiredFiles);

            var aEnvInstallRoot = SDU_FileInstall.CheckInstall(aInstallSetting.EnvInstallRoot,
                aInstallSetting.EnvZipPath, "Env", InstallSetting.EnvRequiredFiles);

            var aWebUIRoot = SDU_FileInstall.CheckInstall(aInstallSetting.WebUIInstallRoot,
                aInstallSetting.WebUIZipPath, "WebUI", InstallSetting.WebUIRequiredFiles);


            var aPythonExePath = aInstallSetting.PythonExePath;//System.IO.Path.Combine(aEnvInstallRoot, Data.PythonExePath);
            UnityEngine.Debug.LogWarning($"PythonExePath:{aPythonExePath}");
            if (!System.IO.File.Exists(aPythonExePath))
            {
                Debug.LogError($"File not found, aPythonExePath:{aPythonExePath}, try reinstall");
                return aProcessID;
            }
            string aBatPath = RunTimeData.InstallSetting.RunBatPath;//System.IO.Path.Combine(aEnvInstallRoot, Data.WebUIScriptBatPath);
            if (!File.Exists(aBatPath))
            {
                Debug.LogError($"File not found, aBatPath:{aBatPath}, try reinstall");
                return aProcessID;
            }

            File.WriteAllText(aInstallSetting.PythonInstallPathFilePath, aPythonRoot);
            File.WriteAllText(aInstallSetting.WebUIInstallPathFilePath, aWebUIRoot);
            File.WriteAllText(aInstallSetting.CommandlindArgsFilePath, aInstallSetting.CommandlindArgs);

            SDU_ProcessList.PreCheckProcessEvent();//check current exist process
            var aProcess = new System.Diagnostics.Process();

            string aRunPythonPath = aInstallSetting.RunPythonPath;

            UnityEngine.Debug.LogWarning($"RunPythonPath:{aRunPythonPath},BatPath:{aBatPath}");

            aProcess.StartInfo.FileName = aPythonExePath;
            aProcess.StartInfo.Arguments = $"{aRunPythonPath} {aBatPath}";
            aProcess.StartInfo.Verb = string.Empty;

            aProcess.StartInfo.CreateNoWindow = false;
            aProcess.StartInfo.RedirectStandardOutput = RunTimeData.Ins.m_RedirectStandardOutput;

            if (RunTimeData.Ins.m_RedirectStandardOutput)
            {
                aProcess.StartInfo.RedirectStandardOutput = true;
                aProcess.StartInfo.UseShellExecute = false;
                SDU_ProcessList.Init(aProcess);
                SDU_ProcessList.s_OnOutputDataReceivedAct = OnOutputDataReceived;
            }
            else
            {
                aProcess.StartInfo.UseShellExecute = true;
                aProcess.StartInfo.RedirectStandardOutput = false;
            }

            aProcess.Start();

            if (RunTimeData.Ins.m_RedirectStandardOutput)
            {
                aProcess.BeginOutputReadLine();
            }

            SDU_ProcessList.AddProcessEvent(aProcess);
            aProcessID = aProcess.Id;
            UnityEngine.Debug.LogWarning($"Start server, ProcessID:{aProcessID}");

            SDU_WebUIStatus.Ins.SetServerStarted(true);
            SDU_WebUIStatus.Ins.ValidateConnectionContinuously((iServerReady) =>
            {
                try
                {
                    UnityEngine.Debug.LogWarning($"ValidateConnectionContinuously End, ServerReady:{iServerReady}");
                    if (iServerReady)
                    {
                        SDU_ProcessList.CheckProcessEvent();
                        //aProcess.StandardOutput.ReadToEnd();
                        if (RunTimeData.Ins.m_AutoOpenWeb)
                        {
                            System.Diagnostics.Process.Start(RunTimeData.Ins.m_WebURL);
                            UnityEngine.Debug.LogWarning($"Open WebURL:{RunTimeData.Ins.m_WebURL}");
                        }
                        RunTimeData.Ins.m_WebUISetting.RefreshModels().Forget();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

            }).Forget();

            return aProcessID;
        }

        private static void OnOutputDataReceived(string iOutPut)
        {
            UnityEngine.Debug.LogWarning($"DataReceived_:{iOutPut}");

        }

        public static void CheckServerStarted()
        {
            SDU_WebUIStatus.Ins.CheckServerReady((iServerReady) => {
                try
                {
                    Debug.LogWarning($"iServerReady:{iServerReady}");
                    if (iServerReady)
                    {
                        if (RunTimeData.Ins.m_AutoOpenWeb)
                        {
                            System.Diagnostics.Process.Start(RunTimeData.Ins.m_WebURL);
                            UnityEngine.Debug.LogWarning($"Open WebURL:{RunTimeData.Ins.m_WebURL}");
                        }
                        RunTimeData.Ins.m_WebUISetting.RefreshModels().Forget();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }).Forget();
        }
    }
}