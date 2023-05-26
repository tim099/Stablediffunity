using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UCL.Core.JsonLib;
using UCL.Core.EditorLib.Page;
using System.IO;
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
        string m_LoadSettingName;
        SDU_DownloadFileSetting m_DownloadFileSetting = new SDU_DownloadFileSetting();
        protected override void ContentOnGUI()
        {
            using (var aScope = new GUILayout.VerticalScope("box"))
            {
                UCL.Core.UI.UCL_GUILayout.DrawObjectData(m_DownloadFileSetting, m_Dic.GetSubDic("DownloadFileSetting"));
            }
            foreach(var aKey in SDU_FileDownloader.DownloadingFiles.Keys)
            {
                var aFileHandle = SDU_FileDownloader.DownloadingFiles[aKey];
                aFileHandle.OnGUI(m_Dic.GetSubDic($"DownloadingFiles_{aKey}"));
            }
        }

    }
}