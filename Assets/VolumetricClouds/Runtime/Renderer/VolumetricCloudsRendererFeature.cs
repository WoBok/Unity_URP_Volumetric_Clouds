using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using VolumetricClouds.Editor;
#endif

namespace VolumetricClouds.Renderer
{
    public class VolumetricCloudsRendererFeature : ScriptableRendererFeature
    {
        public ComputeShader slicer;
        public ComputeShader copy;
        VolumetricCloudsRenderPass m_ScriptablePass;
        public override void Create()
        {
            m_ScriptablePass = new VolumetricCloudsRenderPass(copy);
            m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            //CreateTexture(ref shapeTexture, shapeResolution, SHAPENOISENAME);
            //CreateTexture(ref detailTexture, detailResolution, DETAILNOISENAME);
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
#if UNITY_EDITOR
            NoiseGenerator.Instance = null;//Todo: 清理资源  
#endif
        }
        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            m_ScriptablePass.SetRenderTarget(renderer.cameraColorTargetHandle);
        }
        public void CreateTexture(ref RenderTexture texture, int resolution, string name)
        {
            var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm;
            if (texture == null || !texture.IsCreated() || texture.width != resolution || texture.height != resolution || texture.volumeDepth != resolution || texture.graphicsFormat != format)
            {
                texture = new RenderTexture(resolution, resolution, 0);
                texture.wrapMode = TextureWrapMode.Repeat;
                texture.filterMode = FilterMode.Bilinear;
                texture.graphicsFormat = format;
                texture.volumeDepth = resolution;
                texture.enableRandomWrite = true;
                texture.dimension = TextureDimension.Tex3D;
                texture.name = name;
                texture.Create();
                NoiseLoader.Load(name, texture, copy);
            }
        }
        void CheckNoise()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            var detailNoiseName = NoiseGenerator.DETAILNOISENAME;
            var shapeNoiseName = NoiseGenerator.SHAPENOISENAME;

            var detailNoiseExists = System.IO.File.Exists($"{Application.dataPath} /Resources/{sceneName}_{detailNoiseName}.asset");
            var shapeNoiseExists = System.IO.File.Exists($"{Application.dataPath} /Resources/{sceneName}_{shapeNoiseName}.asset");

            NoiseSaver creator = null;
            if (!detailNoiseExists || !shapeNoiseExists)
            {
                creator = new NoiseSaver();
                creator.slicer = slicer;
            }
            if (!detailNoiseExists)
            {
                NoiseGenerator.Instance.CreateTexture(ref NoiseGenerator.Instance.detailTexture, NoiseGenerator.Instance.detailResolution, detailNoiseName);
                for (int i = 0; i < 4; i++)
                {
                    NoiseGenerator.Instance.activeChannel = (NoiseGenerator.TextureChannel)i;
                    NoiseGenerator.Instance.activeTextureType = NoiseGenerator.CloudNoiseType.Detail;
                    NoiseGenerator.Instance.UpdateCompute();
                }
                creator.Save(NoiseGenerator.Instance.detailTexture, detailNoiseName);
            }
            if (!shapeNoiseExists)
            {
                NoiseGenerator.Instance.CreateTexture(ref NoiseGenerator.Instance.shapeTexture, NoiseGenerator.Instance.shapeResolution, shapeNoiseName);
                for (int i = 0; i < 4; i++)
                {
                    NoiseGenerator.Instance.activeChannel = (NoiseGenerator.TextureChannel)i;
                    NoiseGenerator.Instance.activeTextureType = NoiseGenerator.CloudNoiseType.Shape;
                    NoiseGenerator.Instance.UpdateCompute();
                }
                creator.Save(NoiseGenerator.Instance.shapeTexture, shapeNoiseName);
            }
        }
    }
}