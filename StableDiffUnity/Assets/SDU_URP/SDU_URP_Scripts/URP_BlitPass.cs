using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace SDU
{
    public class BlitData
    {
        public int ID { get; set; }
        public Camera Camera;
        public URP_BlitPass BlitPass;
        public RenderTargetIdentifier Source;
        public CommandBuffer Cmd;
        public RenderTextureDescriptor OpaqueDesc;
        public Material Material;
        public int ShaderPassIndex = 0;
        public ScriptableRenderer Renderer;

        public List<RenderTargetHandle> _temporaryColorTextures = new List<RenderTargetHandle>();
        public List<RenderTexture> _temporaryRenderTextures = new List<RenderTexture>();
        public RenderTargetHandle GetTemporaryRT(FilterMode filterMode)
        {
            RenderTargetHandle targetHandle = new RenderTargetHandle();
            targetHandle.Init($"TemporaryRT_{ID.ToString()}_{_temporaryColorTextures.Count}");
            _temporaryColorTextures.Add(targetHandle);
            Cmd.GetTemporaryRT(targetHandle.id, OpaqueDesc, filterMode);
            return targetHandle;
        }
        public RenderTexture GetTemporaryRenderTexture(FilterMode filterMode)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(OpaqueDesc);
            renderTexture.filterMode = filterMode;
            _temporaryRenderTextures.Add(renderTexture);
            return renderTexture;
        }
        public void FrameCleanup(CommandBuffer cmd)
        {
            foreach (var temporaryColorTexture in _temporaryColorTextures)
            {
                cmd.ReleaseTemporaryRT(temporaryColorTexture.id);
            }
            foreach (var temporaryRenderTexture in _temporaryRenderTextures)
            {
                RenderTexture.ReleaseTemporary(temporaryRenderTexture);
            }
        }
    }
    /// <summary>
    /// reference https://zhuanlan.zhihu.com/p/367518645
    /// this was used on https://gamedevbill.com, but originally taken from https://cyangamedev.wordpress.com/2020/06/22/urp-post-processing/
    /// </summary>
    public class URP_BlitPass : ScriptableRenderPass
    {
        public ScriptableRenderer _renderer;
        public Material _blitMaterial = null;
        public int _blitShaderPassIndex = 0;
        private RenderTargetIdentifier source { get; set; }
        public bool HasRequests => _blitRequests.Count > 0;
        string _profilerTag;
        List<BlitRequest> _blitRequests = new List<BlitRequest>();
        public URP_BlitPass(RenderPassEvent renderPassEvent, Material blitMaterial, int blitShaderPassIndex, string tag)
        {
            this.renderPassEvent = renderPassEvent;
            this._blitMaterial = blitMaterial;
            this._blitShaderPassIndex = blitShaderPassIndex;
            _profilerTag = tag;
        }

        public void Setup(ScriptableRenderer renderer)
        {
            this._renderer = renderer;
            this.source = renderer.cameraColorTarget;
        }
        public void AddBlitRequest(BlitRequest blitRequest)
        {
            if (_blitRequests == null) _blitRequests = new List<BlitRequest>();
            _blitRequests.Add(blitRequest);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            for (int i = 0; i < _blitRequests.Count; i++)
            {
                var blitRequest = _blitRequests[i];
                BlitData blitData = new BlitData()
                {
                    ID = i,
                    Camera = blitRequest.Camera,
                    BlitPass = this,
                    Source = source,
                    Cmd = cmd,
                    OpaqueDesc = opaqueDesc,
                    Material = this._blitMaterial,
                    ShaderPassIndex = this._blitShaderPassIndex,
                    Renderer = this._renderer,
                };
                blitRequest.Blit(blitData);
            }
            _blitRequests.Clear();
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            for (int i = 0; i < _blitRequests.Count; i++)
            {
                var blitRequest = _blitRequests[i];
                blitRequest.FrameCleanup(cmd);
            }
        }
    }
}