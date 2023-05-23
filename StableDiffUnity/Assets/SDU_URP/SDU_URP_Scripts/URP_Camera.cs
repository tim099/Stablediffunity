using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SDU
{
    [UCL.Core.ATTR.EnableUCLEditor]
    public class URP_Camera : MonoBehaviour
    {
        public static URP_Camera CurCamera => s_Cameras.IsNullOrEmpty() ? null : s_Cameras[0];
        public static List<URP_Camera> s_Cameras = new List<URP_Camera>();
        //public static List<RenderTexture> s_RenderTextures = new List<RenderTexture>();
        public RenderTexture m_RT;
        public Texture2D m_Texture;
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
        public BlitToCamera CreateDepthBlitRequest(int iWidth, int iHeight, Material iMat)
        {
            m_RT = RenderTexture.GetTemporary(iWidth, iHeight, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);
            m_RT.antiAliasing = 8;
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

                    int aDesID = iBlitData.GetTemporaryRT(width, height, 0, FilterMode.Point, RenderTextureFormat.Default).id;//s_KeepFrameBuffer;

                    iMat.SetFloat("_Weight", 1f);//depth.weight.value
                    iMat.SetMatrix("_ViewToWorld", aCameraData.camera.cameraToWorldMatrix);

                    aCmd.SetGlobalTexture("_MainTex", aDesID);
                    aCmd.Blit(iBlitData.Renderer.cameraColorTarget, m_RT, iMat, 0);
                },
                RenderPassEvent = UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRenderingPostProcessing,
            };
            URP_BlitRendererFeature.AddBlitRequest(aBlitRequest);
            return aBlitRequest;
        }
        public Texture2D CreateDepthImage(int iWidth, int iHeight)
        {
            return CreateImage(iWidth, iHeight, m_DepthMaterial);
        }
        public Texture2D CreateNormalImage(int iWidth, int iHeight)
        {
            return CreateImage(iWidth, iHeight, m_NormalMaterial);
        }
        public Texture2D CreateImage(int iWidth, int iHeight, Material iMat)
        {
            //var texture = new Texture2D(iWidth, iHeight, TextureFormat.RGB24, false);
            if (m_RT != null)
            {
                RenderTexture.ReleaseTemporary(m_RT);
            }

            try
            {
                m_RT = RenderTexture.GetTemporary(iWidth, iHeight, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);
                m_RT.antiAliasing = 8;
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

                if (m_Texture != null)
                {
                    if(m_Texture.width != iWidth || m_Texture.height != iHeight)
                    {
                        Debug.LogWarning($"Refresh m_Texture m_Texture size:({m_Texture.width},{m_Texture.height}), to: ({iWidth},{iHeight})");
                        GameObject.DestroyImmediate(m_Texture);
                        m_Texture = null;
                    }
                }
                if(m_Texture == null)
                {
                    m_Texture = new Texture2D(iWidth, iHeight, TextureFormat.RGB24, false);
                }
                
                //m_Camera.targetTexture = m_RT;
                m_Camera.Render();
                RenderTexture.active = m_RT;
                m_Texture.ReadPixels(new Rect(0, 0, iWidth, iHeight), 0, 0);
                m_Texture.Apply();
            }
            finally
            {
                //m_Camera.targetTexture = null;
                RenderTexture.ReleaseTemporary(m_RT);
                RenderTexture.active = null;
            }
            //var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);

            //FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
            //var scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];
            //var rendererFeature = ScriptableRendererFeature.CreateInstance<MainRendererFeature>();
            //scriptableRendererData.rendererFeatures.Add(rendererFeature);
            //scriptableRendererData.SetDirty();

            //var volume = m_Volume;
            //URP_DepthVolume depth = null;
            //volume.profile.TryGet<URP_DepthVolume>(out depth);

            //if (depth is null)
            //{
            //    depth = volume.profile.Add<URP_DepthVolume>();
            //}

            //depth.active = true;
            //depth.weight.overrideState = true;
            //depth.weight.value = 1f;
            //var size = new Vector2Int(iWidth, iHeight);
            //var render = new RenderTexture(size.x, size.y, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);
            //render.antiAliasing = 8;
            //var texture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
            //if (m_Camera == null) m_Camera = Camera.main;

            //try
            //{
            //    m_Camera.targetTexture = render;
            //    m_Camera.Render();
            //    RenderTexture.active = render;
            //    texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
            //    texture.Apply();
            //}
            //finally
            //{
            //    m_Camera.targetTexture = null;
            //    RenderTexture.active = null;
            //}

            //volume.profile.Remove<URP_DepthVolume>();
            //scriptableRendererData.rendererFeatures.Remove(rendererFeature);

            return m_Texture;
        }
    }
}