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
using System.Threading;

namespace SDU
{
    [UCL.Core.ATTR.EnableUCLEditor]
    [System.Serializable]
    public class Tex2ImgSetting : SDU_ImgSetting
    {
        override public JsonData GetConfigJson()
        {
            JsonData aJson = base.GetConfigJson();
            return aJson;
        }

        override public SDU_WebUIClient.SDU_WebRequest Client => RunTimeData.SD_API.Client_Txt2img;
        override public FolderEnum PresetFolder => FolderEnum.Tex2ImgPreset;
        public override void DeserializeFromJson(JsonData iJson)
        {
            base.DeserializeFromJson(iJson);
            RequireClearDic = true;
        }

        
        override public object TexSettingOnGUI(string iFieldName, UCL_ObjectDictionary iSubDic, UCL_ObjectDictionary iDataDic)
        {
            base.TexSettingOnGUI(iFieldName, iSubDic, iDataDic);

            return this;
        }
    }
}