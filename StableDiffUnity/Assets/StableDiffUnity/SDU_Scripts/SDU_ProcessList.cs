/*
AutoHeader Test
to change the auto header please go to RCG_AutoHeader.cs
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


namespace SDU
{
    public static class SDU_ProcessList
    {
        public static System.Action<string> s_OnOutputDataReceivedAct = null;
        public static System.Text.StringBuilder s_ServerOutput = new();
        public static int s_ProcessID = -1;
        public static bool ProcessStarted => s_PidList.Count > 0;
        public static List<int> s_PidList = new List<int>();
        public static void KillAllProcess()
        {
            try
            {
                if (s_IsPreCheckProcess)
                {
                    CheckProcessEvent();
                }
                while (s_PidList.Count > 0)
                {
                    try
                    {
                        var aId = s_PidList[0];
                        s_PidList.RemoveAt(0);
                        var aProcess = Process.GetProcessById(aId);
                        if (aProcess == null) continue;
                        if (!aProcess.HasExited)
                        {
                            //aProcess.CloseMainWindow();
                            UnityEngine.Debug.LogWarning($"KillProcess Id:{aProcess.Id},ProcessName:{aProcess.ProcessName}");
                            aProcess.Kill();
                            aProcess.WaitForExit();
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"KillProcess Process.HasExited, Id:{aId}");
                        }
                    }
                    catch(Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            finally
            {
                s_ProcessID = -1;
            }

        }
        public static bool CheckProcessEnd(int iID)
        {
            if (s_PidList.Contains(iID))
            {
                return true;
            }
            var aProcess = Process.GetProcessById(iID);
            if(aProcess == null) return true;//Process Not found
            if(aProcess.HasExited) return true;//Process HasExited

            return false;//Process Not End
        }
        private static bool s_IsPreCheckProcess = false;
        private static List<int> s_PreCheckProcess = new List<int>();
        private static List<string> s_TargetProcessName = new List<string>() { "python", "cmd" };
        public static void PreCheckProcessEvent()
        {
            if (s_IsPreCheckProcess)
            {
                UnityEngine.Debug.LogError("PreCheckProcessEvent() s_IsPreCheckProcess");
                return;
            }
            s_IsPreCheckProcess = true;
            Process[] aProcesses = Process.GetProcesses();
            s_PreCheckProcess.Clear();
            foreach (var aProcess in aProcesses)
            {
                int aPid = aProcess.Id;
                s_PreCheckProcess.Add(aPid);
            }
        }
        
        public static void CheckProcessEvent()
        {
            if (!s_IsPreCheckProcess)
            {
                UnityEngine.Debug.LogError("CheckProcessEvent() !s_IsPreCheckProcess");
                return;
            }
            try
            {
                foreach (var aProcessName in s_TargetProcessName)
                {
                    Process[] aProcesses = Process.GetProcessesByName(aProcessName); //Process.GetProcesses();
                    foreach (var aProcess in aProcesses)
                    {
                        int aPid = aProcess.Id;
                        if (!aProcess.HasExited && !s_PidList.Contains(aPid))
                        {
                            if (!s_PreCheckProcess.Contains(aPid))
                            {
                                //UnityEngine.Debug.LogWarning($"ProcessName:{aProcess.ProcessName},aPid:{aPid}");
                                AddProcessEvent(aProcess);
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            finally
            {
                s_IsPreCheckProcess = false;
                s_PreCheckProcess.Clear();
            }
        }
        public static void AddProcessEvent(System.Diagnostics.Process iProcess)
        {
            if(iProcess == null)
            {
                UnityEngine.Debug.LogError("AddProcessEvent iProcess == null");
                return;
            }
            if (iProcess.HasExited)
            {
                UnityEngine.Debug.LogError("AddProcessEvent iProcess.HasExited");
                return;
            }
            try
            {
                UnityEngine.Debug.LogWarning($"AddProcessEvent ,Id:{iProcess.Id}");
                //ProcessName:{iProcess.ProcessName}
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
           
            s_PidList.Add(iProcess.Id);
            iProcess.EnableRaisingEvents = true;

            iProcess.Exited -= OnExited;
            iProcess.Exited += OnExited;
        }
        private static void OnExited(object sender, EventArgs e)
        {
            var process = (System.Diagnostics.Process)sender;
            UnityEngine.Debug.LogWarning($"OnExited (process.Id:{process.Id})");

            if (s_PidList.IsNullOrEmpty())
            {
                return;
            }

            if (s_PidList.Contains(process.Id))
            {
                s_PidList.Remove(process.Id);
            }
            if (s_PidList.IsNullOrEmpty())
            {
                SDU_Server.Close();
            }
        }

        #region ProcessEvent

        public static void Init(Process iProcess)
        {
            iProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.GetEncoding("shift_jis");
            iProcess.OutputDataReceived -= OnOutputDataReceived;
            iProcess.OutputDataReceived += OnOutputDataReceived;
            s_ServerOutput.Clear();
        }
        private static void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string aData = e.Data;
            if (!string.IsNullOrEmpty(aData))
            {
                s_ServerOutput.Append($"{aData}\n");
                s_OnOutputDataReceivedAct?.Invoke(aData);
            }
        }
        #endregion
    }
}