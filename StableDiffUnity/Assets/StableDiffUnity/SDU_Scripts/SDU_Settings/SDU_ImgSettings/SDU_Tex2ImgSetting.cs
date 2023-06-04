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
            PresetOnGUI(iSubDic);
            LoraOnGUI(iSubDic);

            UCL.Core.UI.UCL_GUILayout.DrawField(this, iSubDic.GetSubDic("Tex2Img"), iFieldName, true);
            UCL.Core.UI.UCL_GUILayout.DrawObjectData(m_CMDs, iDataDic.GetSubDic("CMDs"), "CMDs", false);

            if (SDU_Server.ServerReady)
            {
                if (!SDU_CMDService.TriggeringCMD)
                {
                    if (GUILayout.Button("Generate Image", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                    {
                        var aCMD = new SDU_CMDGenerateImage();
                        var aCMDs = new List<SDU_CMD>() { aCMD };
                        SDU_CMDService.TriggerCMDs(this, aCMDs, new CancellationTokenSource()).Forget();
                    }
                    if (!m_CMDs.IsNullOrEmpty())
                    {
                        if (GUILayout.Button("Trigger CMDs", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                        {
                            var aCMDs = new List<SDU_CMD>();
                            foreach (var aCMD in m_CMDs)
                            {
                                aCMDs.Append(aCMD.GetCMDList());
                            }
                            UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                            {
                                SDU_CMDService.TriggerCMDs(this, aCMDs, new CancellationTokenSource()).Forget();
                            });
                        }
                    }
                }

                SDU_CMDService.OnGUI(iDataDic.GetSubDic("SDU_CMDService"));
            }

            SDU_ImageGenerator.OnGUI(iDataDic.GetSubDic("SDU_ImageGenerator"));

            return this;
        }
    }
}