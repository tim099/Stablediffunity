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
using System.Threading;

namespace SDU
{
    //[UCL.Core.ATTR.RequiresConstantRepaint]
    public class SDU_StableDiffusionPage : UCL_EditorPage
    {
        #region static
        static public SDU_StableDiffusionPage Create() => UCL_EditorPage.Create<SDU_StableDiffusionPage>();

        #endregion
        public override bool IsWindow => true;
        public override string WindowName => $"StableDiffUnity GUI {SDU_EditorMenuPage.SDU_Version}";
        protected override bool ShowCloseButton => false;

        
        UCL.Core.UCL_ObjectDictionary m_DataDic = new UCL.Core.UCL_ObjectDictionary();

        ~SDU_StableDiffusionPage()
        {
            SDU_ImageGenerator.ClearTextures();
            RunTimeData.SaveRunTimeData();
        }
        public override void Init(UCL_GUIPageController iGUIPageController)
        {
            base.Init(iGUIPageController);
            RunTimeData.LoadRunTimeData();
            SDU_FileInstall.CheckAndInstall(RunTimeData.InstallSetting);
            SDU_Server.CheckServerStarted();
        }
        public override void OnClose()
        {
            SDU_ImageGenerator.ClearTextures();
            RunTimeData.SaveRunTimeData();
            //SDU_Server.Close();
            base.OnClose();
        }
        protected override void TopBar()
        {
            int aAction = 0;//0 none 1 back 2 close


            using (var aScope = new GUILayout.VerticalScope("box"))
            {
                using (var aScope2 = new GUILayout.HorizontalScope())
                {
                    if (ShowBackButton)
                    {
                        if (GUILayout.Button(UCL.Core.LocalizeLib.UCL_LocalizeManager.Get("Back"), GUILayout.ExpandWidth(false)))
                        {
                            aAction = 1;
                        }
                    }

                    TopBarButtons();
                }
                GUILayout.Space(5);
                if (SDU_Server.ServerReady)
                {
                    if (!SDU_CMDService.TriggeringCMD)
                    {
                        var aImgSetting = RunTimeData.Ins.CurImgSetting;
                        //GUILayout.Space(20);
                        if (GUILayout.Button($"[{RunTimeData.Ins.m_GenerateMode}] Generate Image ({aImgSetting.m_Width},{aImgSetting.m_Height})",
                            UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                        {
                            var aCMD = new SDU_CMDGenerateImage();
                            var aCMDs = new List<SDU_CMD>() { aCMD };
                            SDU_CMDService.TriggerCMDs(aImgSetting, aCMDs).Forget();
                        }

                        if (!aImgSetting.m_CMDs.IsNullOrEmpty())
                        {
                            //GUILayout.Space(20);
                            if (GUILayout.Button($"Trigger CMDs ({aImgSetting.m_CMDs.Count})", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                            {
                                var aCMDs = new List<SDU_CMD>();
                                foreach (var aCMD in aImgSetting.m_CMDs)
                                {
                                    aCMDs.Append(aCMD.GetCMDList());
                                }
                                UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                                {
                                    SDU_CMDService.TriggerCMDs(aImgSetting, aCMDs).Forget();
                                });
                            }
                        }
                    }
                    
                    SDU_ImageGenerator.OnGUI(m_DataDic.GetSubDic("SDU_ImageGenerator"));
                    SDU_CMDService.OnGUI(m_DataDic.GetSubDic("SDU_CMDService"), true);
                }
            }



            switch (aAction)
            {
                case 1:
                    {
                        BackButtonClicked();
                        break;
                    }
                case 2:
                    {
                        CloseButtonClicked();
                        break;
                    }
            }
        }
        protected override void TopBarButtons()
        {
            using (var aScope = new GUILayout.HorizontalScope())
            {
                //GUILayout.Space(30);
                GUILayout.Label($"[{System.DateTime.Now.ToString("HH:mm:ss")}]", UCL_GUIStyle.LabelStyle,GUILayout.Width(80));

                SDU_Server.OnGUI(m_DataDic.GetSubDic("SDU_Server"));
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label($"[{Application.systemLanguage}]", UCL_GUIStyle.LabelStyle, GUILayout.Width(120));
            if (GUILayout.Button("Download File"))
            {
                SDU_DownloadFilePage.Create();
            }
            if (GUILayout.Button("Debug Log", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
            {
                UCL_DebugLogPage.Create();
            }
        }
        protected override void ContentOnGUI()
        {
            RunTimeData.ConfigOnGUI(m_DataDic.GetSubDic("RunTimeData"));

            UCL.Core.UI.UCL_GUILayout.DrawObjectData(RunTimeData.Ins, m_DataDic.GetSubDic("RunTimeData"), "Configs", false);
            var aSceneControl = SDU_SceneControl.Ins;
            if(aSceneControl != null)
            {
                try
                {
                    aSceneControl.ContentOnGUI(m_DataDic.GetSubDic("SceneControl"));
                }
                catch(Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            var aTextures = SDU_ImageGenerator.s_Textures;
            if (!aTextures.IsNullOrEmpty())
            {
                var aTexSize = SDU_Util.GetTextureSize(512, aTextures[0]);
                var aDataDic = m_DataDic.GetSubDic("DataDic");
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