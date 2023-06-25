using System;
using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;


namespace SDU
{
    [System.Serializable]
    public class SDU_InstallFolderSetting : UnityJsonSerializable, SDUI_FolderSetting, UCLI_FieldOnGUI
    {
        public SDU_InstallFolderSetting() { }
        public enum InstallFolderEnum
        {
            /// <summary>
            /// CheckPoint
            /// </summary>
            CheckPoints,

            Lora,
            LyCORIS,

            VAE,

            ControlNetModel,
        }
        public InstallFolderEnum m_Folder = InstallFolderEnum.CheckPoints;


        virtual public FolderEnum Folder => Enum.Parse<FolderEnum>(m_Folder.ToString());
        //virtual public string Path => RunTimeData.InstallSetting.GetFolderPath(Folder);

        public override void DeserializeFromJson(JsonData iJson)
        {
            base.DeserializeFromJson(iJson);
            //Debug.LogError($"m_Folder:{m_Folder}");
        }
        virtual public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            using (var aScope = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(iFieldName, UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                m_Folder = UCL_GUILayout.PopupAuto(m_Folder, iDataDic.GetSubDic("m_Folder"));
                if (GUILayout.Button("Open Folder", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    RunTimeData.InstallSetting.OpenFolder(Folder);
                }
            }

            return this;
        }
    }
}
