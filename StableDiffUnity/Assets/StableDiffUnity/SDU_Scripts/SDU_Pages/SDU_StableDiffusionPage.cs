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
using Cysharp.Threading.Tasks;
namespace SDU
{
    //[UCL.Core.ATTR.RequiresConstantRepaint]
    public class SDU_StableDiffusionPage : UCL_EditorPage
    {
        const string ConfigFilePathKey = "StableDiffusionPage.ConfigFilePath";

        #region static
        static public SDU_StableDiffusionPage Create() => UCL_EditorPage.Create<SDU_StableDiffusionPage>();

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
        
        #endregion
        public override bool IsWindow => true;
        public override string WindowName => $"StableDiffUnity GUI {SDU_EditorMenuPage.SDU_Version}";
        protected override bool ShowCloseButton => false;

        
        UCL.Core.UCL_ObjectDictionary m_Dic = new UCL.Core.UCL_ObjectDictionary();
        int m_ProcessID = -1;

        ~SDU_StableDiffusionPage()
        {
            SDU_ImageGenerator.ClearTextures();
            RunTimeData.SaveRunTimeData();
        }
        public override void Init(UCL_GUIPageController iGUIPageController)
        {
            base.Init(iGUIPageController);
            RunTimeData.LoadRunTimeData();
            SDU_Server.CheckServerStarted();
        }
        public override void OnClose()
        {
            SDU_ImageGenerator.ClearTextures();
            RunTimeData.SaveRunTimeData();
            SDU_WebUIStatus.Ins.Close();
            base.OnClose();
        }
        protected override void ContentOnGUI()
        {
            using(var aScope = new GUILayout.HorizontalScope())
            {
                SDU_Server.OnGUI(m_Dic.GetSubDic("SDU_Server"));

                string aServerStateStr = string.Empty;
                if (SDU_WebUIStatus.ServerReady)
                {
                    aServerStateStr = "Server Ready.".RichTextColor(Color.green);
                }
                else if (SDU_Server.s_CheckingServerStarted)
                {
                    aServerStateStr = "Checking Server Started.".RichTextColor(Color.cyan);
                }
                else
                {
                    aServerStateStr = "Server Not found!!Please Start Server.".RichTextColor(Color.yellow);
                }
                GUILayout.Label($"{aServerStateStr} Time:{System.DateTime.Now.ToString("HH:mm:ss")}", UCL_GUIStyle.LabelStyle);
            }

            if(!SDU_ProcessList.ProcessStarted)
            {
                if (GUILayout.Button("Start Server", UCL_GUIStyle.ButtonStyle))
                {
                    m_ProcessID = SDU_Server.StartServer();
                }
            }
            else
            {
                if (GUILayout.Button("Stop Server", UCL_GUIStyle.ButtonStyle))
                {
                    UnityEngine.Debug.Log($"Stop server. m_ProcessID:{m_ProcessID}");
                    SDU_WebUIStatus.Ins.Close();
                    SDU_ProcessList.KillAllProcess();
                    SDU_WebUIStatus.ServerReady = false;
                    m_ProcessID = -1;
                }
                //if (SDU_WebUIStatus.ServerReady)
                //{
                //    if (GUILayout.Button("Refresh Models", UCL_GUIStyle.ButtonStyle))
                //    {
                //        RunTimeData.Ins.m_WebUISetting.RefreshModels().Forget();
                //    }
                //}
            }

            if (GUILayout.Button("Download File"))
            {
                SDU_DownloadFilePage.Create();
            }
            using (var aScope = new GUILayout.HorizontalScope("box"))
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

            if (!UnityChan.IdleChanger.s_IdleChangers.IsNullOrEmpty())
            {
                GUILayout.Space(20);
                GUILayout.Box($"Change UnityChan Motion");
                for (int i = 0; i < UnityChan.IdleChanger.s_IdleChangers.Count; i++)
                {
                    var aIdleChanger = UnityChan.IdleChanger.s_IdleChangers[i];
                    
                    aIdleChanger.CustomOnGUI(i);
                }
            }
            var aTextures = SDU_ImageGenerator.s_Textures;
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
    }
}