using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SDU
{
    #region BlitRequest

    public class BlitRequest
    {
        public bool RemoveAfterBlit = true;
        public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRendering;
        public Camera Camera = null;
        public BlitRequest() { }
        public virtual void Blit(BlitData blitData)
        {

        }
        public virtual void FrameCleanup(CommandBuffer cmd) { }
    }
    public class BlitToRenderTexture : BlitRequest
    {
        public BlitToRenderTexture() { }

        public override void Blit(BlitData blitData)
        {
            //Eagle.Log.Warning("blitShaderPassIndex:" + blitShaderPassIndex);
            RenderTexture = RenderTexture.GetTemporary(blitData.OpaqueDesc);
            blitData.BlitPass.Blit(blitData.Cmd, blitData.Source, RenderTexture, blitData.Material, blitData.ShaderPassIndex);

            CompleteCallback?.Invoke(this);
        }
        /// <summary>
        /// blit to renderTexture
        /// </summary>
        public RenderTexture RenderTexture = null;
        public System.Action<BlitToRenderTexture> CompleteCallback = null;
    }

    public class BlitToCamera : BlitRequest
    {
        public BlitToCamera()
        {

        }
        public override void Blit(BlitData blitData)
        {
            if (RenderAction == null) return;
            _blitData = blitData;
            RenderAction.Invoke(blitData);
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (_blitData != null)
            {
                _blitData.FrameCleanup(cmd);
            }
        }
        BlitData _blitData;
        public System.Action<BlitData> RenderAction = null;
    }

    #endregion
    /// <summary>
    /// reference https://zhuanlan.zhihu.com/p/367518645
    /// this was used on https://gamedevbill.com, but originally taken from https://cyangamedev.wordpress.com/2020/06/22/urp-post-processing/
    /// </summary>
    public class URP_BlitRendererFeature : ScriptableRendererFeature
    {
        #region static
        public static void AddBlitRequest(BlitRequest blitRequest)
        {
            s_BlitRequests.Add(blitRequest);
        }
        public static void RemoveBlitRequest(BlitRequest blitRequest)
        {
            s_BlitRequests.Remove(blitRequest);
        }
        static readonly List<BlitRequest> s_BlitRequests = new List<BlitRequest>();
        #endregion

        [System.Serializable]
        public class BlitSettings
        {
            public Material m_BlitMaterial = null;
            public int m_BlitMaterialPassIndex = 0;
        }

        public BlitSettings m_Settings = new BlitSettings();
        readonly Dictionary<RenderPassEvent, URP_BlitPass> m_BlitPassDic = new Dictionary<RenderPassEvent, URP_BlitPass>();
        public override void Create()
        {
            var passIndex = m_Settings.m_BlitMaterial != null ? m_Settings.m_BlitMaterial.passCount - 1 : 1;
            m_Settings.m_BlitMaterialPassIndex = Mathf.Clamp(m_Settings.m_BlitMaterialPassIndex, -1, passIndex);
        }
        private URP_BlitPass GetBlitPass(RenderPassEvent renderPassEvent)
        {

            if (!m_BlitPassDic.ContainsKey(renderPassEvent))
            {
                m_BlitPassDic.Add(renderPassEvent, new URP_BlitPass(renderPassEvent, m_Settings.m_BlitMaterial,
                    m_Settings.m_BlitMaterialPassIndex, $"{name}_{renderPassEvent.ToString()}"));
            }
            return m_BlitPassDic[renderPassEvent];
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            //renderingData.ConfigureInput(ScriptableRenderPassInput.Normal);
            if (s_BlitRequests.Count == 0)
            {
                return;
            }

            var targetCamera = renderingData.cameraData.camera;
            for (int i = s_BlitRequests.Count - 1; i >= 0; i--)
            {
                BlitRequest blitRequest = s_BlitRequests[i];
                if (blitRequest.Camera == targetCamera)
                {
                    GetBlitPass(blitRequest.RenderPassEvent).AddBlitRequest(blitRequest);
                    if (blitRequest.RemoveAfterBlit) s_BlitRequests.RemoveAt(i);
                }
            }

            //var src = renderer.cameraColorTarget;
            foreach (var blitPass in m_BlitPassDic.Values)
            {
                if (blitPass.HasRequests)
                {
                    blitPass.Setup(renderer);
                    renderer.EnqueuePass(blitPass);
                }
            }

        }

        protected override void Dispose(bool iDisposing)
        {
            base.Dispose(iDisposing);
        }
    }
}
