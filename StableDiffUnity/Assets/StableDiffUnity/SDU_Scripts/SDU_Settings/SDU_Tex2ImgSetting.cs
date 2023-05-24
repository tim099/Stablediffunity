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
    public class Tex2ImgSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        public List<string> GetAllModelNames() => RunTimeData.Ins.m_WebUISetting.m_ModelNames;
        [UCL.Core.PA.UCL_List("GetAllModelNames")] public string m_SelectedModel;
        public List<string> GetAllSamplerNames() => RunTimeData.Ins.m_WebUISetting.m_Samplers;
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
        [UCL.Core.PA.UCL_Slider(1, 30)]
        public float m_CfgScale = 7;
        public long m_Seed = -1;

        [UCL.Core.PA.UCL_IntSlider(1, 100)]
        public int m_BatchCount = 1;
        [UCL.Core.PA.UCL_IntSlider(1, 8)]
        public int m_BatchSize = 1;
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
                
                var aLoraNames = RunTimeData.Ins.m_WebUISetting.m_LoraNames;
                if (!aLoraNames.IsNullOrEmpty())
                {
                    int aIndex = aLoraNames.IndexOf(m_SelectedLoraModel);
                    int aSelectedIndex = UCL.Core.UI.UCL_GUILayout.PopupAuto(aIndex, aLoraNames, iDataDic, "Lora", 8);
                    m_SelectedLoraModel = aLoraNames[aSelectedIndex];
                }
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
        public List<string> GetAllModels() => RunTimeData.Ins.m_WebUISetting.m_ControlNetData.m_ModelList;
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

    public class URP_InputImage : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UCLI_ShortName, UCL.Core.UI.UCLI_FieldOnGUI
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
                    m_FolderPath = RunTimeData.Ins.m_InstallSetting.OutputPath;
                }
                //Debug.LogWarning($"m_FolderPath:{m_FolderPath}");
                UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, true);
                return this;
            }
        }
        public class ImageSetting : UCL.Core.UI.UCLI_FieldOnGUI
        {
            [UCL.Core.ATTR.UCL_HideOnGUI]
            public bool m_ShowImageDetail = false;
            public bool m_SaveImageAfterCapture = true;
            public Texture2D Texture { get; set; }
            public Texture2D LoadFromFileTexture { get; set; }

            public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
            {
                //Debug.LogWarning($"m_FolderPath:{m_FolderPath}");
                UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, true);
                GUILayout.BeginHorizontal();
                m_ShowImageDetail = UCL.Core.UI.UCL_GUILayout.Toggle(m_ShowImageDetail);
                GUILayout.Label("Image Detail");
                GUILayout.EndHorizontal();
                if (m_ShowImageDetail)
                {
                    if (Texture != null)
                    {
                        var aSize = SDU_Util.GetTextureSize(512, Texture);
                        GUILayout.Box(Texture, GUILayout.Width(aSize.x), GUILayout.Height(aSize.y));
                    }
                }
                return this;
            }
        }
        public LoadImageSetting m_LoadImageSetting = new LoadImageSetting();
        public ImageSetting m_ImageSetting = new ImageSetting();

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public bool m_ShowImageDetail = false;
        //[UCL.Core.ATTR.UCL_HideOnGUI]
        
        public Texture2D Texture { get => m_ImageSetting.Texture; set => m_ImageSetting.Texture = value; }
        private Texture2D LoadFromFileTexture 
        { get => m_ImageSetting.LoadFromFileTexture; set => m_ImageSetting.LoadFromFileTexture = value; }
        public string GetShortName()
        {
            return $"{m_LoadImageSetting.m_FileName}";//InputImage File:
        }
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
                    if (Texture != null)
                    {
                        var aSize = SDU_Util.GetTextureSize(48, Texture);
                        GUILayout.Box(Texture, GUILayout.Width(aSize.x), GUILayout.Height(aSize.y));
                    }
                    m_ShowImageDetail = UCL.Core.UI.UCL_GUILayout.Toggle(m_ShowImageDetail);

                    
                    using(var aScope2 = new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(iFieldName, GUILayout.ExpandWidth(false));
                        if (Texture != null)
                        {
                            if (GUILayout.Button("SaveImage"))
                            {
                                UCL.Core.ServiceLib.UCL_UpdateService.AddAction(SaveImage);
                            }
                        }
                        GUILayout.EndHorizontal();
                        var aCam = URP_Camera.CurCamera;
                        if (aCam != null)
                        {
                            using (var aScope3 = new GUILayout.HorizontalScope("box"))
                            {
                                if (GUILayout.Button("Capture Depth"))
                                {
                                    UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                                    {
                                        if (aCam != null)
                                        {
                                            var aSetting = RunTimeData.Ins.m_Tex2ImgSettings;
                                            Texture = aCam.CreateDepthImage(aSetting.m_Width, aSetting.m_Height);
                                        }
                                        if (m_ImageSetting.m_SaveImageAfterCapture)
                                        {
                                            SaveImage();
                                        }
                                    });
                                }
                                if (GUILayout.Button("Capture Normal"))
                                {
                                    UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                                    {
                                        if (aCam != null)
                                        {
                                            var aSetting = RunTimeData.Ins.m_Tex2ImgSettings;
                                            Texture = aCam.CreateNormalImage(aSetting.m_Width, aSetting.m_Height);
                                        }
                                        if (m_ImageSetting.m_SaveImageAfterCapture)
                                        {
                                            SaveImage();
                                        }
                                    });
                                }
                            }
                        }

                        if (m_ShowImageDetail)
                        {
                            try
                            {
                                GUILayout.BeginHorizontal();
                                if (File.Exists(m_LoadImageSetting.FilePath))
                                {
                                    if (GUILayout.Button("LoadImage", GUILayout.ExpandWidth(false)))
                                    {
                                        UCL.Core.ServiceLib.UCL_UpdateService.AddAction(LoadImage);
                                    }
                                }
                                //iFieldName
                                UCL.Core.UI.UCL_GUILayout.DrawObjectData(m_LoadImageSetting, iDataDic.GetSubDic("LoadImageSetting"), "LoadImageSetting", false);
                                GUILayout.EndHorizontal();

                                UCL.Core.UI.UCL_GUILayout.DrawObjectData(m_ImageSetting, iDataDic.GetSubDic("ImageSetting"), "ImageSetting", false);
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogException(e);
                            }
                        }
                    }

                    GUILayout.EndHorizontal();
                }

            }



            return this;
        }
        public void SaveImage()
        {
            try
            {
                var aSavePath = SDU_StableDiffusionPage.GetSaveImagePath();
                string aFolderPath = aSavePath.Item1;
                string aFileName = $"Input_{aSavePath.Item2}.png";
                string aFilePath = Path.Combine(aFolderPath, aFileName); // M HH:mm:ss
                Debug.Log($"Save Image Path:{aFilePath}");
                if(!Directory.Exists(aFolderPath))
                {
                    UCL.Core.FileLib.Lib.CreateDirectory(aFolderPath);
                }
                
                File.WriteAllBytes(aFilePath, Texture.EncodeToPNG());
                m_LoadImageSetting.m_FolderPath = aFolderPath;
                m_LoadImageSetting.m_FileName = aFileName;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        public void LoadImage()
        {
            try
            {
                var aPath = m_LoadImageSetting.FilePath;
                if (!File.Exists(aPath))
                {
                    Debug.LogError($"LoadImage() File.Exists(aPath) aPath:{aPath}");
                    return;
                }
                var aBytes = File.ReadAllBytes(aPath);
                var aTexture = UCL.Core.TextureLib.Lib.CreateTexture(aBytes);
                if (aTexture != null)
                {
                    if (LoadFromFileTexture != null)
                    {
                        GameObject.DestroyImmediate(LoadFromFileTexture);
                    }
                    Texture = LoadFromFileTexture = aTexture;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        public override void DeserializeFromJson(JsonData iJson)
        {
            base.DeserializeFromJson(iJson);
            if (File.Exists(m_LoadImageSetting.FilePath))
            {
                LoadImage();
            }
        }
        public string GetTextureBase64String()
        {
            if (Texture == null) return string.Empty;
            byte[] iBytes = Texture.EncodeToPNG();
            return Convert.ToBase64String(iBytes);
        }
    }
}