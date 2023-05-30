using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core.UI;
using UnityEngine;
using System;
using System.Text;
using UCL.Core.EditorLib.Page;
using System.Text.RegularExpressions;
using UCL.Core.JsonLib;
using System.Linq;
using System.Threading.Tasks;
using UCL.Core;
using Cysharp.Threading.Tasks;
using UCL.Core.PA;

namespace SDU
{

    [System.Serializable]
    [UCL.Core.ATTR.EnableUCLEditor]
    public class SDU_ImageOutputSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        public enum OutputFolder
        {
            Default,
            Custom,
        }
        
        public OutputFolder m_OutputFolder = OutputFolder.Default;

        [UCL.Core.PA.Conditional("m_OutputFolder", false, OutputFolder.Custom)]
        //[UCL.Core.PA.UCL_FolderExplorer(ExplorerType.None)]
        public string m_CustomOutputFolder = string.Empty;
        public bool m_OutputControlNetInputImage = false;

        public string OutputFolderPath
        {
            get
            {
                switch (m_OutputFolder)
                {
                    case OutputFolder.Custom:
                        {
                            if (string.IsNullOrEmpty(m_CustomOutputFolder))
                            {
                                m_CustomOutputFolder = SDU_ImageGenerator.DefaultImageOutputFolder();
                            }
                            return m_CustomOutputFolder;
                        }
                }
                return SDU_ImageGenerator.DefaultImageOutputFolder();
            }
        }
        [UCL.Core.ATTR.UCL_FunctionButton("Open Output Folder")]
        public void OpenFolder()
        {
            //RunTimeData.InstallSetting.OpenFolder(FolderEnum.Env);
            var aPath = OutputFolderPath;
            if (!Directory.Exists(aPath))
            {
                Directory.CreateDirectory(aPath);
            }
            System.Diagnostics.Process.Start(aPath);
        }
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            var aDic = iDataDic.GetSubDic("ImageOutputSetting");
            UCL_GUILayout.DrawField(this, aDic, iFieldName, false);
            //bool aIsShowField = aDic.GetData(UCL_GUILayout.IsShowFieldKey, false);
            return this;
        }
    }
}