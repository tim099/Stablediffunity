using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;


namespace SDU
{
    [System.Serializable]
    public class SDU_FolderSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        public SDU_FolderSetting() { }
        //public SDU_FolderSetting(FolderEnum iFolderEnum) { m_Folder = iFolderEnum; }
        public FolderEnum m_Folder = FolderEnum.CheckPoints;

        public string Path => RunTimeData.InstallSetting.GetFolderPath(m_Folder);

        public override void DeserializeFromJson(JsonData iJson)
        {
            base.DeserializeFromJson(iJson);
            //Debug.LogError($"m_Folder:{m_Folder}");
        }
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
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