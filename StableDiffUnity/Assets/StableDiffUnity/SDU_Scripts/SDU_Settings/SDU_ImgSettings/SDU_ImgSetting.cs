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
    public class SDU_ImgSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        #region Save & Load Setting
        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string m_ID = "Default";
        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string m_LoadID = string.Empty;
        #endregion

        public SDU_CheckPointSetting m_CheckPoint = new SDU_CheckPointSetting();

        public SDU_SamplerSetting m_Sampler = new SDU_SamplerSetting();

        public string m_Prompt = "masterpiece, best quality, ultra-detailed,((black background)),1girl,";
        public string m_NegativePrompt = "(low quality, worst quality:1.4), ((bad fingers))";
        public int m_Width = 512;
        public int m_Height = 512;

        [UCL.Core.PA.UCL_IntSlider(1, 150)]
        public int m_Steps = 20;

        [UCL.Core.PA.UCL_Slider(1, 30)]
        public float m_CfgScale = 7;

        public long m_Seed = -1;
        //[UCL.Core.PA.UCL_IntSlider(1, 12)] public int m_ClipSkip = 1;

        [UCL.Core.PA.UCL_IntSlider(1, 100)]
        public int m_BatchCount = 1;

        [UCL.Core.PA.UCL_IntSlider(1, 8)]
        public int m_BatchSize = 1;

        //[UCL.Core.ATTR.UCL_HideOnGUI] 
        public ControlNetSettings m_ControlNetSettings = new ControlNetSettings();

        public SDU_ImageOutputSetting m_ImageOutputSetting = new SDU_ImageOutputSetting();

        public JsonData m_ResultInfo = new JsonData();
        #region Hide
        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string m_SelectedLoraModel;

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public List<SDU_CMD> m_CMDs = new List<SDU_CMD>();
        #endregion

        protected bool m_Show = true;
        public bool RequireClearDic { get; set; } = false;
        virtual public SDU_WebUIClient.SDU_WebRequest Client => RunTimeData.SD_API.Client_Txt2img;
        virtual public FolderEnum PresetFolder => FolderEnum.Tex2ImgPreset;
        public override JsonData SerializeToJson()
        {
            return base.SerializeToJson();
        }
        public override void DeserializeFromJson(JsonData iJson)
        {
            base.DeserializeFromJson(iJson);
            RequireClearDic = true;
        }
        virtual public JsonData GetConfigJson()
        {
            const int MaxRes = 2048;
            JsonData aJson = new JsonData();
            if (m_Width <= 0) m_Width = 8;
            if (m_Height <= 0) m_Height = 8;
            if (m_Width > MaxRes) m_Width = MaxRes;
            if (m_Height > MaxRes) m_Height = MaxRes;
            if (m_Width % 8 != 0)
            {
                m_Width += (8 - m_Width % 8);
            }
            if (m_Height % 8 != 0)
            {
                m_Height += (8 - m_Height % 8);
            }

            aJson["sampler_index"] = m_Sampler.m_SelectedSampler;//m_SelectedSampler;
            aJson["prompt"] = m_Prompt;
            aJson["steps"] = m_Steps;
            aJson["negative_prompt"] = m_NegativePrompt;
            aJson["seed"] = m_Seed;
            //aJson["clip_skip"] = m_ClipSkip;
            aJson["cfg_scale"] = m_CfgScale;
            aJson["width"] = m_Width;
            aJson["height"] = m_Height;
            aJson["batch_size"] = m_BatchSize;
            Debug.LogWarning($"m_Width:{m_Width},m_Height:{m_Height}");
            if (m_ControlNetSettings.m_EnableControlNet)
            {
                JsonData aAlwayson = new JsonData();
                aJson["alwayson_scripts"] = aAlwayson;
                {
                    JsonData aControlnet = m_ControlNetSettings.GetConfigJson();//new JsonData();
                    if (aControlnet != null)
                    {
                        aAlwayson["controlnet"] = aControlnet;
                    }
                }
            }
            return aJson;
        }
        virtual public async Task GenerateImage()
        {
            await SDU_ImageGenerator.GenerateImageAsync(this);
        }
        virtual public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            //iDataDic
            UCL_ObjectDictionary aSubDic = iDataDic.GetSubDic("ImgSetting");
            if (RequireClearDic)
            {
                RequireClearDic = false;
                aSubDic.Clear();
            }
            GUILayout.BeginHorizontal();
            m_Show = UCL_GUILayout.Toggle(m_Show);
            GUILayout.Label(iFieldName, UCL_GUIStyle.LabelStyle);
            GUILayout.EndHorizontal();
            if (!m_Show) return this;
            TexSettingOnGUI(iFieldName, aSubDic, iDataDic);
            return this;
        }
        virtual public void PresetOnGUI(UCL_ObjectDictionary iSubDic)
        {
            using (var aScope = new GUILayout.VerticalScope("box"))
            {
                string aPresetPath = RunTimeData.InstallSetting.GetFolderPath(PresetFolder);
                if (!Directory.Exists(aPresetPath))
                {
                    Directory.CreateDirectory(aPresetPath);
                }
                using (var aScope2 = new GUILayout.HorizontalScope("box"))
                {
                    if (GUILayout.Button("Save Setting", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                    {
                        string aFilePath = Path.Combine(aPresetPath, $"{m_ID}.json");
                        File.WriteAllText(aFilePath, SerializeToJson().ToJsonBeautify());
                        m_LoadID = m_ID;
                    }
                    GUILayout.Label("ID", UCL.Core.UI.UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                    m_ID = GUILayout.TextArea(m_ID);
                    if (GUILayout.Button("Open Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                    {
                        RunTimeData.InstallSetting.OpenFolder(PresetFolder);
                    }
                }
                using (var aScope2 = new GUILayout.HorizontalScope("box"))
                {
                    if (GUILayout.Button("Load Setting", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                    {
                        UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                        {
                            string aFilePath = Path.Combine(aPresetPath, $"{m_LoadID}.json");
                            if (File.Exists(aFilePath))
                            {
                                string aJsonStr = File.ReadAllText(aFilePath);
                                iSubDic.Clear();
                                DeserializeFromJson(JsonData.ParseJson(aJsonStr));
                            }
                        });
                    }
                    GUILayout.Label("ID", UCL.Core.UI.UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                    var aFiles = UCL.Core.FileLib.Lib.GetFilesName(aPresetPath, "*.json", SearchOption.TopDirectoryOnly, true);
                    m_LoadID = UCL_GUILayout.PopupAuto(m_LoadID, aFiles, iSubDic, "Setting ID", 8);
                }
            }
        }
        virtual public void LoraOnGUI(UCL_ObjectDictionary iSubDic)
        {
            using (var aScope = new GUILayout.HorizontalScope("box"))
            {
                if (GUILayout.Button("Refresh", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    RunTimeData.Ins.m_WebUISetting.RefreshLora().Forget();
                }

                if (GUILayout.Button("Add Lora", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    m_Prompt += $"<lora:{m_SelectedLoraModel}:1>";
                }


                var aLoraNames = RunTimeData.Ins.m_WebUISetting.m_LoraNames;
                if (!aLoraNames.IsNullOrEmpty())
                {
                    m_SelectedLoraModel = UCL_GUILayout.PopupAuto(m_SelectedLoraModel, aLoraNames, iSubDic, "Lora", 8);
                }

                if (GUILayout.Button("Open Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    RunTimeData.InstallSetting.OpenFolder(FolderEnum.Lora);
                }
            }
        }
        virtual public object TexSettingOnGUI(string iFieldName, UCL_ObjectDictionary iSubDic, UCL_ObjectDictionary iDataDic)
        {
            return this;
        }

        #region Set
        virtual public void SetInputImage(SDU_InputImage iInputImage)
        {

        }
        #endregion
    }
}