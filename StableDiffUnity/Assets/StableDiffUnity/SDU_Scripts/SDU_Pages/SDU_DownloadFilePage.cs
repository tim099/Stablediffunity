using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UCL.Core.JsonLib;
using UCL.Core.EditorLib.Page;
using System.IO;
using UCL.Core.UI;

namespace SDU
{
    public class SDU_DownloadFilePage : UCL_EditorPage
    {
        #region static
        static public SDU_DownloadFilePage Create() => UCL_EditorPage.Create<SDU_DownloadFilePage>();
        #endregion
        protected override bool ShowCloseButton => false;
        public override string WindowName => $"SDU DownloadFile {SDU_EditorMenuPage.SDU_Version}";
        UCL.Core.UCL_ObjectDictionary m_Dic = new UCL.Core.UCL_ObjectDictionary();
        SDU_DownloadFileSetting DownloadFileSetting => RunTimeData.Ins.m_HideOnGUIData.m_DownloadFileSetting;
        public override void Init(UCL_GUIPageController iGUIPageController)
        {
            base.Init(iGUIPageController);
            SDU_FileInstall.CheckAndInstall(RunTimeData.InstallSetting);
        }
        protected override void ContentOnGUI()
        {
#if UNITY_EDITOR
            if(GUILayout.Button("Save InstallSettings to StreamingAssets"))
            {
                string aPath = Path.Combine(RunTimeData.InstallSetting.EnvInstallRoot, "InstallSettings");
                SDU_FileInstall.SaveInstallEnvToStreammingAssets(aPath);
                //Application.streamingAssetsPath
            }
            if (GUILayout.Button("Load InstallSettings from StreamingAssets"))
            {
                string aPath = Path.Combine(RunTimeData.InstallSetting.EnvInstallRoot, "InstallSettings");
                SDU_FileInstall.LoadInstallEnvFromStreammingAssets(aPath);
                //Application.streamingAssetsPath
            }
#endif

            using (var aScope = new GUILayout.VerticalScope("box"))
            {
                UCL.Core.UI.UCL_GUILayout.DrawObjectData(DownloadFileSetting, m_Dic.GetSubDic("DownloadFileSetting"));
            }
            foreach(var aKey in SDU_FileDownloader.DownloadingFiles.Keys)
            {
                var aFileHandle = SDU_FileDownloader.DownloadingFiles[aKey];
                aFileHandle.OnGUI(m_Dic.GetSubDic($"DownloadingFiles_{aKey}"));
            }

            var aSDUWebUIRequiredExtensions = SDU_FileInstall.SDU_WebUIRequiredExtensions.Ins;
            UCL.Core.UI.UCL_GUILayout.DrawObjectData(aSDUWebUIRequiredExtensions, m_Dic.GetSubDic("SDU_WebUIRequiredExtensions"));
            //aSDUWebUIRequiredExtensions.OnGUI(m_Dic.GetSubDic("SDU_WebUIRequiredExtensions"));
        }

    }
}