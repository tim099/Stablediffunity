using System.Collections;
using System.Collections.Generic;
using UCL.Core.JsonLib;
using UnityEngine;


namespace SDU
{
    [System.Serializable]
    public class SDU_WebUIExtensionSetting : UCL.Core.JsonLib.UnityJsonSerializable
    {
        public string FolderPath;
        public bool OutputTensors = false;

        /// <summary>
        /// Load Tensor from file
        /// </summary>
        public bool LoadTensor = false;
        /// <summary>
        /// FilePath = System.IO.Path.Combine(FolderPath, "tensors", LoadTensorFileName)
        /// </summary>
        public string LoadTensorFileName;
        public override JsonData SerializeToJson()
        {
            
            if (string.IsNullOrEmpty(FolderPath))
            {
                FolderPath = RunTimeData.Ins.CurImgSetting.m_ImageOutputSetting.OutputFolderPath;
            }
            return base.SerializeToJson();
        }
    }
}