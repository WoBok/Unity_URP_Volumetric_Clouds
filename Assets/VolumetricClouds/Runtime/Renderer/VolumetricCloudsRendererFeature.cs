using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VolumetricClouds.Renderer
{
    public class NoiseTexture
    {
        public static RenderTexture shapeTexture;
        public static RenderTexture detailTexture;
    }

    public class VolumetricCloudsRendererFeature : ScriptableRendererFeature
    {
        public Action OnDispose;
        VolumetricCloudsRenderPass m_VolumetricCloudsRenderPass;
        public override void Create()
        {
            m_VolumetricCloudsRenderPass = new VolumetricCloudsRenderPass();
            m_VolumetricCloudsRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            var textureInfo = NoiseTextureInfo.Info;
            NoiseTextureLoader.CreateTexture(ref NoiseTexture.shapeTexture, textureInfo.shapeResolution, textureInfo.shapeNoiseName);
            NoiseTextureLoader.CreateTexture(ref NoiseTexture.detailTexture, textureInfo.detailResolution, textureInfo.detailNoiseName);
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
                renderer.EnqueuePass(m_VolumetricCloudsRenderPass);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            NoiseTexture.shapeTexture.Release();
            NoiseTexture.detailTexture.Release();
            OnDispose?.Invoke();
        }
    }
}