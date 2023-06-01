using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;
namespace SDU
{
    public static class SDU_Server
    {
        /// <summary>
        /// AutoCheck every 30 Seconds
        /// </summary>
        public const float AutoCheckServerInterval = 30.0f;

        public static bool s_CheckingServerStarted = false;
        public static System.DateTime m_PrevCheckServerTime = DateTime.MinValue;
        
        public static string ServerAppId => s_ServerAppId;
        public static bool ServerReady
        {
            get => s_ServerReady;
            set
            {
                //Debug.LogError($"Set ServerReady:{s_ServerReady}");
                s_ServerReady = value;
            }
        }
        public static bool s_ServerReady = false;

        private static string s_ServerAppId;
        private static bool s_CheckEnabled = false;

        public static void OnGUI(UCL.Core.UCL_ObjectDictionary iDic)
        {
            if (!s_CheckingServerStarted)
            {
                if ((System.DateTime.Now - m_PrevCheckServerTime).TotalSeconds > AutoCheckServerInterval)
                {
                    CheckServerStarted(!ServerReady);
                }
                if (GUILayout.Button("Check Sevrver Started", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    CheckServerStarted();
                }
            }

        }
        public static int StartServer()
        {
            int aProcessID = -1;
            var aInstallSetting = RunTimeData.InstallSetting;
            SDU_FileInstall.CheckAndInstall(aInstallSetting);

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

            File.WriteAllText(aInstallSetting.PythonInstallPathFilePath, aInstallSetting.PythonInstallRoot);
            File.WriteAllText(aInstallSetting.WebUIInstallPathFilePath, aInstallSetting.WebUIInstallRoot);
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

            ValidateConnectionContinuously((iServerReady) =>
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

        public static async UniTask<bool> CheckServerReady(System.Action<bool> iEndAct = null)
        {
            //ServerReady = false;
            try
            {
                ServerReady = await ValidateConnection();
            }
            finally
            {
                iEndAct?.Invoke(ServerReady);
                //Close();
            }
            return ServerReady;
        }
        public static async UniTask<bool> ValidateConnection()
        {
            try
            {
                using (var aClient = RunTimeData.SD_API.Client_AppID)
                {
                    var aResultStr = await aClient.SendWebRequestStringAsync();
                    var aResult = JsonData.ParseJson(aResultStr);
                    if (aResult.Contains("app_id"))
                    {
                        var aResultAppID = aResult["app_id"];
                        ulong aAppID = aResultAppID.GetULong();
                        s_ServerAppId = aAppID.ToString();
                        //Debug.LogError($"{aResultAppID.JsonType},m_Obj:{aResultAppID.Obj.GetType().Name},aAppID:{aAppID}");
                    }
                    else
                    {
                        s_ServerAppId = string.Empty;
                    }
                    Debug.LogWarning($"ValidateConnection Result:{aResultStr}");
                }
                UnityEngine.Debug.LogWarning($"Check done. ServerAppId:{s_ServerAppId}");
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"Checking ... {e.Message}");
                //UnityEngine.Debug.LogException(e);
                return false;
            }
        }
        public static async UniTask ValidateConnectionContinuously(System.Action<bool> iEndAct = null)
        {
            if (s_CheckEnabled)
            {
                UnityEngine.Debug.LogWarning($"Check already active.");
                return;
            }

            s_CheckEnabled = true;

            try
            {
                while (s_CheckEnabled)
                {
                    ServerReady = await ValidateConnection();
                    if (ServerReady)
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                s_CheckEnabled = false;
                iEndAct?.Invoke(ServerReady);
            }
        }
        public static void Close()
        {
            //Debug.LogError("SDU_WebUIStatus Close()");
            s_CheckEnabled = false;
            s_ServerReady = false;
        }
        public static void CheckServerStarted(bool iRefreshModels = true)
        {
            if (s_CheckingServerStarted)
            {
                return;
            }
            m_PrevCheckServerTime = System.DateTime.Now;
            s_CheckingServerStarted = true;
            CheckServerReady((iServerReady) => {
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
                        if (iRefreshModels)
                        {
                            RunTimeData.Ins.m_WebUISetting.RefreshModels().Forget();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                finally
                {
                    s_CheckingServerStarted = false;
                }
            }).Forget();
        }
    }
}