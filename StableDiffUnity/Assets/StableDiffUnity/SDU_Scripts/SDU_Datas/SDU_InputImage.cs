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
    public class AutoCaptureSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        public bool m_SaveAutoCaptureImage = true;
        /// <summary>
        /// Interval in seconds
        /// </summary>
        public float m_AutoCaptureInterval = 0.1f;

        public List<URP_Camera.CaptureMode> m_AutoCaptureModes = new List<URP_Camera.CaptureMode>();

        public System.DateTime PrevCaptureTime { get; set; } = System.DateTime.MinValue;
        public bool CheckAutoCaptureTime()
        {
            if ((System.DateTime.Now - PrevCaptureTime).TotalSeconds >= m_AutoCaptureInterval)
            {
                PrevCaptureTime = System.DateTime.Now;
                return true;
            }
            return false;
        }
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, false);
            if(iDataDic.GetData(UCL_GUILayout.IsShowFieldKey, false))
            {
                if (!URP_Camera.IsAutoCapturing)
                {
                    if (GUILayout.Button("Enable Auto Capture", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                    {
                        URP_Camera.EnableAutoCapture(this, SDU_InputImage.CurOnGUIInputImage);
                    }
                }
                else
                {
                    if (GUILayout.Button("Disable Auto Capture", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                    {
                        URP_Camera.DisableAutoCapture();
                    }
                }
            }
            return this;
        }
    }
    public class SDU_InputImage : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UCLI_ShortName, UCL.Core.UI.UCLI_FieldOnGUI
    {
        public static SDU_InputImage CurOnGUIInputImage { get; private set; } = null;
        public class LoadImageSetting : UCL.Core.UI.UCLI_FieldOnGUI
        {
            public bool RequireClearDic { get; set; } = false;

            [UCL.Core.PA.UCL_FolderExplorer(UCL.Core.PA.ExplorerType.None)]
            public string m_FolderPath = string.Empty;//SDU_StableDiffusionPage.Data.m_InstallSetting.EnvInstallRoot;

            public IList<string> GetAllFileNames() {

                var aFileNames = UCL.Core.FileLib.Lib.GetFilesName(m_FolderPath, "*.png");
                if(aFileNames.IsNullOrEmpty()) return new List<string> { string.Empty };
                return aFileNames;
            }
            /// <summary>
            /// ÀÉ®×¦WºÙ
            /// </summary>
            [UCL.Core.PA.UCL_List("GetAllFileNames")]
            public string m_FileName = "InputImage.png";

            public string FilePath => string.IsNullOrEmpty(m_FileName) ? string.Empty : Path.Combine(m_FolderPath, m_FileName);
            public bool FileExist => File.Exists(FilePath);

            public void SetPath(string iFolderPath, string iFileName)
            {
                RequireClearDic = true;
                m_FolderPath = iFolderPath;
                m_FileName = iFileName;
            }
            public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
            {
                if (string.IsNullOrEmpty(m_FolderPath))
                {
                    m_FolderPath = RunTimeData.InstallSetting.OutputPath;
                }
                //Debug.LogWarning($"m_FolderPath:{m_FolderPath}");
                GUILayout.BeginHorizontal();
                UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, true);
                if (GUILayout.Button("Open", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    if (!Directory.Exists(m_FolderPath))
                    {
                        UCL.Core.FileLib.Lib.CreateDirectory(m_FolderPath);
                    }
                    System.Diagnostics.Process.Start(m_FolderPath);
                }
                GUILayout.EndHorizontal();
                return this;
            }
        }
        public class ImageSetting : UCL.Core.UI.UCLI_FieldOnGUI
        {
            [UCL.Core.ATTR.UCL_HideOnGUI]
            public bool m_ShowImageDetail = false;
            public bool m_SaveImageAfterCapture = true;
            [HideInInspector] public Texture2D Texture;

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
        

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public LoadImageSetting m_LoadImageSetting = new LoadImageSetting();

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public ImageSetting m_ImageSetting = new ImageSetting();

        //public AutoCaptureMode m_AutoCaptureMode = AutoCaptureMode.Off;

        public AutoCaptureSetting m_AutoCaptureSetting = new AutoCaptureSetting();

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public URP_Camera.CaptureMode m_CaptureMode = URP_Camera.CaptureMode.Depth;

        public bool ShowImageDetail => m_ShowImageDetail;
        [UCL.Core.ATTR.UCL_HideOnGUI]
        public bool m_ShowImageDetail = false;

        private bool m_StartCapture = false;//m_AutoCaptureMode
        public Texture2D Texture { get => m_ImageSetting.Texture; set => m_ImageSetting.Texture = value; }

        public SDU_InputImage() { }
        ~SDU_InputImage()
        {
            Clear();
        }
        public void Clear()
        {
            if (Texture != null)
            {
                GameObject.DestroyImmediate(Texture);
                Texture = null;
            }
        }
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
            CurOnGUIInputImage = this;
            using (var aScope = new GUILayout.VerticalScope("box"))
            {
                {
                    GUILayout.BeginHorizontal();
                    if (Texture != null)
                    {
                        var aSize = SDU_Util.GetTextureSize(64, Texture);
                        GUILayout.Box(Texture, GUILayout.Width(aSize.x), GUILayout.Height(aSize.y));
                    }
                    m_ShowImageDetail = UCL.Core.UI.UCL_GUILayout.Toggle(m_ShowImageDetail);


                    using (var aScope2 = new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(iFieldName, GUILayout.ExpandWidth(false));

                        var aCam = URP_Camera.CurCamera;
                        if (aCam != null)
                        {
                            using (var aScope3 = new GUILayout.HorizontalScope())//"box"
                            {
                                if (GUILayout.Button("Capture Image", UCL.Core.UI.UCL_GUIStyle.ButtonStyle,
                                    GUILayout.ExpandWidth(false)))
                                {
                                    CaptureImage(new List<URP_Camera.CaptureMode>() { m_CaptureMode }, m_ImageSetting.m_SaveImageAfterCapture);
                                }
                                m_CaptureMode = UCL_GUILayout.PopupAuto(m_CaptureMode, iDataDic.GetSubDic("CaptureMode"),
                                    "CaptureMode");
                            }
                        }
                        UCL_GUILayout.DrawCopyPaste(this);
                        GUILayout.EndHorizontal();


                        if (m_ShowImageDetail)
                        {
                            try
                            {
                                if (Texture != null)
                                {
                                    using (var aScope3 = new GUILayout.HorizontalScope())
                                    {
                                        if (GUILayout.Button("Save Image", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                                        {
                                            UCL.Core.ServiceLib.UCL_UpdateService.AddAction(SaveImage);
                                        }
                                        if (GUILayout.Button("Open Output Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                                        {
                                            var aSavePath = SDU_ImageGenerator.GetSaveImagePath();
                                            string aFolderPath = aSavePath.Item1;

                                            System.Diagnostics.Process.Start(aFolderPath);
                                            //RunTimeData.Ins.m_Tex2ImgSettings.m_ImageOutputSetting.OpenFolder();
                                        }
                                    }
                                }

                                GUILayout.BeginHorizontal();
                                if (File.Exists(m_LoadImageSetting.FilePath))
                                {
                                    if (GUILayout.Button("Load Image", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                                    {
                                        UCL.Core.ServiceLib.UCL_UpdateService.AddAction(LoadImage);
                                    }
                                }
                                //iFieldName
                                var aLoadImageSettingDic = iDataDic.GetSubDic("LoadImageSetting");
                                if (m_LoadImageSetting.RequireClearDic)
                                {
                                    m_LoadImageSetting.RequireClearDic = false;
                                    aLoadImageSettingDic.Clear();
                                }
                                UCL.Core.UI.UCL_GUILayout.DrawObjectData(m_LoadImageSetting, aLoadImageSettingDic, "LoadImageSetting", false);
                                GUILayout.EndHorizontal();

                                UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic.GetSubDic("InputImage"), "InputImage", true);
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

            //if (!m_StartCapture)
            //{
            //    if (URP_Camera.IsAutoCapturing)
            //    {
            //        if ((System.DateTime.Now - m_AutoCaptureSetting.PrevCaptureTime).TotalSeconds >= m_AutoCaptureSetting.m_AutoCaptureInterval)
            //        {
            //            m_AutoCaptureSetting.PrevCaptureTime = System.DateTime.Now;
            //            var aCaptureModes = m_AutoCaptureSetting.m_AutoCaptureModes.Clone();
            //            if (aCaptureModes.Count == 0) aCaptureModes.Add(m_CaptureMode);
            //            CaptureImage(aCaptureModes, m_AutoCaptureSetting.m_SaveAutoCaptureImage);
            //        }
            //    }
            //}
            CurOnGUIInputImage = null;

            return this;
        }
        public void CaptureImage(List<URP_Camera.CaptureMode> iCaptureModes, bool iSaveAfterCapture)
        {
            //Debug.LogWarning($"CaptureImage iCaptureMod:{iCaptureMod}");
            if (m_StartCapture)
            {
                return;
            }
            m_StartCapture = true;
            UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
            {
                try
                {
                    var aCam = URP_Camera.CurCamera;
                    if (aCam != null)
                    {
                        var aSetting = RunTimeData.Ins.CurImgSetting;

                        var aFilePaths = aCam.CaptureImage(aSetting.m_Width, aSetting.m_Height, ref m_ImageSetting.Texture, iCaptureModes, iSaveAfterCapture);
                        if (!aFilePaths.IsNullOrEmpty())
                        {
                            var aPath = aFilePaths.LastElement();
                            m_LoadImageSetting.SetPath(aPath.Item1, aPath.Item2);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("UCL_UpdateService, CaptureImage aCam == null");
                    }
                }
                catch(System.Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    m_StartCapture = false;
                }
            });
        }
        public void SaveImage()
        {
            try
            {
                var aPath = SDU_ImageGenerator.SaveImage(Texture);
                m_LoadImageSetting.SetPath(aPath.Item1, aPath.Item2);
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
                    Clear();
                    Texture = aTexture;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        public override void DeserializeFromJson(JsonData iJson)
        {
            Clear();
            base.DeserializeFromJson(iJson);
            if (File.Exists(m_LoadImageSetting.FilePath))
            {
                LoadImage();
            }
        }
        public string GetTextureBase64String()
        {
            if (Texture == null)
            {
                if (File.Exists(m_LoadImageSetting.FilePath))
                {
                    LoadImage();
                }

                if (Texture == null)//Load Fail
                {
                    return string.Empty;
                }
            }
            byte[] iBytes = Texture.EncodeToPNG();
            return Convert.ToBase64String(iBytes);
        }
    }
}
