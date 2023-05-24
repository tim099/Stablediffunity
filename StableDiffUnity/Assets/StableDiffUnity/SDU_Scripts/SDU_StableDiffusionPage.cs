/*
AutoHeader Test
to change the auto header please go to RCG_AutoHeader.cs
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core.UI;
using UnityEngine;
using System;
using System.Text;
using UCL.Core.EditorLib.Page;
using System.Text.RegularExpressions;
using UCL.Core.JsonLib;
using System.Linq;
using System.Threading.Tasks;
//using System.Diagnostics;

namespace SDU
{
    //[UCL.Core.ATTR.RequiresConstantRepaint]
    public class SDU_StableDiffusionPage : UCL_EditorPage
    {
        const string ConfigFilePathKey = "StableDiffusionPage.ConfigFilePath";

        #region static
        public static string ServerUrl => RunTimeData.Ins.m_WebURL;

        public static string ConfigFilePath
        {
            get
            {
                if (!PlayerPrefs.HasKey(ConfigFilePathKey))
                {
                    PlayerPrefs.SetString(ConfigFilePathKey, DefaultConfigFilePath);
                }
                return PlayerPrefs.GetString(ConfigFilePathKey);
            }
            set
            {
                PlayerPrefs.SetString(ConfigFilePathKey, value);
            }
        }
        
        public static string DefaultConfigFilePath => Path.Combine(InstallSetting.DefaultInstallRoot, "Configs", "StableDiffusion.json");
        static public SDU_StableDiffusionPage Create() => UCL_EditorPage.Create<SDU_StableDiffusionPage>();

        static public StableDiffusionAPI SD_API => RunTimeData.Ins.m_APISetting.m_StableDiffusionAPI;
        static public ControlNetAPI ControlNet_API => RunTimeData.Ins.m_APISetting.m_ControlNetAPI;
        

        #endregion
        public override bool IsWindow => true;
        public override string WindowName => $"StableDiffUnity GUI {SDU_EditorMenuPage.SDU_Version}";
        protected override bool ShowCloseButton => false;


        UCL.Core.UCL_ObjectDictionary m_Dic = new UCL.Core.UCL_ObjectDictionary();
        int m_ProcessID = -1;
        //System.DateTime m_CheckProcessEndTime = DateTime.MinValue;
        GenMode m_GenMode = GenMode.None;

        ~SDU_StableDiffusionPage()
        {
            SDU_ImageGenerator.ClearTextures();
            RunTimeData.SaveRunTimeData();
        }
        public override void Init(UCL_GUIPageController iGUIPageController)
        {
            base.Init(iGUIPageController);
            RunTimeData.LoadRunTimeData();
            RunTimeData.Ins.m_AutoGenMode = GenMode.None;
        }
        public override void OnClose()
        {
            SDU_ImageGenerator.ClearTextures();
            RunTimeData.SaveRunTimeData();
            SDU_WebUIStatus.Ins.Close();
            base.OnClose();
        }
        public async System.Threading.Tasks.ValueTask RefreshModels()
        {
            var aWebUISetting = RunTimeData.Ins.m_WebUISetting;
            await aWebUISetting.RefreshCheckpoints();
            await aWebUISetting.RefreshSamplers();
            await aWebUISetting.RefreshLora();
            await aWebUISetting.RefreshControlNetModels();
        }
        protected override void ContentOnGUI()
        {
            GUILayout.Label($"StableDiffusion Time:{System.DateTime.Now.ToString("HH:mm:ss.ff")}", UCL_GUIStyle.LabelStyle);
            if(!SDU_ProcessList.ProcessStarted)
            {
                if (GUILayout.Button("StartServer", UCL_GUIStyle.ButtonStyle))
                {
                    StartServer();
                }
            }
            else
            {
                if (GUILayout.Button("StopServer", UCL_GUIStyle.ButtonStyle))
                {
                    UnityEngine.Debug.Log($"Stop server. m_ProcessID:{m_ProcessID}");
                    if (!SDU_WebUIStatus.s_ServerReady)
                    {
                        SDU_ProcessList.CheckProcessEvent();
                    }
                    SDU_ProcessList.KillAllProcess();
                    m_ProcessID = -1;
                }
                if (SDU_WebUIStatus.s_ServerReady)
                {
                    if (GUILayout.Button("Refresh Models", UCL_GUIStyle.ButtonStyle))
                    {
                        RefreshModels().Forget();
                    }
                    if (!SDU_ImageGenerator.GeneratingImage)
                    {
                        m_GenMode = RunTimeData.Ins.m_AutoGenMode;
                        if (GUILayout.Button("GenerateImage", UCL_GUIStyle.ButtonStyle))
                        {
                            m_GenMode = GenMode.GenTex2Img;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(SDU_ImageGenerator.ProgressStr))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(SDU_ImageGenerator.ProgressStr, UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                if(SDU_ImageGenerator.ProgressVal > 0) GUILayout.HorizontalSlider(SDU_ImageGenerator.ProgressVal, 0f, 1f);
                GUILayout.EndHorizontal();
            }

            using(var aScope = new GUILayout.HorizontalScope("box"))
            {
                if (GUILayout.Button("Save", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    RunTimeData.SaveRunTimeData();
                }
                if (File.Exists(ConfigFilePath))
                {
                    if (GUILayout.Button("Load", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                    {
                        RunTimeData.ReloadRunTimeData();
                    }
                }

                var aConfigFilePath = ConfigFilePath;
                var aNewConfigFilePath = UCL_GUILayout.TextField("ConfigFilePath", aConfigFilePath);
                if (aNewConfigFilePath != aConfigFilePath)
                {
                    ConfigFilePath = aNewConfigFilePath;
                }
            }

            UCL.Core.UI.UCL_GUILayout.DrawObjectData(RunTimeData.Ins, m_Dic.GetSubDic("RunTimeData"), "Configs", false);
            
            RunTimeData.Ins.m_Tex2ImgSettings.OnGUI("Tex2Img", m_Dic.GetSubDic("Tex2ImgSettings"));
            //m_Tex2ImgSettings
            if (!SDU_ImageGenerator.GeneratingImage)
            {
                switch (m_GenMode)
                {
                    case GenMode.GenTex2Img:
                        {
                            SDU_ImageGenerator.GenerateImage(RunTimeData.Ins.m_Tex2ImgSettings);
                            break;
                        }
                }
            }

            if (!UnityChan.IdleChanger.s_IdleChangers.IsNullOrEmpty())
            {
                GUILayout.Box($"Change Motion");
                for (int i = 0; i < UnityChan.IdleChanger.s_IdleChangers.Count; i++)
                {
                    var aIdleChanger = UnityChan.IdleChanger.s_IdleChangers[i];
                    
                    aIdleChanger.CustomOnGUI(i);
                }
            }
            var aTextures = SDU_ImageGenerator.m_Textures;
            if (!aTextures.IsNullOrEmpty())
            {
                var aTexSize = SDU_Util.GetTextureSize(512, aTextures[0]);
                var aDataDic = m_Dic.GetSubDic("DataDic");
                Vector2 aScrollPos = aDataDic.GetData("ScrollPos", Vector2.zero);
                using (var aScrollScope = new GUILayout.ScrollViewScope(aScrollPos, GUILayout.Height(aTexSize.y + 32)))
                {
                    aDataDic.SetData("ScrollPos", aScrollScope.scrollPosition);
                    GUILayout.BeginHorizontal();
                    foreach (var aTexture in aTextures)
                    {
                        var aSize = SDU_Util.GetTextureSize(512, aTexture);
                        GUILayout.Box(aTexture, GUILayout.Width(aSize.x), GUILayout.Height(aSize.y));
                    }
                    GUILayout.EndHorizontal();
                }
            }
            //if (UnityChan.FaceUpdate.s_Ins != null)
            //{
            //    UnityChan.FaceUpdate.s_Ins.CustomOnGUI();
            //}

        }
        public void StartServer()
        {
            SDU_WebUIStatus.s_ServerReady = false;
            var aInstallSetting = RunTimeData.Ins.m_InstallSetting;
            var aPythonRoot = CheckInstall(aInstallSetting.PythonInstallRoot, aInstallSetting.PythonZipPath, "Python");
            var aEnvInstallRoot = CheckInstall(aInstallSetting.EnvInstallRoot, aInstallSetting.EnvZipPath, "Env");
            var aWebUIRoot = CheckInstall(aInstallSetting.WebUIInstallRoot, aInstallSetting.WebUIZipPath, "WebUI");
            File.WriteAllText(aInstallSetting.PythonInstallPathFilePath, aPythonRoot);
            File.WriteAllText(aInstallSetting.WebUIInstallPathFilePath, aWebUIRoot);
            File.WriteAllText(aInstallSetting.CommandlindArgsFilePath, aInstallSetting.CommandlindArgs);

            var aPythonExePath = aInstallSetting.PythonExePath;//System.IO.Path.Combine(aEnvInstallRoot, Data.PythonExePath);
            UnityEngine.Debug.LogWarning($"PythonExePath:{aPythonExePath}");
            if (!System.IO.File.Exists(aPythonExePath))
            {
                Debug.LogError($"File not found, path:{aPythonExePath}");
                return;
            }

            if (SDU_ProcessList.RestoreEventsForListedProcess())
            {
                UnityEngine.Debug.LogWarning($"Server alredy started.");
                return;
            }
            SDU_ProcessList.PreCheckProcessEvent();
            var aProcess = new System.Diagnostics.Process();
            
            //string aBatPath = System.IO.Path.Combine(Data.RootPath, Data.WebUIScriptBatPath);
            string aRunPythonPath = aInstallSetting.RunPythonPath;
            string aBatPath = RunTimeData.Ins.m_InstallSetting.RunBatPath;//System.IO.Path.Combine(aEnvInstallRoot, Data.WebUIScriptBatPath);
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
            m_ProcessID = aProcess.Id;
            UnityEngine.Debug.LogWarning($"Start server, m_ProcessID:{m_ProcessID}");

            SDU_WebUIStatus.Ins.SetServerStarted(true);
            SDU_WebUIStatus.Ins.ValidateConnectionContinuously(()=> {
                try
                {
                    UnityEngine.Debug.LogWarning($"ValidateConnectionContinuously End");
                    SDU_ProcessList.CheckProcessEvent();
                    //aProcess.StandardOutput.ReadToEnd();
                    if (RunTimeData.Ins.m_AutoOpenWeb)
                    {
                        System.Diagnostics.Process.Start(RunTimeData.Ins.m_WebURL);
                        UnityEngine.Debug.LogWarning($"Open WebURL:{RunTimeData.Ins.m_WebURL}");
                    }
                    RefreshModels().Forget();
                }
                catch(Exception ex)
                {
                    Debug.LogException(ex);
                }

            }).Forget();
        }
        private void OnOutputDataReceived(string iOutPut)
        {
            if (iOutPut.Contains("Running on local URL:"))
            {
                string aURL = iOutPut.Replace("Running on local URL:", string.Empty);
                RunTimeData.Ins.m_WebURL = Regex.Replace(aURL, @"\s", string.Empty);
                //System.Diagnostics.Process.Start(Data.m_WebURL);
                //UnityEngine.Debug.LogWarning($"Open WebURL:{Data.m_WebURL}");
            }
            else
            {
                //UnityEngine.Debug.LogWarning($"DataReceived_:{iOutPut}");
            }
            UnityEngine.Debug.LogWarning($"DataReceived_:{iOutPut}");

        }
        private string CheckInstall(string iInstallRoot, string iZipAbsolutePath, string iInstallTarget)
        {
            if (Directory.Exists(iInstallRoot))//Install done
            {
                return iInstallRoot;
            }
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
    }
}