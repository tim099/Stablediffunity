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
        public static string s_ProcessName = "python";
        public static bool ProcessStarted => s_PidList.Count > 0;
        public static List<int> s_PidList = new List<int>();
        public static void KillAllProcess()
        {
            try
            {
                //Process[] Processes = Process.GetProcessesByName(s_ProcessName);

                //foreach (Process aProcess in Processes)
                //{
                //    if (s_PidList.Contains(aProcess.Id))
                //    {
                //        UnityEngine.Debug.LogWarning($"KillProcess Id:{aProcess.Id},ProcessName:{aProcess.ProcessName}");
                //        if (!aProcess.HasExited)
                //        {
                //            //aProcess.CloseMainWindow();
                //            aProcess.Kill();
                //            aProcess.WaitForExit();
                //        }
                //        else
                //        {
                //            UnityEngine.Debug.LogError($"KillProcess Process.HasExited, Id:{aProcess.Id},ProcessName:{aProcess.ProcessName}");
                //        }
                //        s_PidList.Remove(aProcess.Id);
                //    }
                //}

                while (s_PidList.Count > 0)
                {
                    var aId = s_PidList[0];
                    s_PidList.RemoveAt(0);
                    var aProcess = Process.GetProcessById(aId);
                    if (aProcess == null) continue;
                    UnityEngine.Debug.LogWarning($"KillProcess Id:{aProcess.Id},ProcessName:{aProcess.ProcessName}");
                    if (!aProcess.HasExited)
                    {
                        //aProcess.CloseMainWindow();
                        aProcess.Kill();
                        aProcess.WaitForExit();
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"KillProcess Process.HasExited, Id:{aProcess.Id},ProcessName:{aProcess.ProcessName}");
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            //DoActionForListedProcess((iProcess) =>
            //{
            //    if (!iProcess.HasExited)
            //    {
            //        UnityEngine.Debug.LogWarning($"KillProcess iProcess iID:{iProcess.Id}");
            //        iProcess.Kill();
            //        iProcess.WaitForExit();
            //    }
            //},(iID)=> {
            //    UnityEngine.Debug.LogError($"KillProcess not found iID:{iID}");
            //    s_PidList.Remove(iID);
            //});
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
            //Process[] Processes = Process.GetProcessesByName(s_ProcessName);
            //foreach (Process process in Processes)
            //{
            //    if (process.Id == iID)
            //    {
            //        return process.HasExited;
            //    }
            //}
            return false;//Process Not End
        }
        static List<int> s_PreCheckProcess = new List<int>();
        public static void PreCheckProcessEvent()
        {
            Process[] aProcesses = Process.GetProcesses();
            s_PreCheckProcess.Clear();
            foreach (var aProcess in aProcesses)
            {
                int aPid = aProcess.Id;
                s_PreCheckProcess.Add(aPid);
            }
        }
        static List<string> s_TargetProcessName = new List<string>() { "python", "cmd" };
        public static void CheckProcessEvent()
        {
            
            foreach(var aProcessName in s_TargetProcessName)
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

            s_PreCheckProcess.Clear();
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
            UnityEngine.Debug.LogWarning($"AddProcessEvent ,Id:{iProcess.Id})");//ProcessName:{iProcess.ProcessName}
            s_PidList.Add(iProcess.Id);
            iProcess.EnableRaisingEvents = true;

            iProcess.Exited -= OnExited;
            iProcess.Exited += OnExited;
        }
        private static void OnExited(object sender, EventArgs e)
        {
            var process = (System.Diagnostics.Process)sender;

            if (s_PidList.Contains(process.Id))
            {
                s_PidList.Remove(process.Id);
            }

            UnityEngine.Debug.LogWarning($"OnExited (process.Id:{process.Id})");

            //_webUIStatus.Close();
            //_webUIStatus.ResetServerArgs();
        }

        #region ProcessEvent

        private static bool DoActionForListedProcess(Action<Process> processFound, Action<int> processNotFound)
        {
            bool foundAtLeastOne = false;

            Span<int> buffer = stackalloc int[s_PidList.Count];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = s_PidList[i];
            }

            Process[] Processes = Process.GetProcessesByName(s_ProcessName);

            foreach (var id in buffer)
            {
                bool found = false;

                foreach (Process process in Processes)
                {
                    if (process.Id == id)
                    {
                        if (processFound != null) { processFound(process); }

                        found = true;
                        foundAtLeastOne = true;
                        continue;
                    }
                }

                if (!found)
                {
                    if (processNotFound != null) { processNotFound(id); }
                }
            }

            return foundAtLeastOne;
        }

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

        public static bool RestoreEventsForListedProcess()
        {
            return DoActionForListedProcess(RegisterEvents, x => s_PidList.Remove(x));
        }

        private static void RegisterEvents(Process process)
        {
            AddProcessEvent(process);

            SDU_WebUIStatus.Ins.SetServerStarted(true);
            SDU_WebUIStatus.Ins.ValidateConnectionContinuously().Forget();
        }

        #endregion
    }
}