using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;


namespace SDU
{
    [System.Serializable]
    public class SDU_WebUIExtensionSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        public string FolderPath;
        public bool OutputTensors = false;

        /// <summary>
        /// Load Tensor from file
        /// </summary>
        public bool LoadTensor = false;
        /// <summary>
        /// FilePath = System.IO.Path.Combine(FolderPath, LoadTensorFileName)
        /// </summary>
        //[UCL.Core.PA.Conditional("LoadTensor", false, true)]
        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string LoadTensorFileName = string.Empty;

        public List<SDU_WebUICMD> m_WebUICMDs = new List<SDU_WebUICMD>();
        public override JsonData SerializeToJson()
        {
            
            if (string.IsNullOrEmpty(FolderPath))
            {
                FolderPath = System.IO.Path.Combine(RunTimeData.Ins.CurImgSetting.m_ImageOutputSetting.OutputFolderPath, "tensors");
            }
            return base.SerializeToJson();
        }
        virtual public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL.Core.UI.UCL_GUILayout.DrawObjExSetting aDrawObjExSetting = new()
            {
                OnShowField = () =>
                {
                    if (LoadTensor)
                    {
                        string aPath = FolderPath;
                        IList<string> aFiles = new List<string>();
                        if (Directory.Exists(aPath))
                        {
                            aFiles = UCL.Core.FileLib.Lib.GetFilesName(aPath, "*.pt");
                        }
                        using (var aScope = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("LoadTensorFileName", UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                            LoadTensorFileName = UCL_GUILayout.PopupAuto(LoadTensorFileName, aFiles, iDataDic, "LoadTensorFileName");
                        }
                    }
                }
            };
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, iDrawObjExSetting: aDrawObjExSetting);

            
            return this;
        }
    }
}