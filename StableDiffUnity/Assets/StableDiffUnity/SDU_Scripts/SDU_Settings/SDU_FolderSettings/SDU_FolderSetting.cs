using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;


namespace SDU
{
    public static partial class SDUI_FolderSettingExtensions
    {
        public static string Path(this SDUI_FolderSetting iFolderSetting)
        {
            return RunTimeData.InstallSetting.GetFolderPath(iFolderSetting.Folder);
        }
        public static string GetDownloadSettingsFolderPath(this SDUI_FolderSetting iFolderSetting)
        {
            return RunTimeData.InstallSetting.GetDownloadSettingsFolderPath(iFolderSetting.Folder);
        }
        public static void OpenDownloadSettingsFolder(this SDUI_FolderSetting iFolderSetting)
        {
            InstallSetting.OpenFolder(iFolderSetting.GetDownloadSettingsFolderPath());
        }
    }
    public interface SDUI_FolderSetting
    {
        FolderEnum Folder { get; }
    }


    [System.Serializable]
    public class SDU_FolderSetting : UnityJsonSerializable, SDUI_FolderSetting, UCLI_FieldOnGUI
    {
        public SDU_FolderSetting() { }
        //public SDU_FolderSetting(FolderEnum iFolderEnum) { m_Folder = iFolderEnum; }
        public FolderEnum m_Folder = FolderEnum.CheckPoints;


        virtual public FolderEnum Folder => m_Folder;
        //virtual public string Path => RunTimeData.InstallSetting.GetFolderPath(Folder);

        public override void DeserializeFromJson(JsonData iJson)
        {
            base.DeserializeFromJson(iJson);
            //Debug.LogError($"m_Folder:{m_Folder}");
        }
        virtual public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            using(var aScope = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(iFieldName, UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                m_Folder = UCL_GUILayout.PopupAuto(m_Folder, iDataDic.GetSubDic("m_Folder"));
                if (GUILayout.Button("Open Folder", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    RunTimeData.InstallSetting.OpenFolder(m_Folder);
                }
            }

            return this;
        }
    }
}