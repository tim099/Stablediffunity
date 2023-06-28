using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;
namespace SDU
{
    public class SDU_WebUICMDLoadTensor : SDU_WebUICMD
    {
        public string m_FolderPath;
        /// <summary>
        /// Load Tensor at which step,
        /// if m_LoadAtStep == -1 then output image immediately(skip the sample process)
        /// </summary>
        public int m_LoadAtStep = -1;

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string m_LoadTensorFileName = string.Empty;
        public override JsonData GetConfigJson()
        {
            CheckFolderPath();
            return base.GetConfigJson();
        }
        public override JsonData SerializeToJson()
        {
            return base.SerializeToJson();
        }
        private void CheckFolderPath()
        {
            if (string.IsNullOrEmpty(m_FolderPath))
            {
                m_FolderPath = System.IO.Path.Combine(RunTimeData.Ins.CurImgSetting.m_ImageOutputSetting.OutputFolderPath, "tensors");
            }
        }
        public override object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            CheckFolderPath();
            UCL.Core.UI.UCL_GUILayout.DrawObjExSetting aDrawObjExSetting = new()
            {
                OnShowField = () =>
                {
                    string aPath = m_FolderPath;
                    IList<string> aFiles = new List<string>();
                    if (Directory.Exists(aPath))
                    {
                        aFiles = UCL.Core.FileLib.Lib.GetFilesName(aPath, "*.pt");
                    }
                    using (var aScope = new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("LoadTensorFileName", UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                        m_LoadTensorFileName = UCL_GUILayout.PopupAuto(m_LoadTensorFileName, aFiles, iDataDic, "LoadTensorFileName");
                    }
                }
            };
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, iDrawObjExSetting: aDrawObjExSetting);
            return this;
        }
    }
}