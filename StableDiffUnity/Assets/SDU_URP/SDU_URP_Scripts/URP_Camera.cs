using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SDU
{
    [UCL.Core.ATTR.EnableUCLEditor]
    public class URP_Camera : MonoBehaviour
    {
        [System.Serializable]
        public class MatSetting
        {
            public CaptureMode m_CaptureMode;
            public Material m_Material;
        }
        public enum CaptureMode
        {
            Depth,
            Normal,
            Canny,
        }

        public static URP_Camera CurCamera => s_Cameras.IsNullOrEmpty() ? null : s_Cameras[0];
        public static List<URP_Camera> s_Cameras = new List<URP_Camera>();
        //public static List<RenderTexture> s_RenderTextures = new List<RenderTexture>();
        public static bool IsAutoCapturing => s_AutoCaptureInputImage != null;
        public static bool s_CaptureSingleFrame = true;
        //private static bool s_EnableAutoCapture = false;
        public static AutoCaptureSetting s_AutoCaptureSetting = new AutoCaptureSetting();
        public static SDU_InputImage s_AutoCaptureInputImage = null;

        public List<MatSetting> m_MatSettings = new List<MatSetting>();
        public List<RenderTexture> m_RenderTextures = new List<RenderTexture>();
        public Camera m_Camera;

        private Dictionary<CaptureMode, MatSetting> m_MatDic = null;

        private bool m_Inited = false;
        private void Start()
        {
            Init();
        }
        public void Init()
        {
            if (m_Inited) return;
            m_Inited = true;
            if (m_Camera == null) m_Camera = GetComponent<Camera>();
            m_Camera.enabled = true;
            s_Cameras.Add(this);
            m_MatDic = new Dictionary<CaptureMode, MatSetting>();
            foreach(var aMat in m_MatSettings)
            {
                if (!m_MatDic.ContainsKey(aMat.m_CaptureMode))
                {
                    m_MatDic[aMat.m_CaptureMode] = aMat;
                }
            }
        }
        private void OnDestroy()
        {
            ClearRenderTextures();
            if (m_Inited)
            {
                s_Cameras.Remove(this);
            }
        }
        public static void DisableAutoCapture()
        {
            //s_EnableAutoCapture = false;
            s_AutoCaptureInputImage = null;
        }
        public static void EnableAutoCapture(AutoCaptureSetting iAutoCaptureSetting, SDU_InputImage iInputImage, bool iCaptureSingleFrame = false)
        {
            s_CaptureSingleFrame = iCaptureSingleFrame;
            s_AutoCaptureInputImage = iInputImage;
            s_AutoCaptureSetting.DeserializeFromJson(iAutoCaptureSetting.SerializeToJson());
        }

        public void ClearRenderTextures()
        {
            foreach(var aRenderTexture in m_RenderTextures)
            {
                RenderTexture.ReleaseTemporary(aRenderTexture);
            }
            m_RenderTextures.Clear();
        }
        public RenderTexture CreateRenderTexture(int iWidth, int iHeight)
        {
            var aRenderTexture = RenderTexture.GetTemporary(iWidth, iHeight, 24, GraphicsFormat.R32G32B32A32_SFloat);
            m_RenderTextures.Add(aRenderTexture);
            return aRenderTexture;
        }
        private Material GetMaterial(CaptureMode iCaptureMode)
        {
            if (!m_MatDic.ContainsKey(iCaptureMode))
            {
                Debug.LogError($"GetMaterial !m_MatDic.ContainsKey CaptureMode:{iCaptureMode}");
                return null;
            }
            return m_MatDic[iCaptureMode].m_Material;
        }
        public List<Tuple<string, string>> CaptureImage(int iWidth, int iHeight, ref Texture2D iTexture, 
            List<CaptureMode> iCaptureModes, bool iSaveAfterCapture)
        {
            ClearRenderTextures();

            List<Tuple<string,string>> aSaveFilePaths = new List<Tuple<string,string>>();
            List<Tuple<CaptureMode, RenderTexture>> aRenderTextures = new();
            for (int i = 0; i < iCaptureModes.Count; i++)
            {
                CaptureMode aCaptureMode = iCaptureModes[i];
                
                RenderTexture aRT = null;
                var aMat = GetMaterial(aCaptureMode);
                if (aMat != null)
                {
                    aRT = CaptureImage(iWidth, iHeight, aMat);
                }
                
                if (aRT != null)
                {
                    aRenderTextures.Add(new Tuple<CaptureMode, RenderTexture>(aCaptureMode, aRT));
                }
            }

            
            RenderTexture aCameraRT = CreateRenderTexture(iWidth, iHeight);
            m_Camera.targetTexture = aCameraRT;

            m_Camera.Render();
            m_Camera.targetTexture = null;

            if (!aRenderTextures.IsNullOrEmpty())
            {
                var aFormat = TextureFormat.RGB24;
                if (iTexture != null)
                {
                    if (iTexture.width != iWidth || iTexture.height != iHeight
                        || iTexture.format != aFormat)
                    {
                        Debug.LogWarning($"Refresh m_Texture m_Texture size:" +
                            $"({iTexture.width},{iTexture.height}) Format:{iTexture.format}" +
                            $", to: ({iWidth},{iHeight}) Format:{aFormat}");
                        GameObject.DestroyImmediate(iTexture);
                        iTexture = null;
                    }
                }
                if (iTexture == null)
                {
                    iTexture = new Texture2D(iWidth, iHeight, aFormat, false);
                }

                if (!iSaveAfterCapture)
                {
                    iTexture.ReadPixels(aRenderTextures.LastElement().Item2);
                }
                else
                {
                    var aSetting = RunTimeData.Ins.CurImgSetting.m_ImageOutputSetting;
                    ++aSetting.m_OutPutFileID;
                    foreach (var aRT in aRenderTextures)
                    {
                        string aFolderPath = aRT.Item1.ToString();
                        iTexture.ReadPixels(aRT.Item2);
                        aSaveFilePaths.Add(SDU_ImageGenerator.SaveImage(iTexture, aFolderPath, aSetting, false));
                    }
                }
            }
            return aSaveFilePaths;
        }
        public BlitToCamera CreateBlitRequest(Material iMat, RenderTexture iRenderTexture)
        {
            BlitToCamera aBlitRequest = new BlitToCamera()
            {
                RemoveAfterBlit = true,
                Camera = m_Camera,
                RenderAction = (BlitData iBlitData) =>
                {
                    var aCmd = iBlitData.Cmd;
                    var aCameraData = iBlitData.RenderingData.cameraData;

                    iMat.SetMatrix("_ViewToWorld", aCameraData.camera.cameraToWorldMatrix);

                    aCmd.Blit(iBlitData.Renderer.cameraColorTarget, iRenderTexture, iMat, 0);
                },
                RenderPassEvent = UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRenderingPostProcessing,
            };
            URP_BlitRendererFeature.AddBlitRequest(aBlitRequest);
            return aBlitRequest;
        }
        public RenderTexture CaptureImage(int iWidth, int iHeight, Material iMat)
        {
            //RenderTexture aCameraRT = CreateRenderTexture(iWidth, iHeight);
            RenderTexture aRT = CreateRenderTexture(iWidth, iHeight);
            try
            {
                CreateBlitRequest(iMat, aRT);
                
                //m_Camera.targetTexture = aCameraRT;
                //m_Camera.Render();
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                //m_Camera.targetTexture = null;
            }

            return aRT;
        }

        private void Update()
        {
            if(CurCamera != this)
            {
                return;
            }
            bool aCapture = false;
            if (IsAutoCapturing)
            {
                if (s_AutoCaptureSetting.CheckAutoCaptureTime())
                {
                    aCapture = true;
                }
            }

            if (aCapture)
            {
                var aCaptureModes = s_AutoCaptureSetting.m_AutoCaptureModes.Clone();
                if (aCaptureModes.Count == 0) aCaptureModes.Add(s_AutoCaptureInputImage.m_CaptureMode);
                var aSetting = RunTimeData.Ins.CurImgSetting;
                var aImageSetting = s_AutoCaptureInputImage.m_ImageSetting;
                var aFilePaths = CaptureImage(aSetting.m_Width, aSetting.m_Height, ref aImageSetting.Texture, aCaptureModes,
                    s_AutoCaptureSetting.m_SaveAutoCaptureImage);
                if (!aFilePaths.IsNullOrEmpty())
                {
                    var aPath = aFilePaths.LastElement();
                    s_AutoCaptureInputImage.m_LoadImageSetting.SetPath(aPath.Item1, aPath.Item2);
                }
                if (s_CaptureSingleFrame)
                {
                    s_CaptureSingleFrame = false;
                    DisableAutoCapture();
                }
            }
        }
    }
}