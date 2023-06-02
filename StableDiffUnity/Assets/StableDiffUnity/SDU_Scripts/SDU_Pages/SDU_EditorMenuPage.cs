using System.Collections;
using System.Collections.Generic;
using UCL.Core.EditorLib.Page;
using UCL.Core.Game;
using UCL.Core.LocalizeLib;
using UnityEngine;



namespace SDU
{
    public class SDU_EditorMenuPage : UCL_EditorPage
    {
        public static string SDU_Version => Application.version;
        public override string WindowName => $"SDU_EditorMenu {SDU_Version}";

        protected override bool ShowCloseButton => false;
        protected override bool ShowBackButton => false;


        UCL.Core.UCL_ObjectDictionary m_Dic = new UCL.Core.UCL_ObjectDictionary();
        protected override void TopBar()
        {
            //base.TopBar();
        }
        //UCL.Core.UCL_ObjectDictionary m_Dic = new UCL.Core.UCL_ObjectDictionary();

        /// <summary>
        /// Draw Editor Munu
        /// </summary>
        protected override void ContentOnGUI()
        {
            using (var aScope = new GUILayout.VerticalScope("box"))//, GUILayout.MaxWidth(320)
            {
                RunTimeData.ConfigOnGUI(m_Dic.GetSubDic("RunTimeData"));
                GUILayout.Space(10);
                UCL.Core.UI.UCL_GUILayout.DrawObjectData(RunTimeData.Ins.m_InstallSetting, m_Dic.GetSubDic("InstallSetting"), "InstallSetting", false);
                GUILayout.Space(10);
                if (GUILayout.Button("Stable Diffusion", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                {
                    SDU_StableDiffusionPage.Create();
                }

                GUILayout.Space(10);
                if (GUILayout.Button("Download File"))
                {
                    SDU_DownloadFilePage.Create();
                }

                GUILayout.Space(10);
                if (GUILayout.Button("Compress Image"))
                {
                    SDU_CompressImagePage.Create();
                }
#if !UNITY_EDITOR
                GUILayout.Space(30);
                if (GUILayout.Button("Exit SDU"))
                {
                    UCL_GameManager.Instance.ExitGame();
                }
#endif
            }
        }
    }
}