using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VolumetricClouds.Renderer
{
    class VolumetricCloudsRenderPass : ScriptableRenderPass
    {
        Material m_CloudsMaterial;
        VolumetricCloudsVolume m_Volume;

        RTHandle m_CameraColorTargetHandle;
        int m_TempTarget = Shader.PropertyToID("TempTarget");

        RenderTexture shapeTexture;
        RenderTexture detailTexture;

        public int shapeResolution = 128;
        public int detailResolution = 64;
        public const string DETAILNOISENAME = "DetailNoise";
        public const string SHAPENOISENAME = "ShapeNoise";
        public VolumetricCloudsRenderPass()
        {
            m_CloudsMaterial = new Material(Shader.Find("Hidden/Clouds"));
            m_Volume = VolumeManager.instance.stack.GetComponent<VolumetricCloudsVolume>();

            CreateTexture(ref shapeTexture, shapeResolution, SHAPENOISENAME);
            CreateTexture(ref detailTexture, detailResolution, DETAILNOISENAME);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!m_Volume.IsActive()) return;

            UpdateMaterial();

            var descriptor = renderingData.cameraData.cameraTargetDescriptor;

            m_CameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

            CommandBuffer cmd = CommandBufferPool.Get("Clouds");
            cmd.Clear();
            cmd.GetTemporaryRT(m_TempTarget, descriptor, FilterMode.Bilinear);
            cmd.Blit(m_CameraColorTargetHandle, m_TempTarget);
            cmd.Blit(m_TempTarget, m_CameraColorTargetHandle, m_CloudsMaterial);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_TempTarget);
        }
        void UpdateMaterial()
        {
            if (!m_CloudsMaterial) return;

            m_CloudsMaterial.SetTexture("NoiseTex", shapeTexture);
            m_CloudsMaterial.SetTexture("DetailNoiseTex", detailTexture);
            m_CloudsMaterial.SetTexture("BlueNoise", m_Volume.blueNoise.value);
            m_CloudsMaterial.SetFloat("scale", m_Volume.cloudScale.value);
            m_CloudsMaterial.SetFloat("densityMultiplier", m_Volume.densityMultiplier.value);
            m_CloudsMaterial.SetFloat("densityOffset", m_Volume.densityOffset.value);
            m_CloudsMaterial.SetFloat("lightAbsorptionThroughCloud", m_Volume.lightAbsorptionThroughCloud.value);
            m_CloudsMaterial.SetFloat("lightAbsorptionTowardSun", m_Volume.lightAbsorptionTowardSun.value);
            m_CloudsMaterial.SetFloat("darknessThreshold", m_Volume.darknessThreshold.value);
            m_CloudsMaterial.SetFloat("rayOffsetStrength", m_Volume.rayOffsetStrength.value);
            m_CloudsMaterial.SetFloat("detailNoiseScale", m_Volume.detailNoiseScale.value);
            m_CloudsMaterial.SetFloat("detailNoiseWeight", m_Volume.detailNoiseWeight.value);
            m_CloudsMaterial.SetVector("shapeOffset", m_Volume.shapeOffset.value);
            m_CloudsMaterial.SetVector("detailOffset", m_Volume.detailOffset.value);
            m_CloudsMaterial.SetVector("detailWeights", m_Volume.detailNoiseWeights.value);
            m_CloudsMaterial.SetVector("shapeNoiseWeights", m_Volume.shapeNoiseWeights.value);
            m_CloudsMaterial.SetVector("phaseParams", new Vector4(m_Volume.forwardScattering.value, m_Volume.backScattering.value, m_Volume.baseBrightness.value, m_Volume.phaseFactor.value));
            m_CloudsMaterial.SetVector("boundsMin", m_Volume.position.value - m_Volume.scale.value / 2);
            m_CloudsMaterial.SetVector("boundsMax", m_Volume.position.value + m_Volume.scale.value / 2);
            m_CloudsMaterial.SetInt("numStepsLight", m_Volume.numStepsLight.value);
            m_CloudsMaterial.SetVector("mapSize", new Vector4(Mathf.CeilToInt(m_Volume.scale.value.x), Mathf.CeilToInt(m_Volume.scale.value.y), Mathf.CeilToInt(m_Volume.scale.value.z), 0));
            m_CloudsMaterial.SetFloat("timeScale", (Application.isPlaying) ? m_Volume.timeScale.value : 0);
            m_CloudsMaterial.SetFloat("baseSpeed", m_Volume.baseSpeed.value);
            m_CloudsMaterial.SetFloat("detailSpeed", m_Volume.detailSpeed.value);
        }
        public void UpdateMaterialNoise()
        {

        }
        void CreateTexture(ref RenderTexture texture, int resolution, string name)
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
                NoiseTextureLoader.Load(name, texture);
            }
        }
    }
}