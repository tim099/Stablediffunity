using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UCL.Core.UI;
using UnityEngine;


namespace SDU
{
    public class SDU_CheckPointSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string m_CheckPoint;

        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            using (var aScope = new GUILayout.HorizontalScope("box"))
            {
                if (GUILayout.Button("Refresh", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    RunTimeData.Ins.m_WebUISetting.RefreshSamplers().Forget();
                }

                GUILayout.Label(iFieldName, UCL.Core.UI.UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));

                var aNames = RunTimeData.Ins.m_WebUISetting.m_ModelNames;
                if (!aNames.IsNullOrEmpty())
                {
                    m_CheckPoint = UCL_GUILayout.PopupAuto(m_CheckPoint, aNames, iDataDic, "Selected Model", 8);
                }

                if (GUILayout.Button("Open Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    RunTimeData.InstallSetting.OpenFolder(FolderEnum.CheckPoints);
                }
            }
            return this;
        }
    }
}