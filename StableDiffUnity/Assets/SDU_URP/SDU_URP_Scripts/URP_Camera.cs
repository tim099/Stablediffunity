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
        public enum CaptureMode
        {
            Depth,
            Normal,
        }

        public static URP_Camera CurCamera => s_Cameras.IsNullOrEmpty() ? null : s_Cameras[0];
        public static List<URP_Camera> s_Cameras = new List<URP_Camera>();
        //public static List<RenderTexture> s_RenderTextures = new List<RenderTexture>();
        public RenderTexture m_RT;

        public Camera m_Camera;
        public Volume m_Volume;
        public Material m_DepthMaterial;
        public Material m_NormalMaterial;
        private void Start()
        {
            if(m_Camera == null) m_Camera = GetComponent<Camera>();
            m_Camera.enabled = true;
            s_Cameras.Add(this);
        }
        private void OnDestroy()
        {
            s_Cameras.Remove(this);
        }
        [UCL.Core.ATTR.UCL_FunctionButton]
        public void Test()
        {
            if(s_Cameras == null) return;
            if(s_Cameras.Contains(this)) return;
            s_Cameras.Add(this);
        }
        public Texture2D CaptureImage(int iWidth, int iHeight, ref Texture2D iTexture, CaptureMode iCaptureMode)
        {
            switch(iCaptureMode)
            {
                case CaptureMode.Depth:
                    {
                        return CaptureImage(iWidth, iHeight, m_DepthMaterial, ref iTexture);
                    }
                    case CaptureMode.Normal:
                    {
                        return CaptureImage(iWidth, iHeight, m_NormalMaterial, ref iTexture);
                    }
            }
            return CaptureImage(iWidth, iHeight, m_DepthMaterial, ref iTexture);
        }
        public Texture2D CaptureImage(int iWidth, int iHeight, Material iMat, ref Texture2D iTexture)
        {
            //Debug.LogWarning($"CaptureImage iWidth:{iWidth},iHeight:{iHeight}.iMat:{iMat.name}");
            //var texture = new Texture2D(iWidth, iHeight, TextureFormat.RGB24, false);
            if (m_RT != null)
            {
                RenderTexture.ReleaseTemporary(m_RT);
            }
            RenderTexture aRenderTarget = null;
            try
            {
                m_RT = RenderTexture.GetTemporary(iWidth, iHeight, 24, GraphicsFormat.R32G32B32A32_SFloat);
                aRenderTarget = RenderTexture.GetTemporary(iWidth, iHeight, 24, GraphicsFormat.R32G32B32A32_SFloat);
                //m_RT.antiAliasing = 8;
                var aBlitRequest = new BlitToCamera()
                {
                    RemoveAfterBlit = true,
                    Camera = m_Camera,
                    RenderAction = (BlitData iBlitData) =>
                    {
                        var aCmd = iBlitData.Cmd;
                        var aCameraData = iBlitData.RenderingData.cameraData;
                        int width = aCameraData.cameraTargetDescriptor.width;
                        int height = aCameraData.cameraTargetDescriptor.height;

                        //int aDesID = iBlitData.GetTemporaryRT(width, height, 0, FilterMode.Point, RenderTextureFormat.Default).id;//s_KeepFrameBuffer;

                        //iMat.SetFloat("_Weight", 1f);//depth.weight.value
                        iMat.SetMatrix("_ViewToWorld", aCameraData.camera.cameraToWorldMatrix);

                        //aCmd.SetGlobalTexture("_MainTex", aDesID);
                        aCmd.Blit(iBlitData.Renderer.cameraColorTarget, m_RT, iMat, 0);
                    },
                    RenderPassEvent = UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRenderingPostProcessing,
                };
                URP_BlitRendererFeature.AddBlitRequest(aBlitRequest);

                var aFormat = TextureFormat.RGB24;
                if (iTexture != null)
                {
                    if(iTexture.width != iWidth || iTexture.height != iHeight
                        || iTexture.format != aFormat)
                    {
                        Debug.LogWarning($"Refresh m_Texture m_Texture size:" +
                            $"({iTexture.width},{iTexture.height}) Format:{iTexture.format}" +
                            $", to: ({iWidth},{iHeight}) Format:{aFormat}");
                        GameObject.DestroyImmediate(iTexture);
                        iTexture = null;
                    }
                }
                if(iTexture == null)
                {
                    iTexture = new Texture2D(iWidth, iHeight, aFormat, false);
                }
                
                m_Camera.targetTexture = aRenderTarget;
                m_Camera.Render();
                RenderTexture.active = m_RT;
                iTexture.ReadPixels(new Rect(0, 0, iWidth, iHeight), 0, 0);
                iTexture.Apply();
            }
            finally
            {
                m_Camera.targetTexture = null;
                //RenderTexture.ReleaseTemporary(m_RT);
                if(aRenderTarget != null) RenderTexture.ReleaseTemporary(aRenderTarget);
                RenderTexture.active = null;
            }

            return iTexture;
        }
    }
}