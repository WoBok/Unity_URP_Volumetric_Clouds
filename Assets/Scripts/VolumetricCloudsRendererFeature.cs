using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricCloudsRendererFeature : ScriptableRendererFeature
{
    class VolumetricCloudsRenderPass : ScriptableRenderPass
    {
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    VolumetricCloudsRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new VolumetricCloudsRenderPass();
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}