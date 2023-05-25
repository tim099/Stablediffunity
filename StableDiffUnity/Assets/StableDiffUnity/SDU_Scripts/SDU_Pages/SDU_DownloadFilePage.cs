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
            string aDownloadFileSettingPath = RunTimeData.InstallSetting.DownloadLoraSettingsPath;
            if (!Directory.Exists(aDownloadFileSettingPath))
            {
                Directory.CreateDirectory(aDownloadFileSettingPath);
            }

            using (var aScope = new GUILayout.VerticalScope("box"))
            {
                UCL.Core.UI.UCL_GUILayout.DrawObjectData(m_DownloadFileSetting, m_Dic.GetSubDic("DownloadFileSetting"));
                if (GUILayout.Button("Save Setting"))
                {
                    JsonData aJson = JsonConvert.SaveDataToJsonUnityVer(m_DownloadFileSetting);
                    File.WriteAllText(Path.Combine(aDownloadFileSettingPath, $"{m_DownloadFileSetting.m_FileName}.json"),
                        aJson.ToJsonBeautify());
                }
                using (var aScope2 = new GUILayout.HorizontalScope())
                {
                    var aFiles = UCL.Core.FileLib.Lib.GetFilesName(aDownloadFileSettingPath, "*.json", SearchOption.TopDirectoryOnly);
                    if (!aFiles.IsNullOrEmpty())
                    {
                        if (!string.IsNullOrEmpty(m_LoadSettingName))
                        {
                            var aPath = Path.Combine(aDownloadFileSettingPath, m_LoadSettingName);
                            if (File.Exists(aPath))
                            {
                                if (GUILayout.Button("Load Setting"))
                                {
                                    var aJsonStr = File.ReadAllText(aPath);
                                    JsonData aJson = JsonData.ParseJson(aJsonStr);
                                    m_DownloadFileSetting = JsonConvert.LoadDataFromJsonUnityVer<SDU_DownloadFileSetting>(aJson);
                                }
                            }
                        }
                        m_LoadSettingName = UCL.Core.UI.UCL_GUILayout.PopupAuto(m_LoadSettingName, aFiles, m_Dic.GetSubDic("LoadSettingName"), "PopupAuto");
                    }

                }
            }
        }

    }
}