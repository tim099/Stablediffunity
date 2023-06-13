using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
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

        public enum ServerState
        {
            Off,
            Starting,
            Ready,
        }

        public static bool s_CheckingServerStarted = false;
        public static ServerState s_ServerState = ServerState.Off;
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
        public static bool IsCancelStartServer => s_CancellationTokenSource == null || s_CancellationTokenSource.IsCancellationRequested;
        private static CancellationTokenSource s_CancellationTokenSource = null;

        private static string s_ServerAppId;

        public static void OnGUI(UCL.Core.UCL_ObjectDictionary iDic)
        {
            if (!s_CheckingServerStarted)
            {
                if ((System.DateTime.Now - m_PrevCheckServerTime).TotalSeconds > AutoCheckServerInterval)
                {
                    CheckServerStarted(!ServerReady);
                }
            }
            if (!SDU_ProcessList.ProcessStarted)
            {
                if (GUILayout.Button("Start Server", UCL_GUIStyle.GetButtonStyle(Color.white), GUILayout.Width(160)))//GUILayout.ExpandWidth(false)
                {
                    StartServer().Forget();
                }
            }
            else
            {
                if (GUILayout.Button("Stop Server", UCL_GUIStyle.GetButtonStyle(Color.yellow), GUILayout.Width(160)))
                {
                    UnityEngine.Debug.Log($"Stop server. ProcessID:{SDU_ProcessList.s_ProcessID}");
                    SDU_Server.StopServer();
                }
            }

            string aServerStateStr = string.Empty;
            if (ServerReady)
            {
                aServerStateStr = "Server Ready.".RichTextColor(Color.green);
            }
            else if (s_ServerState == ServerState.Starting)
            {
                aServerStateStr = "Server starting up...".RichTextColor(Color.cyan);
            }
            else if (s_CheckingServerStarted)
            {
                aServerStateStr = "Checking Server Started.".RichTextColor(Color.white);
            }
            else
            {
                aServerStateStr = "Server Not Started!!".RichTextColor(Color.yellow);
            }
            GUILayout.Label($"{aServerStateStr}", UCL_GUIStyle.LabelStyle);

            if (GUILayout.Button("Check Server", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
            {
                if (!s_CheckingServerStarted) CheckServerStarted();
            }

        }
        public static void CancelStartServer()
        {
            if (s_CancellationTokenSource == null) return;
            if (!s_CancellationTokenSource.IsCancellationRequested)
            {
                s_CancellationTokenSource.Cancel();
            }
            
            s_CancellationTokenSource.Dispose();
            s_CancellationTokenSource = null;
        }
        public static async UniTask StartServer()
        {
            CancelStartServer();
            s_CancellationTokenSource = new CancellationTokenSource();
            int aProcessID = -1;
            SDU_ProcessList.s_ProcessID = aProcessID;
            var aInstallSetting = RunTimeData.InstallSetting;
            SDU_FileInstall.CheckAndInstall(aInstallSetting);

            var aPythonExePath = aInstallSetting.PythonExePath;//System.IO.Path.Combine(aEnvInstallRoot, Data.PythonExePath);
            UnityEngine.Debug.LogWarning($"PythonExePath:{aPythonExePath}");
            if (!System.IO.File.Exists(aPythonExePath))
            {
                Debug.LogError($"File not found, aPythonExePath:{aPythonExePath}, try reinstall");
                return;
            }
            string aBatPath = RunTimeData.InstallSetting.RunBatPath;//System.IO.Path.Combine(aEnvInstallRoot, Data.WebUIScriptBatPath);
            if (!File.Exists(aBatPath))
            {
                Debug.LogError($"File not found, aBatPath:{aBatPath}, try reinstall");
                return;
            }

            File.WriteAllText(aInstallSetting.PythonInstallPathFilePath, aInstallSetting.PythonInstallRoot);
            File.WriteAllText(aInstallSetting.WebUIInstallPathFilePath, aInstallSetting.WebUIInstallRoot);
            File.WriteAllText(aInstallSetting.CommandlindArgsFilePath, aInstallSetting.GetCommandlindArgs);
            File.WriteAllText(aInstallSetting.RunBatFilePath, aBatPath);
            SDU_ProcessList.PreCheckProcessEvent();//check current exist process
            var aProcess = new System.Diagnostics.Process();

            string aRunPythonPath = aInstallSetting.RunPythonPath;

            UnityEngine.Debug.LogWarning($"RunPythonPath:{aRunPythonPath},BatPath:{aBatPath}");

            aProcess.StartInfo.FileName = aPythonExePath;
            string aArguments = string.Empty;
            if (!string.IsNullOrEmpty(aInstallSetting.PythonArgs))
            {
                aArguments = $"{aInstallSetting.PythonArgs} {aRunPythonPath}";
            }
            else
            {
                aArguments = aRunPythonPath;
            }
            Debug.LogWarning($"Process Arguments:{aArguments}");
            aProcess.StartInfo.Arguments = aArguments;// {aBatPath}
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
            SDU_ProcessList.s_ProcessID = aProcessID;
            UnityEngine.Debug.LogWarning($"Start server, ProcessID:{aProcessID}");
            s_ServerState = ServerState.Starting;
            try
            {
                while (!ServerReady && !IsCancelStartServer)
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


            try
            {
                UnityEngine.Debug.LogWarning($"ValidateConnectionContinuously End, ServerReady:{ServerReady}");
                SDU_ProcessList.CheckProcessEvent();

                if (!ServerReady)
                {
                    s_ServerState = ServerState.Off;
                    UnityEngine.Debug.LogError($"StartServer() fail, !ServerReady");
                    return;
                }
                
                bool aIsInstall = await SDU_FileInstall.SDU_WebUIRequiredExtensions.Ins.
                    CheckAndInstallRequiredExtensions(RunTimeData.InstallSetting);
                if (aIsInstall)//Need to restart Server
                {
                    StopServer();
                    await Task.Delay(1);
                    UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                    {
                        StartServer().Forget();
                    });
                    return;
                }

                s_ServerState = ServerState.Ready;
                //aProcess.StandardOutput.ReadToEnd();
                if (RunTimeData.Ins.m_AutoOpenWeb)
                {
                    System.Diagnostics.Process.Start(RunTimeData.Ins.m_WebURL);
                    UnityEngine.Debug.LogWarning($"Open WebURL:{RunTimeData.Ins.m_WebURL}");
                }
                RunTimeData.Ins.m_WebUISetting.RefreshModels().Forget();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

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
        public static void StopServer()
        {
            Close();
            SDU_ProcessList.KillAllProcess();
        }
        public static void Close()
        {
            CancelStartServer();
            //Debug.LogError("SDU_WebUIStatus Close()");
            s_ServerState = ServerState.Off;
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