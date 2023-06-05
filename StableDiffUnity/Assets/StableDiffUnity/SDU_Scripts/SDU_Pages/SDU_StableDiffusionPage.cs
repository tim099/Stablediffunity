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
        #region static
        static public SDU_StableDiffusionPage Create() => UCL_EditorPage.Create<SDU_StableDiffusionPage>();

        #endregion
        public override bool IsWindow => true;
        public override string WindowName => $"StableDiffUnity GUI {SDU_EditorMenuPage.SDU_Version}";
        protected override bool ShowCloseButton => false;

        
        UCL.Core.UCL_ObjectDictionary m_Dic = new UCL.Core.UCL_ObjectDictionary();

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
        protected override void TopBarButtons()
        {
            using (var aScope = new GUILayout.HorizontalScope())
            {
                //GUILayout.Space(30);
                GUILayout.Label($"[{System.DateTime.Now.ToString("HH:mm:ss")}]", UCL_GUIStyle.LabelStyle,GUILayout.Width(80));
                SDU_Server.OnGUI(m_Dic.GetSubDic("SDU_Server"));
            }
        }
        protected override void ContentOnGUI()
        {

            if (GUILayout.Button("Download File"))
            {
                SDU_DownloadFilePage.Create();
            }
            RunTimeData.ConfigOnGUI(m_Dic.GetSubDic("RunTimeData"));

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