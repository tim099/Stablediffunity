using System.Collections;
using System.Collections.Generic;
using UCL.Core.JsonLib;
using UnityEngine;


namespace SDU
{
    [System.Serializable]
    public class SDU_WebUIExtensionSetting : UCL.Core.JsonLib.UnityJsonSerializable
    {
        public string stablediffunity = "SDU_WebUIExtensionSetting";
        public int arg2 = 1125;
        public string OutputPath;
        public bool OutputTensors = false;

        public bool LoadTensor = false;
        public override JsonData SerializeToJson()
        {
            if (string.IsNullOrEmpty(OutputPath))
            {
                OutputPath = RunTimeData.Ins.CurImgSetting.m_ImageOutputSetting.OutputFolderPath;
            }
            return base.SerializeToJson();
        }
    }
}