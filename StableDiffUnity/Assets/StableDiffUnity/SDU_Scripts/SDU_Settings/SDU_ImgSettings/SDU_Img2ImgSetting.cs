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
    public class SDU_Img2ImgSetting : SDU_ImgSetting
    {
        [UCL.Core.PA.UCL_Slider(0f, 1f)]
        public float DenoisingStrength = 0.75f;//Denoising strength
        public SDU_InputImage m_InputImage = new SDU_InputImage();
        override public FolderEnum PresetFolder => FolderEnum.Img2ImgPreset;
        public override SDU_WebUIClient.SDU_WebRequest Client => RunTimeData.SD_API.Client_Img2img;
        public override JsonData GetConfigJson()
        {
            var aJson = base.GetConfigJson();
            aJson["denoising_strength"] = DenoisingStrength;

            if(m_InputImage.Texture != null)
            {
                JsonData aInitImages = new JsonData();
                aJson["init_images"] = aInitImages;
                aInitImages.Add(m_InputImage.GetTextureBase64String());
            }
            return aJson;
        }
        public override JsonData SerializeToJson()
        {
            return base.SerializeToJson();
        }
        public override void DeserializeFromJson(JsonData iJson)
        {
            base.DeserializeFromJson(iJson);
            RequireClearDic = true;
        }
        public override object TexSettingOnGUI(string iFieldName, UCL_ObjectDictionary iSubDic, UCL_ObjectDictionary iDataDic)
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

        override public void SetInputImage(SDU_InputImage iInputImage)
        {
            if (iInputImage == null) return;//iInputImage
            if (!iInputImage.m_LoadImageSetting.FileExist)
            {
                return;
            }
            m_InputImage.DeserializeFromJson(iInputImage.SerializeToJson());
        }
    }
}