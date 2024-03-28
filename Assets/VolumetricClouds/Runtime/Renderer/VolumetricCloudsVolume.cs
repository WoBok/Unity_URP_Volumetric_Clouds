using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VolumetricClouds.Renderer
{
#if UNITY_2023_1_OR_NEWER
[Serializable, VolumeComponentMenu("WoBok/Volumetric Clouds"), SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
#else
    [Serializable, VolumeComponentMenuForRenderPipeline("WoBok/Volumetric Clouds", typeof(UniversalRenderPipeline))]
#endif
    public class VolumetricCloudsVolume : VolumeComponent, IPostProcessComponent
    {
        [Header("March Settings")]
        public ClampedIntParameter numStepsLight = new(20, 0, 20);
        public ClampedFloatParameter rayOffsetStrength = new(10, 0, 15);
        public TextureParameter blueNoise = new(null);

        [Header("Base Shape")]
        public ClampedFloatParameter cloudScale = new(1, 0, 100);
        public ClampedFloatParameter densityMultiplier = new(1, 0, 100);
        public ClampedFloatParameter densityOffset = new(-1, -100, 100);
        public Vector4Parameter shapeNoiseWeights = new(new Vector4(6, 0.5f, 1, 2));
        public Vector3Parameter shapeOffset = new(new Vector3(2, 0, 0));
        public Vector2Parameter heightOffset = new(new Vector2(0, 0));

        [Header("Detail")]
        public ClampedFloatParameter detailNoiseScale = new(2, 1, 100);
        public ClampedFloatParameter detailNoiseWeight = new(5, 1, 100);
        public Vector3Parameter detailNoiseWeights = new(new Vector3(15, 0.5f, 0.5f));
        public Vector3Parameter detailOffset = new(new Vector3(5, 0, 0));

        [Header("Lighting")]
        public ClampedFloatParameter lightAbsorptionThroughCloud = new(1, 0, 100);
        public ClampedFloatParameter lightAbsorptionTowardSun = new(2, 0, 100);
        [Range(0, 1)]
        public ClampedFloatParameter darknessThreshold = new(0.2f, 0, 1);
        [Range(0, 1)]
        public ClampedFloatParameter forwardScattering = new(0.8f, 0, 1);
        [Range(0, 1)]
        public ClampedFloatParameter backScattering = new(0.3f, 0, 1);
        [Range(0, 1)]
        public ClampedFloatParameter baseBrightness = new(0.8f, 0, 1);
        [Range(0, 1)]
        public ClampedFloatParameter phaseFactor = new(0.15f, 0, 1);

        [Header("Animation")]
        public ClampedFloatParameter timeScale = new(1, 0, 100);
        public ClampedFloatParameter baseSpeed = new(1, 0, 100);
        public ClampedFloatParameter detailSpeed = new(1, 0, 100);

        [Header("Transform")]
        public Vector3Parameter position = new(new Vector3(1000, 300, 100));
        public Vector3Parameter scale = new(Vector3.one * 2000);

        public Vector3Parameter cloudTestParams = new(Vector3.zero);
        public bool IsActive() => blueNoise != null;
        public bool IsTileCompatible() => false;
    }
}