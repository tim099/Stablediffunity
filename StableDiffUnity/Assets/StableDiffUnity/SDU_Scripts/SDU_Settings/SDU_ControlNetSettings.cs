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
    public class ControlNetSettings : UCL.Core.UI.UCLI_FieldOnGUI
    {
        public bool m_EnableControlNet = false;
        public List<string> GetAllModels() => RunTimeData.Ins.m_WebUISetting.m_ControlNetData.m_ModelList;
        //[UCL.Core.PA.UCL_List("GetAllModels")] 
        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string m_SelectedModel;

        public SDU_InputImage m_InputImage = new SDU_InputImage();

        private bool m_Show = false;
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            using (var aScope = new GUILayout.HorizontalScope())
            {
                m_Show = UCL_GUILayout.Toggle(m_Show);
                using (var aScope2 = new GUILayout.VerticalScope())
                {
                    GUILayout.Label(iFieldName, UCL_GUIStyle.LabelStyle);
                    if (m_Show)
                    {
                        using (var aScope3 = new GUILayout.HorizontalScope("box"))
                        {
                            if (GUILayout.Button("Refresh", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                            {
                                RunTimeData.Ins.m_WebUISetting.RefreshControlNetModels().Forget();
                            }

                            GUILayout.Label("Selected Model", UCL.Core.UI.UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));

                            var aNames = RunTimeData.Ins.m_WebUISetting.m_ModelNames;
                            if (!aNames.IsNullOrEmpty())
                            {
                                m_SelectedModel = UCL_GUILayout.PopupAuto(m_SelectedModel, GetAllModels(), iDataDic, "Selected Model", 8);
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
