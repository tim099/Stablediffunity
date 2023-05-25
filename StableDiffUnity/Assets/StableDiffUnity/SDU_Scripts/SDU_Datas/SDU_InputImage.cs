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
    public class SDU_InputImage : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UCLI_ShortName, UCL.Core.UI.UCLI_FieldOnGUI
    {
        public class LoadImageSetting : UCL.Core.UI.UCLI_FieldOnGUI
        {
            [UCL.Core.PA.UCL_FolderExplorer(UCL.Core.PA.ExplorerType.None)]
            public string m_FolderPath = string.Empty;//SDU_StableDiffusionPage.Data.m_InstallSetting.EnvInstallRoot;
            /// <summary>
            /// ÀÉ®×¦WºÙ
            /// </summary>
            [UCL.Core.PA.UCL_List("GetAllFileNames")]
            public string m_FileName = "InputImage.png";

            public string FilePath => string.IsNullOrEmpty(m_FileName) ? string.Empty : Path.Combine(m_FolderPath, m_FileName);
            public IList<string> GetAllFileNames()
            {
                if (!Directory.Exists(m_FolderPath))
                {
                    return null;
                }
                return UCL.Core.FileLib.Lib.GetFilesName(m_FolderPath, "*.png"); //RCG_FileData.GetFileData(m_FolderPath, "*");
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
            using (var aScope = new GUILayout.VerticalScope("box"))
            {
                {
                    GUILayout.BeginHorizontal();
                    if (Texture != null)
                    {
                        var aSize = SDU_Util.GetTextureSize(48, Texture);
                        GUILayout.Box(Texture, GUILayout.Width(aSize.x), GUILayout.Height(aSize.y));
                    }
                    m_ShowImageDetail = UCL.Core.UI.UCL_GUILayout.Toggle(m_ShowImageDetail);


                    using (var aScope2 = new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(iFieldName, GUILayout.ExpandWidth(false));
                        if (Texture != null)
                        {
                            if (GUILayout.Button("SaveImage", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
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
                                if (GUILayout.Button("Capture Depth", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
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
                                if (GUILayout.Button("Capture Normal", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
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
                                    if (GUILayout.Button("LoadImage", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
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
                var aSavePath = SDU_ImageGenerator.GetSaveImagePath();
                string aFolderPath = aSavePath.Item1;
                string aFileName = $"Input_{aSavePath.Item2}.png";
                string aFilePath = Path.Combine(aFolderPath, aFileName); // M HH:mm:ss
                Debug.Log($"Save Image Path:{aFilePath}");
                if (!Directory.Exists(aFolderPath))
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
