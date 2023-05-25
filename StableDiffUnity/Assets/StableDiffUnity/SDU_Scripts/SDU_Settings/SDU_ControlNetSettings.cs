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
namespace SDU
{
    [System.Serializable]
    public class ControlNetSettings : UCL.Core.UI.UCLI_FieldOnGUI
    {
        public bool m_EnableControlNet = false;
        public List<string> GetAllModels() => RunTimeData.Ins.m_WebUISetting.m_ControlNetData.m_ModelList;
        [UCL.Core.PA.UCL_List("GetAllModels")] public string m_SelectedModel;

        public SDU_InputImage m_InputImage = new SDU_InputImage();
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, false);
            return this;
        }
        public JsonData GetConfigJson()//byte[] iDepth = iDepthTexture.EncodeToPNG();
        {
            if (!m_EnableControlNet)
            {
                return null;
            }
            JsonData aData = new JsonData();
            {
                JsonData aArgs = new JsonData();
                aData["args"] = aArgs;
                {
                    var aSetting = RunTimeData.Ins.m_Tex2ImgSettings.m_ControlNetSettings;
                    JsonData aArg1 = new JsonData();
                    //aArg1["module"] = "depth";
                    aArg1["input_image"] = m_InputImage.GetTextureBase64String();
                    aArg1["model"] = aSetting.m_SelectedModel;//"control_sd15_depth"
                    aArgs.Add(aArg1);
                }
            }
            return aData;
        }
    }
}
