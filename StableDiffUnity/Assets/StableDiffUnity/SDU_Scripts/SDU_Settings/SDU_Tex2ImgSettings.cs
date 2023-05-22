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
    [UCL.Core.ATTR.EnableUCLEditor]
    [System.Serializable]
    public class Tex2ImgSettings : UCL.Core.UI.UCLI_FieldOnGUI
    {
        public List<string> GetAllModelNames() => SDU_StableDiffusionPage.Data.m_WebUISettings.m_ModelNames;
        [UCL.Core.PA.UCL_List("GetAllModelNames")] public string m_SelectedModel;
        public List<string> GetAllSamplerNames() => SDU_StableDiffusionPage.Data.m_WebUISettings.m_Samplers;
        [UCL.Core.PA.UCL_List("GetAllSamplerNames")] public string m_SelectedSampler;


        //public List<string> GetAllLoraNames() => Data.m_WebUISettings.m_LoraNames;
        //[UCL.Core.PA.UCL_Button("AddLora")]
        //[UCL.Core.PA.UCL_List("GetAllLoraNames")] 
        [UCL.Core.ATTR.UCL_HideOnGUI] public string m_SelectedLoraModel;

        public string m_Prompt = "masterpiece, best quality, ultra-detailed,((black background)),1girl,";
        public string m_NegativePrompt;
        public int m_Width = 512;
        public int m_Height = 512;
        public int m_Steps = 20;
        public float m_CfgScale = 7;
        public long m_Seed = -1;

        //[UCL.Core.ATTR.UCL_HideOnGUI] 
        public ControlNetSettings m_ControlNetSettings = new ControlNetSettings();

        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            using (var aScope = new GUILayout.HorizontalScope("box"))
            {
                if (GUILayout.Button("Add Lora", GUILayout.ExpandWidth(false)))
                {
                    m_Prompt += $"<lora:{m_SelectedLoraModel}:1>";
                }
                var aLoraNames = SDU_StableDiffusionPage.Data.m_WebUISettings.m_LoraNames;
                int aIndex = aLoraNames.IndexOf(m_SelectedLoraModel);
                int aSelectedIndex = UCL.Core.UI.UCL_GUILayout.PopupAuto(aIndex, aLoraNames, iDataDic, "Lora", 8);
                m_SelectedLoraModel = aLoraNames[aSelectedIndex];
            }
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic.GetSubDic("Tex2Img"), iFieldName, true);
            //m_ControlNetSettings.OnGUI(iDataDic.GetSubDic("ControlNetSettings"));
            return this;
        }
    }

    [System.Serializable]
    public class ControlNetSettings : UCL.Core.UI.UCLI_FieldOnGUI
    {
        public bool m_EnableControlNet = false;
        //Data.m_WebUISettings.m_ControlNetSettings.m_ModelList
        public List<string> GetAllModels() => SDU_StableDiffusionPage.Data.m_WebUISettings.m_ControlNetSettings.m_ModelList;
        [UCL.Core.PA.UCL_List("GetAllModels")] public string m_SelectedModel;

        public URP_InputImage m_InputImage = new URP_InputImage();
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
                    var aSetting = SDU_StableDiffusionPage.Data.m_Tex2ImgSettings.m_ControlNetSettings;
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

    public class URP_InputImage : UCL.Core.UI.UCLI_FieldOnGUI
    {
        public class LoadImageSetting : UCL.Core.UI.UCLI_FieldOnGUI
        {
            [UCL.Core.PA.UCL_FolderExplorer(UCL.Core.PA.ExplorerType.None)]
            public string m_FolderPath = string.Empty;//SDU_StableDiffusionPage.Data.m_InstallSetting.EnvInstallRoot;
            /// <summary>
            /// 檔案名稱
            /// </summary>
            [UCL.Core.PA.UCL_List("GetAllFileNames")]
            public string m_FileName = "InputImage.png";

            public string FilePath => string.IsNullOrEmpty(m_FileName)?string.Empty : Path.Combine(m_FolderPath, m_FileName);
            public IList<string> GetAllFileNames()
            {
                if (!Directory.Exists(m_FolderPath))
                {
                    return null;
                }
                return UCL.Core.FileLib.Lib.GetFilesName(m_FolderPath,"*.png"); //RCG_FileData.GetFileData(m_FolderPath, "*");
            }
            public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
            {
                if (string.IsNullOrEmpty(m_FolderPath))
                {
                    m_FolderPath = SDU_StableDiffusionPage.Data.m_InstallSetting.OutputPath;
                }
                //Debug.LogWarning($"m_FolderPath:{m_FolderPath}");
                UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, true);
                return this;
            }
        }
        public LoadImageSetting m_LoadImageSetting = new LoadImageSetting();
        public Texture2D Texture => m_Texture;
        private Texture2D m_Texture;
        private Texture2D m_LoadFromFileTexture;
        /// <summary>
        /// return new data if the data of field altered
        /// </summary>
        /// <param name="iFieldName"></param>
        /// <param name="iEditTmpDatas"></param>
        /// <returns></returns>
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            using(var aScope = new GUILayout.VerticalScope("box"))
            {
                
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(iFieldName, GUILayout.ExpandWidth(false));

                    if (m_Texture != null)
                    {
                        if (GUILayout.Button("SaveImage"))
                        {
                            UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                            {
                                var aSavePath = SDU_StableDiffusionPage.GetSaveImagePath();
                                string aPath = aSavePath.Item1;
                                string aFileName = aSavePath.Item2;
                                string aFilePath = Path.Combine(aPath, $"Input_{aFileName}.png"); // M HH:mm:ss
                                Debug.Log($"Save Image Path:{aFilePath}");

                                File.WriteAllBytes(aFilePath, m_Texture.EncodeToPNG());
                            });
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                
                var aCam = URP_Camera.CurCamera;
                if (aCam != null)
                {
                    using (var aScope2 = new GUILayout.HorizontalScope("box"))
                    {
                        if (GUILayout.Button("Capture Depth"))
                        {
                            UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                            {
                                if (aCam != null)
                                {
                                    var aSetting = SDU_StableDiffusionPage.Data.m_Tex2ImgSettings;
                                    m_Texture = aCam.CreateDepthImage(aSetting.m_Width, aSetting.m_Height);
                                }
                            });
                        }
                        if (GUILayout.Button("Capture Normal"))
                        {
                            UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                            {
                                if (aCam != null)
                                {
                                    var aSetting = SDU_StableDiffusionPage.Data.m_Tex2ImgSettings;
                                    m_Texture = aCam.CreateNormalImage(aSetting.m_Width, aSetting.m_Height);
                                }
                            });
                        }
                    }
                }
                if (m_Texture != null)
                {
                    GUILayout.Box(m_Texture, GUILayout.MaxHeight(256));
                }
            }
            try
            {
                UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, false);
                var aPath = m_LoadImageSetting.FilePath;
                if (File.Exists(aPath))
                {
                    if (GUILayout.Button("LoadImage"))
                    {
                        UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                        {
                            try
                            {
                                var aBytes = File.ReadAllBytes(aPath);
                                var aTexture = UCL.Core.TextureLib.Lib.CreateTexture(aBytes);
                                if (aTexture != null)
                                {
                                    if (m_LoadFromFileTexture != null)
                                    {
                                        GameObject.DestroyImmediate(m_LoadFromFileTexture);
                                    }
                                    m_Texture = m_LoadFromFileTexture = aTexture;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        });
                    }
                }
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
            }

            return this;
        }

        public string GetTextureBase64String()
        {
            if (m_Texture == null) return string.Empty;
            byte[] iBytes = m_Texture.EncodeToPNG();
            return Convert.ToBase64String(iBytes);
        }
    }
}