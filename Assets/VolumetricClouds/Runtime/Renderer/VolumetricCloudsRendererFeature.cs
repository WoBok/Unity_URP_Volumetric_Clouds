using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using VolumetricClouds.Editor;
#endif

namespace VolumetricClouds.Renderer
{
    public class VolumetricCloudsRendererFeature : ScriptableRendererFeature
    {
        public ComputeShader copy;
        VolumetricCloudsRenderPass m_VolumetricCloudsRenderPass;

        public override void Create()
        {
            m_VolumetricCloudsRenderPass = new VolumetricCloudsRenderPass();
            m_VolumetricCloudsRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_VolumetricCloudsRenderPass);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
#if UNITY_EDITOR
            NoiseTextureGenerator.Instance = null;//Todo: 清理资源  
#endif
        }
    }
}