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

namespace SDU
{
    [System.Serializable]
    public class ControlNetSettings : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        [UCL.Core.ATTR.UCL_HideOnGUI]
        public bool m_EnableControlNet = true;
        //public List<string> GetAllModels() => RunTimeData.Ins.m_WebUISetting.m_ControlNetData.m_ModelList;
        //[UCL.Core.PA.UCL_List("GetAllModels")] 
        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string m_SelectedModel;

        public SDU_InputImage m_InputImage = new SDU_InputImage();
        [UCL.Core.PA.UCL_Slider(0f, 2f)]
        public float m_ControlWeight = 1f;

        [UCL.Core.PA.UCL_Slider(0f, 1f)]
        public float m_StartingControlStep = 0f;
        [UCL.Core.PA.UCL_Slider(0f, 1f)]
        public float m_EndingControlStep = 1f;

        private bool m_Show = false;
        public bool RequireClearDic { get; set; } = false;
        public override void DeserializeFromJson(JsonData iJson)
        {
            base.DeserializeFromJson(iJson);
            RequireClearDic = true;
        }
        public void SetInputImage(SDU_InputImage iInputImage)
        {
            RequireClearDic = true;
            m_InputImage.DeserializeFromJson(iInputImage.SerializeToJson());
        }
        public void SetEnable(bool iEnable)
        {
            RequireClearDic = true;
            m_EnableControlNet = iEnable;
        }
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            if (RequireClearDic)
            {
                RequireClearDic = false;
                iDataDic.Clear();
            }
            using (var aScope = new GUILayout.HorizontalScope())
            {
                m_Show = UCL_GUILayout.Toggle(m_Show);
                using (var aScope2 = new GUILayout.VerticalScope())
                {
                    using (var aScope3 = new GUILayout.HorizontalScope())
                    {
                        m_EnableControlNet = UCL_GUILayout.CheckBox(m_EnableControlNet);
                        GUILayout.Label($"{iFieldName} [{m_SelectedModel}]", UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                        GUILayout.FlexibleSpace();
                        UCL_GUILayout.DrawCopyPaste(this);
                    }

                    if (m_Show)
                    {
                        using (var aScope3 = new GUILayout.HorizontalScope("box"))
                        {
                            if (GUILayout.Button("Refresh", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                            {
                                RunTimeData.Ins.m_WebUISetting.RefreshControlNetModels().Forget();
                            }

                            GUILayout.Label("Selected Model", UCL.Core.UI.UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));

                            var aNames = RunTimeData.Ins.m_WebUISetting.m_ControlNetData.m_ModelList;
                            if (!aNames.IsNullOrEmpty())
                            {
                                m_SelectedModel = UCL_GUILayout.PopupAuto(m_SelectedModel, aNames, iDataDic, "Selected Model", 8);
                            }

                            if (GUILayout.Button("Open Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                            {
                                RunTimeData.InstallSetting.OpenFolder(FolderEnum.ControlNetModel);
                            }
                        }

                        UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, true);
                    }
                }
            }

            return this;
        }
        public JsonData GetConfigJson()//byte[] iDepth = iDepthTexture.EncodeToPNG();
        {
            if (!m_EnableControlNet)
            {
                return null;
            }
            JsonData aArg1 = new JsonData();
            aArg1["input_image"] = m_InputImage.GetTextureBase64String();
            aArg1["model"] = m_SelectedModel;//"control_sd15_depth"
            aArg1["weight"] = m_ControlWeight;
            aArg1["guidance_start"] = m_StartingControlStep;
            aArg1["guidance_end"] = m_EndingControlStep;
            return aArg1;
        }
    }
}
